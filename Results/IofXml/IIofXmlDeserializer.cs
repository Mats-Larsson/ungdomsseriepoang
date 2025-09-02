using System.Diagnostics.CodeAnalysis;
using Results.Model;

namespace Results.IofXml;

public interface IIofXmlDeserializer
{
    public IofXmlResult Deserialize(Stream xmlStream);
}

[SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Global")]
public record IofXmlResult(
    TimeSpan CurrentTimeOfDay,
    string CompetitionName,
    IList<ParticipantResult> ParticipantResults
);