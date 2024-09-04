using Results.Contract;
using Results.Liveresultat.Model;
using Results.Model;

namespace Results.Liveresultat;

public sealed class LiveresultatResultSource : IResultSource
{
    private readonly Configuration configuration;
    private readonly LiveresultatFacade liveresultatFacade;
    private readonly int competitionId;
    private TimeSpan lastCurrentTimeOfDay = TimeSpan.Zero;

    public LiveresultatResultSource(Configuration configuration, LiveresultatFacade liveresultatFacade)
    {
        this.configuration = configuration;
        this.liveresultatFacade = liveresultatFacade;
        if (configuration is null) throw new ArgumentNullException(nameof(configuration));
        if (!configuration.LiveresultatComp.HasValue) throw new InvalidOperationException("LiveresultatComp is missing");
        competitionId = configuration.LiveresultatComp.Value;
    }

    public bool SupportsPreliminary => false;

    public IList<ParticipantResult> GetParticipantResults()
    {
        return GetParticipantResultsAsync().GetAwaiter().GetResult();
    }

    private async Task<IList<ParticipantResult>> GetParticipantResultsAsync()
    {
        var competitionInfo = await liveresultatFacade.GetCompetitionInfoAsync(competitionId).ConfigureAwait(false);
        if (competitionInfo is null) return new List<ParticipantResult>();

        var classList = await liveresultatFacade.GetClassesAsync(competitionId).ConfigureAwait(false);
        if (classList?.Classes is null) return new List<ParticipantResult>();

        var classNames = classList.Classes
            .Where(c => !string.IsNullOrEmpty(c.ClassName))
            .Select(c => c.ClassName!);

        var results = new List<(string className, PersonResult personResult)>();
        foreach (var className in classNames)
        {
            var classResult = await liveresultatFacade.GetClassResultAsync(competitionId, className).ConfigureAwait(false);
            if (classResult?.Results is null) continue;
            results.AddRange(classResult!.Results!.Select(r => (className!, r)));
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
        return ret;
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
                throw new ArgumentOutOfRangeException(nameof(personResultStatus), personResultStatus, null)
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
            if (currentTime.HasValue) lastCurrentTimeOfDay = currentTime.Value;
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