using System.Diagnostics.CodeAnalysis;
using Results.IofXml;
using Results.Model;

namespace Results.Eventor
{
    public sealed class EventorResultSource(
        Configuration configuration, 
        EventorFacade eventorFacade,
        IIofXmlDeserializer deserializer) : IResultSource
    {
        private TimeSpan currentTimeOfDay;
        private IList<ParticipantResult> participantResults = [];
        public bool SupportsPreliminary => false;
        public TimeSpan CurrentTimeOfDay => currentTimeOfDay;

        public IList<ParticipantResult> GetParticipantResults()
        {
            if (participantResults.Any())
            {
                return participantResults;
            }

            using var iofXmlStream = eventorFacade.GetIofXmlStream(configuration.EventorEventId);
            participantResults = deserializer.Deserialize(
                iofXmlStream.ConfigureAwait(false).GetAwaiter().GetResult(), 
                out currentTimeOfDay);
            return participantResults;
        }

        public Task<string> NewResultPostAsync(Stream body, DateTime timestamp)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            eventorFacade.Dispose();
        }
    }
}