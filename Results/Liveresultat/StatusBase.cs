using System.Text.Json.Serialization;

namespace Results.Liveresultat;

public abstract record StatusBase {

    [JsonPropertyName("status")]
    public string? Status { get; init; }
        
    [JsonPropertyName("hash")]
    public string? Hash { get; init; }
}