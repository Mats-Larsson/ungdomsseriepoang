using System.Diagnostics.CodeAnalysis;
using Results.Model;

namespace Results.IofXml;

internal interface IIofXmlDeserializer
{
    public IofXmlResult Deserialize(Stream xmlStream);
}

[SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Global")]
internal record IofXmlResult(
    TimeSpan CurrentTimeOfDay,
    string CompetitionName,
    IList<ParticipantResult> ParticipantResults
);