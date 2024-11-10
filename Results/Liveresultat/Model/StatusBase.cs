using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Results.Liveresultat.Model;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public abstract record StatusBase : DeserializationBase
{

    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("hash")]
    public string? Hash { get; init; }
}