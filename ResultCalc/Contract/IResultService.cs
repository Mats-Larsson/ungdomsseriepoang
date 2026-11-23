namespace ResultCalc.Contract;

public interface IResultService
{
    bool SupportsPreliminary { get; }
    Task<Result> GetScoreBoardAsync();
    event EventHandler OnNewResults;
    Task<string> NewResultPostAsync(Stream body, DateTime timestamp);
    Task<IEnumerable<ParticipantPoints>> GetParticipantPointsListAsync();
}
