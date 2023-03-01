namespace Results.Contract;

public interface IResultService
{
    Result GetScoreBoard();
    event EventHandler OnNewResults;
}
