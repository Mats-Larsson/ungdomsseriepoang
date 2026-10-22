using ResultCalc.IofXml;
using ResultCalc.Model;

namespace ResultCalc.Eventor
{
    internal sealed class EventorResultSource(
        Configuration configuration, 
        IEventorFacade eventorFacade,
        IIofXmlDeserializer deserializer) : IResultSource
    {
        private IofXmlResult? iofXmlResult;
        public bool SupportsPreliminary => false;
        public TimeSpan CurrentTimeOfDay => iofXmlResult?.CurrentTimeOfDay ?? TimeSpan.Zero;

        public async Task<IList<ParticipantResult>> GetParticipantResultsAsync()
        {
            if (iofXmlResult != null)
            {
                return iofXmlResult.ParticipantResults;
            }

            var iofXmlStream = await eventorFacade.GetIofXmlStream(configuration.EventorEventId).ConfigureAwait(false);
            await using (iofXmlStream.ConfigureAwait(false))
            {
                iofXmlResult = deserializer.Deserialize(iofXmlStream);
            }
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