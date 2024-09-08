namespace Results.Model;

public interface IResultSource : IDisposable
{
    bool SupportsPreliminary { get; }
    IList<ParticipantResult> GetParticipantResults();
    TimeSpan CurrentTimeOfDay { get; }
    Task<string> NewResultPostAsync(Stream body, DateTime timestamp);
}
