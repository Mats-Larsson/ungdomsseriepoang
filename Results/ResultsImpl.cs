using System.Collections.Immutable;

namespace Results;

public class ResultsImpl : IResults
{
    private readonly IResultSource resultSource;
    private readonly IList<TeamResult> senasteKlubbResultats = ImmutableList<TeamResult>.Empty;
    private readonly int senasteKlubbResultatsHashCode;

    internal ResultsImpl(IResultSource resultSource)
    {
        this.resultSource = resultSource;
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
}
