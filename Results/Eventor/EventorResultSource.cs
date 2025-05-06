using Results.IofXml;
using Results.Model;

namespace Results.Eventor
{
    public sealed class EventorResultSource(
        Configuration configuration, 
        EventorFacade eventorFacade,
        IIofXmlDeserializer deserializer) : IResultSource
    {
        private IofXmlResult? iofXmlResult;
        public bool SupportsPreliminary => false;
        public TimeSpan CurrentTimeOfDay => iofXmlResult?.CurrentTimeOfDay ?? TimeSpan.Zero;

        public IList<ParticipantResult> GetParticipantResults()
        {
            if (iofXmlResult != null)
            {
                return iofXmlResult.ParticipantResults;
            }

            using var iofXmlStream = eventorFacade.GetIofXmlStream(configuration.EventorEventId);
            iofXmlResult = deserializer.Deserialize(
                iofXmlStream.ConfigureAwait(false).GetAwaiter().GetResult());
            return iofXmlResult.ParticipantResults;
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