using System.Collections.Immutable;
using System.Timers;
using Microsoft.Extensions.Logging;
using Results.Contract;
using Results.Model;

namespace Results;

// ReSharper disable CommentTypo
// TODO: Lägg till baspoäng
// TODO: Lägg till export av resultat per klubb och per löpare
// ReSharper restore CommentTypo

public sealed class ResultService : IResultService, IDisposable
{
    private IList<TeamResult> latestTeamResults = ImmutableList<TeamResult>.Empty;
    private int latestResultsHash;
    private Statistics latestStatistics = new();
    private readonly Configuration configuration;
    private readonly ILogger<ResultService> logger;
    private readonly IPointsCalc pointsCalc;
    private readonly System.Timers.Timer timer;
    private readonly IResultSource resultSource;
    private readonly ITeamService teamService;
    private static readonly SemaphoreSlim NewResultPostSemaphore = new(1, 1);

    public ResultService(Configuration configuration, IResultSource resultSource, ITeamService teamService, ILogger<ResultService> logger)
    {
        this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        this.resultSource = resultSource ?? throw new ArgumentNullException(nameof(resultSource));
        this.teamService = teamService ?? throw new ArgumentNullException(nameof(teamService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

        pointsCalc = configuration.IsFinal 
            ? new PointsCalcFinal(teamService, configuration) 
            : new PointsCalcNormal(teamService, configuration);

        GetResult();
        timer = new System.Timers.Timer(TimeSpan.FromSeconds(2).TotalMilliseconds); // TODO: Synka med MeOS post av ny data
        timer.Elapsed += OnTimedEvent;
        timer.AutoReset = true;
        timer.Enabled = true;
    }

    private void OnTimedEvent(object? sender, ElapsedEventArgs e)
    {
        GetResult();
    }

    private void GetResult()
    {
        try
        {
            var participantResults = FilterTeams(resultSource.GetParticipantResults());

            var notStartedCutOff = resultSource.CurrentTimeOfDay - configuration.TimeUntilNotStated;

            foreach (var pr in participantResults)
            {
                switch (pr.Status) {
                    case ParticipantStatus.NotActivated:
                    if (pr.StartTime < notStartedCutOff)
                            pr.Status = ParticipantStatus.NotStarted;
                        break;

                    case ParticipantStatus.Activated:
                        if (pr.StartTime < resultSource.CurrentTimeOfDay)
                            pr.Status = ParticipantStatus.Started;
                        break;
                }
            }

            var teamResults = pointsCalc.CalcScoreBoard(resultSource.CurrentTimeOfDay, participantResults);
            var statistics = Statistics.GetStatistics(participantResults, resultSource.CurrentTimeOfDay, configuration); // TODO: Uppdatera resultat om denna ändras.
            var resultsHash = CalcHasCode(teamResults, statistics);

            if (resultsHash == latestResultsHash) return;

            latestTeamResults = teamResults;
            latestStatistics = statistics;
            latestResultsHash = resultsHash;

            OnNewResults?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"{nameof(OnTimedEvent)}");
        }
    }

    private IList<ParticipantResult> FilterTeams(IList<ParticipantResult> participantResults)
    {
        return teamService.Teams == null 
            ? participantResults 
            : participantResults.Where(pr => teamService.Teams.Contains(pr.Club)).ToList();
    }

    public bool SupportsPreliminary => resultSource.SupportsPreliminary;

    public Result GetScoreBoard()
    {
        latestStatistics.LastUpdatedTimeOfDay = resultSource.CurrentTimeOfDay;
        var result = new Result(latestTeamResults, latestStatistics);
        return result;
    }

    public event EventHandler? OnNewResults;

    public async Task<string> NewResultPostAsync(Stream body, DateTime timestamp)
    {
        await NewResultPostSemaphore.WaitAsync().ConfigureAwait(true);
        try
        {
            string status = await resultSource.NewResultPostAsync(body, timestamp).ConfigureAwait(true);
            GetResult();
            return status;
        }
        finally
        {
            NewResultPostSemaphore.Release();
        }
    }

    public IEnumerable<ParticipantPoints> GetParticipantPointsList()
    {
        return pointsCalc.GetParticipantPoints(resultSource.CurrentTimeOfDay, resultSource.GetParticipantResults());
    }

    private static int CalcHasCode(IEnumerable<TeamResult> results, Statistics statistics)
    {
        var hashCode = statistics.GetHashCode();

        foreach (var result in results)
        {
            unchecked
            {
                hashCode += result.GetHashCode();
            }
        }
        return hashCode;
    }

    public void Dispose()
    {
        timer.Dispose();
        resultSource.Dispose();
    }
}