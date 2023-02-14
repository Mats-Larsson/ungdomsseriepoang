using System.Collections.Immutable;

namespace Results;

public class ResultsImpl : IResults
{
    private readonly IDbResultat dbResultat;
    private readonly IList<KlubbResultat> senasteKlubbResultats = ImmutableList<KlubbResultat>.Empty;
    private readonly int senasteKlubbResultatsHashCode;

    internal ResultsImpl(IDbResultat dbResultat)
    {
        this.dbResultat = dbResultat;
    }

    public IList<KlubbResultat> GetKlubbResultats()
    {
        return senasteKlubbResultats;
    }
        
    public event EventHandler? OnNyaResultat;

    private static int CalcHasCode(IList<KlubbResultat> resultats)
    {
        var hashCode = 0;

        foreach (var resultat in resultats)
        {
            unchecked
            {
                hashCode += resultat.GetHashCode();
            }
        }
        return hashCode;
    }
}
