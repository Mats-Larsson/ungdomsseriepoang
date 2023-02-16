using Results.MySql;

namespace Results
{
    internal interface IResultSource
    {
        IList<ParticipantResult> GetParticipantResults();

        event EventHandler? NyaResult;
    }

    class ResultSourceSimulator : IResultSource
    {
        public IList<ParticipantResult> GetParticipantResults()
        {
            var participantResults = new List<ParticipantResult>
            {
                new("H10", "Adam", "Club A", TimeSpan.Parse("00:11:00"), ParticipantStatus.Passed),
                new("H10", "Beta", "Club B", TimeSpan.Parse("00:11:00"), ParticipantStatus.Passed),
                new("H10", "Rory", "Club A", TimeSpan.Parse("00:14:00"), ParticipantStatus.Passed),
                new("H10", "Eric", "Club B", TimeSpan.Parse("00:15:00"), ParticipantStatus.Started),
                new("D10", "Aida", "Club A", TimeSpan.Parse("00:11:00"), ParticipantStatus.Passed),
                new("D10", "Dodo", "Club B", TimeSpan.Parse("00:12:20"), ParticipantStatus.Passed),
                new("D10", "Elsa", "Club A", TimeSpan.Parse("00:13:30"), ParticipantStatus.Passed),
                new("D10", "Lena", "Club B", TimeSpan.Parse("00:19:09"), ParticipantStatus.Started),
            };

            return participantResults;
        }

        public event EventHandler? NyaResult;
    }
}
