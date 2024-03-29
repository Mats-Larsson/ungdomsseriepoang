﻿namespace Results;

public class Configuration
{
    // General
    public TimeSpan TimeUntilNotStated { get; init; }
    public string? TeamsFilePath { get; init; }
    public bool IsFinal { get; init; }
    public TimeSpan MaxPatrolStartInterval { get; init; }

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
}
