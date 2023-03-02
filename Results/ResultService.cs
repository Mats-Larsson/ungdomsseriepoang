using System.Collections.Immutable;
using System.Timers;
using Results.Contract;
using Results.Model;
using Results.Simulator;

namespace Results;

public class ResultService : IResultService, IDisposable
{
    private IList<TeamResult> latestTeamResults = ImmutableList<TeamResult>.Empty;
    private int latestTeamResultsHash;
    private Statistics latestStatistics = new();
    private readonly IResultSource resultSource;
    private readonly Configuration configuration;
    private readonly PointsCalc pointsCalc;
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly System.Timers.Timer timer;

    public ResultService() : this(new SimulatorResultSource(), new Configuration()) { }

    private ResultService(IResultSource resultSource, Configuration configuration)
    {
        this.resultSource = resultSource;
        this.configuration = configuration;
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
            var teamResults = pointsCalc.CalcScoreBoard(participantResults);
            var teamResultsHash = CalcHasCode(teamResults);

            if (teamResultsHash != latestTeamResultsHash)
            {
                latestTeamResults = teamResults;
                latestTeamResultsHash = teamResultsHash;
                latestStatistics = GetStatistics(participantResults);

                OnNewResults?.Invoke(this, EventArgs.Empty);
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
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
    }
}
