using System.Diagnostics.CodeAnalysis;
using ResultCalc.Model;

namespace ResultCalc.IofXml;

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