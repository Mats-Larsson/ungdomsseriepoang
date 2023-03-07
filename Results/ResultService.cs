using System.Collections.Immutable;
using System.Timers;
using Microsoft.Extensions.Logging;
using Results.Contract;
using Results.Meos;
using Results.Model;
using Results.Ola;
using Results.Simulator;

namespace Results;

public sealed class ResultService : IResultService, IDisposable
{
    private IList<TeamResult> latestTeamResults = ImmutableList<TeamResult>.Empty;
    private int latestTeamResultsHash;
    private Statistics latestStatistics = new();
    private readonly Configuration configuration;
    private readonly ILogger logger;
    private readonly PointsCalc pointsCalc;
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly System.Timers.Timer timer;
    private readonly IResultSource resultSource;

    public ResultService(Configuration configuration, ILogger<ResultService> logger)
    {
        this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        this.logger = logger;

        resultSource = configuration!.ResultSource switch
        {
            ResultSource.OlaDatabase => new OlaResultSource(configuration),
            ResultSource.Simulator => new SimulatorResultSource(configuration),
            ResultSource.Meos => new MeosResultSource(),
            _ => throw new ArgumentException($"Unknown {nameof(resultSource)}: {resultSource}")
        };

        this.configuration = new Configuration();
        this.pointsCalc = new PointsCalc();

        timer = new System.Timers.Timer(TimeSpan.FromSeconds(2).TotalMilliseconds);
        timer.Elapsed += OnTimedEvent;
        timer.AutoReset = true;
        timer.Enabled = true;
    }

    private void OnTimedEvent(object? sender, ElapsedEventArgs e)
    {
        try
        {
            IList<ParticipantResult> participantResults = resultSource.GetParticipantResults();
            var teamResults = pointsCalc.CalcScoreBoard(participantResults, resultSource.CurrentTimeOfDay);
            var teamResultsHash = CalcHasCode(teamResults);

            if (teamResultsHash != latestTeamResultsHash)
            {
                latestTeamResults = teamResults;
                latestTeamResultsHash = teamResultsHash;
                latestStatistics = GetStatistics(participantResults);

                OnNewResults?.Invoke(this, EventArgs.Empty);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"{nameof(OnTimedEvent)}");
        }
    }

    private Statistics GetStatistics(IEnumerable<ParticipantResult> participantResults)
    {
        var statistics = new Statistics();
        foreach (var pr in participantResults)
        {
            var status = pr.Status;
            statistics.LastChangedTimeOfDay = resultSource.CurrentTimeOfDay;
            if (HasNotShownUpAtExpectedStatTime(pr))
                status = ParticipantStatus.NotStarted;

            switch (status)
            {
                case ParticipantStatus.Ignored: break;
                case ParticipantStatus.NotActivated: statistics.IncNumNotActivated(); break;
                case ParticipantStatus.Activated: statistics.IncNumActivated(); break;
                case ParticipantStatus.Started: statistics.IncNumStarted(); break;
                case ParticipantStatus.Preliminary: statistics.IncNumPreliminary(); break;
                case ParticipantStatus.Passed: statistics.IncNumPassed(); break;
                case ParticipantStatus.NotValid: statistics.IncNumNotValid(); break;
                case ParticipantStatus.NotStarted: statistics.IncNumNotStarted(); break;
                default: throw new NotImplementedException();
            }
        }
        return statistics;
    }

    private bool HasNotShownUpAtExpectedStatTime(ParticipantResult pr)
    {
        return pr.Status == ParticipantStatus.NotActivated
               && pr.StartTime > TimeSpan.Zero 
               && pr.StartTime < resultSource.CurrentTimeOfDay.Add(configuration.TimeUntilNotStated);
    }

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
