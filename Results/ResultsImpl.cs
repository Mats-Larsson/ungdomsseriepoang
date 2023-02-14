using System.Collections.Immutable;

namespace Results;

public class ResultsImpl : IResults
{
    private readonly IResultSource resultSource;
    private readonly IList<ClubResult> senasteKlubbResultats = ImmutableList<ClubResult>.Empty;
    private readonly int senasteKlubbResultatsHashCode;

    internal ResultsImpl(IResultSource resultSource)
    {
        this.resultSource = resultSource;
    }

    public IList<ClubResult> GetScoreBoard()
    {
        return senasteKlubbResultats;
    }
        
    public event EventHandler? OnNyaResultat;

    private static int CalcHasCode(IList<ClubResult> results)
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
