namespace ResultCalc.Model;

internal interface IResultSource : IDisposable
{
    bool SupportsPreliminary { get; }
    Task<IList<ParticipantResult>> GetParticipantResultsAsync();
    TimeSpan CurrentTimeOfDay { get; }
    Task<string> NewResultPostAsync(Stream body, DateTime timestamp);
}
