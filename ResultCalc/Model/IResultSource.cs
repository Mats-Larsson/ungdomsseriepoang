namespace ResultCalc.Model;

internal interface IResultSource : IDisposable
{
    bool SupportsPreliminary { get; }
    Task<IList<ParticipantResult>> GetParticipantResultsAsync();
    Task<TimeSpan> GetCurrentTimeOfDayAsync();
    Task<string> NewResultPostAsync(Stream body, DateTime timestamp);
}
