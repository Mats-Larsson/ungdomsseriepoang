using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Results.Liveresultat;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public record CompetitionInfo {

    [JsonPropertyName("id")]
    public int? Id { get; init; }
}