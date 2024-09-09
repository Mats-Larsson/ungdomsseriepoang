using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Results.Liveresultat.Model;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public record CompetitionInfo : DeserializationBase
{

    [JsonPropertyName("id")]
    public int? Id { get; init; }
    
    [JsonPropertyName("name")]
    public String? Name { get; init; }
}