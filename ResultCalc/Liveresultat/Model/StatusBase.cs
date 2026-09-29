using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace ResultCalc.Liveresultat.Model;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
internal abstract record StatusBase : DeserializationBase
{

    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("hash")]
    public string? Hash { get; init; }
}