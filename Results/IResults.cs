using Results.Contract;

namespace Results;

public interface IResults
{
    IList<TeamResult> GetScoreBoard();
    event EventHandler OnNewResults;
}
