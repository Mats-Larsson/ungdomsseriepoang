using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Results.Liveresultat.Model;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public record ClassList : StatusBase
{

    [JsonPropertyName("classes")]
    public IList<Class>? Classes { get; init; }
}