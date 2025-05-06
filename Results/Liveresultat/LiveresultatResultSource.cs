using Microsoft.Extensions.Logging;
using Results.Contract;
using Results.Liveresultat.Model;
using Results.Model;

namespace Results.Liveresultat;

public sealed class LiveresultatResultSource : IResultSource
{
    private readonly LiveresultatFacade liveresultatFacade;
    private readonly ILogger<LiveresultatResultSource> logger;
    private readonly ClassFilter classFilter;
    private readonly int competitionId;
    private TimeSpan lastCurrentTimeOfDay = TimeSpan.Zero;
    private bool competitionFound;

    public LiveresultatResultSource(Configuration configuration, LiveresultatFacade liveresultatFacade, ILogger<LiveresultatResultSource> logger, ClassFilter classFilter)
    {
        if (configuration is null) throw new ArgumentNullException(nameof(configuration));
        this.liveresultatFacade = liveresultatFacade ?? throw new ArgumentNullException(nameof(liveresultatFacade));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.classFilter = classFilter ?? throw new ArgumentNullException(nameof(classFilter));

        if (!configuration.LiveresultatId.HasValue) throw new InvalidOperationException("LiveresultatId is missing");
        competitionId = configuration.LiveresultatId.Value;
    }

    public bool SupportsPreliminary => false;

    public IList<ParticipantResult> GetParticipantResults()
    {
        return GetParticipantResultsAsync().GetAwaiter().GetResult();
    }

    private async Task<IList<ParticipantResult>> GetParticipantResultsAsync()
    {
        if (!await CheckCompetitionInfo().ConfigureAwait(false)) return [];

        var classList = await liveresultatFacade.GetClassesAsync(competitionId).ConfigureAwait(false);
        if (classList?.Classes is null) return [];

        // TODO: fråga inte vare gång
        var classNames = classList.Classes
            .Where(c => !string.IsNullOrEmpty(c.ClassName) && classFilter.IsIncluded(c.ClassName))
            .Select(c => c.ClassName!);

        var results = new List<(string className, PersonResult personResult)>();
        foreach (var className in classNames)
        {
            var classResult = await liveresultatFacade.GetClassResultAsync(competitionId, className).ConfigureAwait(false);
            if (classResult?.Results is null) continue;
            results.AddRange(classResult.Results!.Select(r => (className, r)));
        }

        var ret = results.Select(r =>
            {
                (string className, PersonResult personResult) = r;
                return new ParticipantResult(
                    className,
                    personResult.Name!,
                    personResult.Club ?? string.Empty,
                    personResult.StartTime,
                    personResult.Time,
                    MapStatus(personResult.Status));
            })
            .ToList();
        TimeSpan? maxTime = ret
            .Where(r => r.Time.HasValue && r.Time > TimeSpan.Zero && r.Time.Value < TimeSpan.FromHours(3))
            .Max(r => r.StartTime + r.Time);
        if (maxTime.HasValue && maxTime.Value > lastCurrentTimeOfDay) lastCurrentTimeOfDay = maxTime.Value;
        return ret;
    }

    private async Task<bool> CheckCompetitionInfo()
    {
        if (competitionFound) return true;
        var competitionInfo = await liveresultatFacade.GetCompetitionInfoAsync(competitionId).ConfigureAwait(false);
        if (competitionInfo?.Name is null)
        {
            logger.LogError("Competition with LiveresultatId {LiveresultatId} not found in Liveresultat", competitionId);
            return false;
        }
        competitionFound = true;
        return true;
    }

    private static ParticipantStatus MapStatus(Status personResultStatus)
    {
        // TODO: Hur kan man sätta Activated?
        return personResultStatus switch
        {
            Status.OK => 
                ParticipantStatus.Passed,
            Status.DNS or Status.WalkOver or Status.MovedUp => 
                ParticipantStatus.NotStarted,
            Status.DNF or Status.MP or Status.DSQ or Status.OT => 
                ParticipantStatus.NotValid,
            Status.NotStartedYet1 or Status.NotStartedYet2 => 
                ParticipantStatus.NotActivated,
            _ => 
                ParticipantStatus.Ignored
        };
    }

    public TimeSpan CurrentTimeOfDay
    {
        get
        {
            var lastPassingList = liveresultatFacade.GetLastPassingListAsync(competitionId).GetAwaiter().GetResult();
            var hasData = lastPassingList?.Passings?.Any() ?? false;
            if (!hasData) return lastCurrentTimeOfDay;
            var currentTime = lastPassingList!.Passings!
                .Select(p => p.PassTime)
                .Max();
            if (currentTime > lastCurrentTimeOfDay ) lastCurrentTimeOfDay = currentTime.Value;
            return lastCurrentTimeOfDay;
        }
    }

    public Task<string> NewResultPostAsync(Stream body, DateTime timestamp)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        liveresultatFacade.Dispose();
    }
}