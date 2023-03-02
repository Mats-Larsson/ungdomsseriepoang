namespace Results.Model
{
    internal interface IResultSource : IDisposable
    {
        IList<ParticipantResult> GetParticipantResults();
        TimeSpan CurrentTimeOfDay { get; }

    }
}
