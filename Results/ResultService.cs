using System.Collections.Immutable;
using System.Timers;
using Results.Contract;
using Results.Model;
using Results.Simulator;

namespace Results;

public class ResultService : IResultService
{
    private IList<TeamResult> latestTeamResults = ImmutableList<TeamResult>.Empty;
    private int latestTeamResultsHash;
    private Statistics latestStatistics = new Statistics(0, 0, 0, 0, 0);
    private readonly IResultSource resultSource;
    private readonly ITimeService timeService;
    private readonly PointsCalc pointsCalc;
    private readonly System.Timers.Timer timer;

    public ResultService() : this(new SimulatorResultSourceImpl(), new TimeService()) { }

    private ResultService(IResultSource resultSource, ITimeService timeService)
    {
        this.resultSource = resultSource;
        this.timeService = timeService;
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
            IList<Model.ParticipantResult> participantResults = resultSource.GetParticipantResults();
            var teamResults = pointsCalc.CalcScoreBoard(participantResults);
            var teamResultsHash = CalcHasCode(teamResults);

            if (teamResultsHash != latestTeamResultsHash)
            {
                latestTeamResults = teamResults;
                latestTeamResultsHash = teamResultsHash;
                latestStatistics = GetStatistics(participantResults);

                OnNewResults?.Invoke(this, new EventArgs());
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }

    private Statistics GetStatistics(IList<ParticipantResult> participantResults)
    {
        var statistics = new Statistics();
        foreach (var pr in participantResults)
        {
            ParticipantStatus status = pr.Status;
            if (HasNotShownUpAtExpectedStatTime(pr))
                status = ParticipantStatus.NotStarted;

            switch (status)
            {
                case ParticipantStatus.Ignored: break;
                case ParticipantStatus.NotActivated: statistics.IncNumNotActivated(); break;
                case ParticipantStatus.Started: statistics.IncNumStarted(); break;
                case ParticipantStatus.Preliminary: statistics.IncNumStarted(); break;
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
                         && pr.StartTime.HasValue 
                         && pr.StartTime < timeService.TimeOfDay.Add(Configuration.TIME_UNTIL_NOT_STATED);
    }

    public Result GetScoreBoard()
    {
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
