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
    private int latestTeamResultsHash;
    private Statistics latestStatistics = new();
    private readonly Configuration configuration;
    private readonly ILogger<ResultService> logger;
    private readonly PointsCalc pointsCalc;
    private readonly System.Timers.Timer timer;
    private readonly IResultSource resultSource;

    public ResultService(Configuration configuration, IResultSource resultSource, IBasePointsService basePointsService, ILogger<ResultService> logger)
    {
        if (basePointsService == null) throw new ArgumentNullException(nameof(basePointsService));
        this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        this.resultSource = resultSource ?? throw new ArgumentNullException(nameof(resultSource));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

        pointsCalc = new PointsCalc(basePointsService.GetBasePoints(), configuration, resultSource);

        GetResult();
        timer = new System.Timers.Timer(TimeSpan.FromSeconds(2).TotalMilliseconds);
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
            IList<ParticipantResult> participantResults = resultSource.GetParticipantResults();
            var teamResults = pointsCalc.CalcScoreBoard(participantResults);
            var teamResultsHash = CalcHasCode(teamResults);

            if (teamResultsHash == latestTeamResultsHash) return;

            latestTeamResults = teamResults;
            latestTeamResultsHash = teamResultsHash;
            latestStatistics = Statistics.GetStatistics(participantResults, resultSource.CurrentTimeOfDay, configuration);

            OnNewResults?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"{nameof(OnTimedEvent)}");
        }
    }

    public bool SupportsPreliminary => resultSource.SupportsPreliminary;

    public Result GetScoreBoard()
    {
        latestStatistics.LastUpdatedTimeOfDay = resultSource.CurrentTimeOfDay;
        var result = new Result(latestTeamResults, latestStatistics);
        return result;
    }

    public event EventHandler? OnNewResults;
    public Task<string> NewResultPostAsync(Stream body, DateTime timestamp)
    {
        return resultSource.NewResultPostAsync(body, timestamp);
    }

    private static int CalcHasCode(IList<TeamResult> results)
    {
        var hashCode = 0;

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
