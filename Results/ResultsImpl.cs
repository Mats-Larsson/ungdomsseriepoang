using System.Collections.Immutable;
using Results.Contract;
using Results.Simulator;

namespace Results;

public class ResultsImpl : IResults
{
    private readonly IResultSource resultSource;
    private readonly PointsCalc pointsCalc;
    private readonly IList<TeamResult> senasteKlubbResultats = ImmutableList<TeamResult>.Empty;
    private readonly int senasteKlubbResultatsHashCode;

    public ResultsImpl() : this(new ResultSourceSimulator()) { }

    private ResultsImpl(IResultSource resultSource)
    {
        this.resultSource = resultSource;
        this.pointsCalc = new PointsCalc();

        senasteKlubbResultats = pointsCalc.CalcScoreBoard(resultSource.GetParticipantResults());
    }

    public IList<TeamResult> GetScoreBoard()
    {
        return pointsCalc.CalcScoreBoard(resultSource.GetParticipantResults());
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
}
