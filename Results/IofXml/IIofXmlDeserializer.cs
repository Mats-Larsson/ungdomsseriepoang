using Results.Model;

namespace Results.IofXml;

public interface IIofXmlDeserializer
{
    public IofXmlResult Deserialize(Stream xmlStream);
}

public record IofXmlResult(
    TimeSpan CurrentTimeOfDay,
    string CompetitionName,
    IList<ParticipantResult> ParticipantResults
);