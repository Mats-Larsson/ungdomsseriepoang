using System.Collections.Immutable;
using System.Timers;
using Results.Contract;
using Results.Model;
using Results.Simulator;

namespace Results;

public class ResultsImpl : IResults
{
    private IList<TeamResult> latestTeamResults = ImmutableList<TeamResult>.Empty;
    private int latestTeamResultsHash;
    private Statistics latestStatistics = new Statistics(0, 0, 0, 0, 0);
    private readonly IResultSource resultSource;
    private readonly PointsCalc pointsCalc;
    private readonly System.Timers.Timer timer;

    public ResultsImpl() : this(new SimulatorResultSource()) { }

    private ResultsImpl(IResultSource resultSource)
    {
        this.resultSource = resultSource;
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
            switch (pr.Status)
            {
                case ParticipantStatus.Ignored:      break;
                case ParticipantStatus.NotActivated: statistics.IncNumNotActivated(); break;
                case ParticipantStatus.Started:      statistics.IncNumStarted(); break;
                case ParticipantStatus.Preliminary:  statistics.IncNumStarted(); break;
                case ParticipantStatus.Passed:       statistics.IncNumPassed(); break;
                case ParticipantStatus.NotValid:     statistics.IncNumNotValid(); break;
                case ParticipantStatus.NotStarted:   statistics.IncNumNotStarted(); break;
                default: throw new NotImplementedException();
            }
        }
        return statistics;
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
