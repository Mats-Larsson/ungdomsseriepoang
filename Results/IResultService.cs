using Results.Contract;

namespace Results;

public interface IResultService
{
    Result GetScoreBoard();
    event EventHandler OnNewResults;
}
