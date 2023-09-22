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
    private readonly PointsCalcBase pointsCalc;
    private readonly System.Timers.Timer timer;
    private readonly IResultSource resultSource;
    private readonly ITeamService teamService;

    public ResultService(Configuration configuration, IResultSource resultSource, ITeamService teamService, ILogger<ResultService> logger)
    {
        this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        this.resultSource = resultSource ?? throw new ArgumentNullException(nameof(resultSource));
        this.teamService = teamService ?? throw new ArgumentNullException(nameof(teamService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

        pointsCalc = configuration.IsFinal 
            ? new PointsCalcFinal(teamService.GetTeamBasePoints(), configuration) 
            : new PointsCalcNormal(teamService.GetTeamBasePoints(), configuration);

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
            var participantResults = FilterTeams(resultSource.GetParticipantResults());
            var teamResults = pointsCalc.CalcScoreBoard(resultSource.CurrentTimeOfDay, participantResults);
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

    public Task<string> NewResultPostAsync(Stream body, DateTime timestamp)
    {
        return resultSource.NewResultPostAsync(body, timestamp);
    }

    public IEnumerable<ParticipantPoints> GetParticipantPointsList()
    {
        return pointsCalc.GetParticipantPoints(resultSource.CurrentTimeOfDay, resultSource.GetParticipantResults());
    }

    private static int CalcHasCode(IEnumerable<TeamResult> results)
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