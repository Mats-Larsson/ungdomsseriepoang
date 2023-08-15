using Results.Contract;

namespace Results;

public class Configuration
{
    public ResultSource ResultSource { get; set; }

    public string? OlaMySqlHost { get; init; }
    public int OlaMySqlPort { get; init; }
    public string? OlaMySqlDatabase { get; init; }
    public string? OlaMySqlUser { get; init; }
    public string? OlaMySqlPassword { get; init; }
    public int? OlaEventId { get; init; }

    public TimeSpan TimeUntilNotStated { get; init; }
    public int SpeedMultiplier { get; init; }
    public int NumTeams { get; init; }
    public string? BasePointsFilePath { get; init; }
    public bool IsFinal { get; init; }

    public Configuration()
    {
    }

    public Configuration(ResultSource resultSource)
    {
        ResultSource = resultSource;
    }

}
