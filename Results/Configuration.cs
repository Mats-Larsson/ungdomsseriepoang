using System.Diagnostics.CodeAnalysis;

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
}
