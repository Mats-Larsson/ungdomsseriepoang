namespace Results.Model
{
    public interface IResultSource : IDisposable
    {
        IList<ParticipantResult> GetParticipantResults();
        TimeSpan CurrentTimeOfDay { get; }
        Task<string> NewResultPostAsync(Stream body, DateTime timestamp);
    }
}
