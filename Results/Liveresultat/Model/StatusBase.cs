using System.Text.Json.Serialization;

namespace Results.Liveresultat.Model;

public abstract record StatusBase : DeserializationBase
{

    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("hash")]
    public string? Hash { get; init; }
}