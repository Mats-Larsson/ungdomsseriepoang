using System.Text.Json.Serialization;

namespace Results.Liveresultat.Model;

internal record LastPassingList : StatusBase
{
    [JsonPropertyName("passings")]
    public IList<Passing>? Passings { get; init; }
}