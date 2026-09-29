using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace ResultCalc.Liveresultat.Model;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
internal record ClassList : StatusBase
{

    [JsonPropertyName("classes")]
    public IList<Class>? Classes { get; init; }
}