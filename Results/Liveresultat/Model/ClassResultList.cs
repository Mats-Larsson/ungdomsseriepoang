using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Results.Liveresultat.Model;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public record ClassResultList : StatusBase
{
    [JsonPropertyName("className")]
    public string? ClassName { get; init; }

    [JsonPropertyName("results")]
    public IList<PersonResult>? Results { get; init; }
}