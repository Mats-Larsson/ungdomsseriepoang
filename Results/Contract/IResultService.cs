using Results.Model;

namespace Results.Contract;

public interface IResultService
{
    bool SupportsPreliminary { get; }
    Result GetScoreBoard();
    event EventHandler OnNewResults;
    Task<string> NewResultPostAsync(Stream body, DateTime timestamp);
    IList<ParticipantPoints> GetParticipantPointsList();
}
