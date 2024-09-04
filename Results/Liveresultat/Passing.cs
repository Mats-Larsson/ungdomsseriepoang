using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Results.Liveresultat;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public record Passing
{
    [JsonPropertyName("passtime")]
    public string? PassTimeRaw { get; init; }
    public TimeSpan? PassTime => string.IsNullOrEmpty(PassTimeRaw) ? null : TimeSpan.Parse(PassTimeRaw);
}