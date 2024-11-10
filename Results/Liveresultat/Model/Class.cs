using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Results.Liveresultat.Model;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("Naming", "CA1716:Identifiers should not match keywords")]
public record Class
{
    [JsonPropertyName("className")]
    public string? ClassName { get; init; }
}