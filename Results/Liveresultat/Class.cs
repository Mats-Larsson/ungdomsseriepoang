using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

#pragma warning disable CA1716 // Identifiers should not match keywords
namespace Results.Liveresultat;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public record Class
{
    [JsonPropertyName("className")]
    public string? ClassName { get; init; }
}