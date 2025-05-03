using Results.Model;

namespace Results.IofXml
{
    public interface IIofXmlDeserializer
    {
        public IList<ParticipantResult> Deserialize(Stream xmlStream, out TimeSpan currentTimeOfDay);
    }
}