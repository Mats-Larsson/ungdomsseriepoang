using Results.Contract;

namespace Results;

public interface IResults
{
    Result GetScoreBoard();
    event EventHandler OnNewResults;
}
