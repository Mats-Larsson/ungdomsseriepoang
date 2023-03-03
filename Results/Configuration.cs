using Results.Contract;

namespace Results;

public class Configuration
{
    public ResultSource ResultSource { get; set; } = ResultSource.Simulator;

    public string? OlaMySqlHost { get; set; } = "NUC";
    public int? OlaMySqlPort { get; set; }
    public string? OlaMySqlDatabase { get; set; } = "kretstavling2019";
    public string? OlaMySqlUser { get; set; } = "root";
    public string? OlaMySqlPassword { get; set; } = "kasby";
    public int? OlaEventId { get; set; } = 1;

    public TimeSpan TimeUntilNotStated { get; set; } = TimeSpan.FromMinutes(10);
    public int SpeedMultiplier { get; set; } = 10;

    public Configuration()
    {
    }

    public Configuration(ResultSource resultSource)
    {
        ResultSource = resultSource;
    }

}
