namespace Results.Contract;

public interface IResultService
{
    Result GetScoreBoard();
    event EventHandler OnNewResults;
    Task<string> NewResultPostAsync(Stream body);
}
