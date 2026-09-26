using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Results;

[SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
public record Configuration
{
    // General
    public TimeSpan RefreshInterval { get; init; }

    // Points
    public TimeSpan TimeUntilNotStated { get; init; }
    public string? TeamsFilePath { get; init; }
    public bool IsFinal { get; init; }
    public TimeSpan MaxPatrolStartInterval { get; init; }
    public ISet<string>? IncludeClasses { get; init; }
    public ISet<string>? ExcludeClasses { get; init; }

    //Ola
    public string? OlaMySqlHost { get; init; }
    public int OlaMySqlPort { get; init; }
    public string? OlaMySqlDatabase { get; init; }
    public string? OlaMySqlUser { get; init; }
    public string? OlaMySqlPassword { get; init; }
    public int? OlaEventId { get; init; }

    // Simulator
    public int SpeedMultiplier { get; init; }
    public int NumTeams { get; init; }

    //Liveresultat
    public int? LiveresultatId { get; init; }

    // IofXml
    public virtual string? IofXmlInputFolder { get; init; }
    
    // Eventor
    public string? ApiKey { get; init; }
    public int EventorEventId { get; init; }

    // Hardcoded config
    public static bool IsUsePatrolLongestTime => false;

    /// <summary>
    /// All properties, one per line, with collection values listed and password and api key hidden. Safe to write to logs.
    /// </summary>
    public string ToLogString()
    {
        var sb = new StringBuilder(nameof(Configuration));
        foreach (var property in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var value = property.Name switch
            {
                nameof(OlaMySqlPassword) => Mask(OlaMySqlPassword),
                nameof(ApiKey) => Mask(ApiKey),
                _ => FormatValue(property.GetValue(this))
            };
            sb.Append(CultureInfo.InvariantCulture, $"{Environment.NewLine}    {property.Name} = {value}");
        }
        return sb.ToString();
    }

    private static string? FormatValue(object? value) => value switch
    {
        null => null,
        string s => s,
        IEnumerable items => $"[{string.Join(", ", items.Cast<object?>())}]",
        IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString()
    };

    private static string? Mask(string? secret) => string.IsNullOrEmpty(secret) ? secret : "***";
}
