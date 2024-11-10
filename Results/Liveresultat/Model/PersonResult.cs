using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Results.Liveresultat.Model;

public record PersonResult
{
    [JsonPropertyName("place")]
    public string? Place { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("club")]
    public string? Club { get; init; }

    [JsonPropertyName("result")]
    public string? ResultRaw { get; init; }
    public TimeSpan? Time => ToTimeSpan(ResultRaw);

    [JsonPropertyName("status")]
    public Status Status { get; init; }

    [JsonPropertyName("timeplus")]
    public string? Timeplus { get; init; }

    [JsonPropertyName("progress")]
    public int Progress { get; init; }

    [JsonPropertyName("start")]
    public JsonElement? StartRaw { get; init; }
    public TimeSpan? StartTime => !StartRaw.HasValue ? default : ToTimeSpan(StartRaw.Value.ToString());

    private static TimeSpan? ToTimeSpan(string? val)
    {
        if (val == null) return default;

        return !int.TryParse(val, out int intVal)
            ? default(TimeSpan?)
            : TimeSpan.FromMilliseconds(10 * intVal);
    }
}