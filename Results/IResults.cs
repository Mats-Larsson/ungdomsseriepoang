namespace Results;

public interface IResults
{
    IList<ClubResult> GetScoreBoard();
    event EventHandler OnNyaResultat;
}
