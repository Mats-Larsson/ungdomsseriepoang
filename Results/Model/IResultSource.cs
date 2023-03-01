namespace Results.Model
{
    internal interface IResultSource
    {
        IList<ParticipantResult> GetParticipantResults();
        TimeSpan CurrentTimeOfDay { get; }

    }
}
