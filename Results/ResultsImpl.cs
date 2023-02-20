using System.Collections.Immutable;
using System.Timers;
using Results.Contract;
using Results.Simulator;

namespace Results;

public class ResultsImpl : IResults
{
    private IList<TeamResult> senasteKlubbResultats = ImmutableList<TeamResult>.Empty;
    private int senasteKlubbResultatsHashCode;

    private readonly IResultSource resultSource;
    private readonly PointsCalc pointsCalc;
    private readonly System.Timers.Timer timer;

    public ResultsImpl() : this(new SimulatorResultSourceImpl()) { }

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
            var klubbResultats = pointsCalc.CalcScoreBoard(resultSource.GetParticipantResults());
            var klubbResultatsHashCode = CalcHasCode(klubbResultats);

            if (klubbResultatsHashCode != senasteKlubbResultatsHashCode)
            {
                senasteKlubbResultats = klubbResultats;
                senasteKlubbResultatsHashCode = klubbResultatsHashCode;

                OnNyaResultat?.Invoke(this, new EventArgs());
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }

    public IList<TeamResult> GetScoreBoard()
    {
        return senasteKlubbResultats;
    }
        
    public event EventHandler? OnNyaResultat;

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
