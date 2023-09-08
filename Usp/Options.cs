using CommandLine;
using CommandLine.Text;
using Results;
using Results.Contract;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Usp;

// ReSharper disable once ClassNeverInstantiated.Global
public class Options
{
    private static ParserResult<Options>? _parserResult;

    // General options
    [Option("listenerport", Default = 8880, HelpText = "Port that the application listens to. Remember to open the firewall for this port if you ar using a browser on another")]
    public int ListenerPort { get; set; }

    [Option('s', "source", Default = Source.Simulator, HelpText = "Select datasource for results to process.")]
    public Source Source { get; set; }

    [Option("maxlatestart", Default = 10, HelpText = "Number of minutes to wait after schedulated starttime for participeant to get activated, until register as not started.")]
    private int MinutesUntilNotStated { get; set; }
    public TimeSpan TimeUntilNotStated => TimeSpan.FromMinutes(MinutesUntilNotStated);

    [Option("basepoints", Default = "BasePoints.csv", HelpText = "Points given to each team as a start. Format is comma separated file in UTF-8 format with first team name then point. First, header row must be: \"Team,Points\"")]
    public string? BasePoints { get; set; }

    [Option("pointscalc", Default = PointsCalcType.Final, HelpText = "How to calculate points.")]
    public PointsCalcType PointsCalc { get; set; }

    // Simulator options
    [Option("speed", Group = "Sim", Default = 10, HelpText = "Simulation speed. Times faster than normal time.")]
    public int Speed { get; set; }

    [Option("numteams", Group = "Sim", Default = 27, HelpText = "Number of teams to show in simulation.")]
    public int NumTeams { get; set; }

    // MeOS options
    // Not yet

    // Ola options
    [Option('h', "host", Group = "Ola", Default = "localhost", HelpText = "MySQL database server host")]
    public string? Host { get; set; }

    [Option('P', "port", Group = "Ola", Default = 3306, HelpText = "MySQL database server port")]
    public int Port { get; set; }

    [Option('D', "database", Group = "Ola", Required = true, HelpText = "MySQL database server database")]
    public string? Database { get; set; }

    [Option('u', "user", Group = "Ola", Required = true, HelpText = "MySQL database server user to run select on database to get results")]
    public string? User { get; set; }

    [Option('p', "password", Group = "Ola", Required = true, HelpText = "MySQL database server password associated with user")]
    public string? Password { get; set; }

    [Option('e', "eventid", Group = "Ola", Default = 1, HelpText = "Event Id för tävlingen i OLA. Starta OLA, öppna tävlingen. Navigera till: Tävling -> Tävlingsuppgifter -> Etapper -> Välj Etapp till vänster och läs av Etapp-id till höger.")]
    public int EventId { get; set; }


    public static HelpText? HelpText { get; private set; }

    internal static Options? Parse(string[] args)
    {
        using var parser = new Parser(with =>
        {
            with.CaseInsensitiveEnumValues = true;
            with.CaseSensitive = true;   
        });
        _parserResult = parser.ParseArguments<Options>(args);

        if (_parserResult.Errors.Any())
        {
            HelpText = HelpText.AutoBuild(_parserResult, h =>
            {
                h.AddEnumValuesToHelpText = true;
                return h;
            });
        }

        return _parserResult.Value;
    }

    public Configuration CreateConfiguration()
    {
        Options value = _parserResult!.Value;
        var conf = new Configuration()
        {
            ResultSourceType = value.Source switch
            {
                Source.Simulator => ResultSourceType.Simulator,
                Source.Ola => ResultSourceType.OlaDatabase,
                Source.Meos => ResultSourceType.Meos,
                _ => throw new InvalidOperationException("Cannot determine result source"),
            },

            TimeUntilNotStated = value.TimeUntilNotStated,
            SpeedMultiplier = value.Speed,
            NumTeams = value.NumTeams,
            BasePointsFilePath = value.BasePoints,
            IsFinal = value.PointsCalc == PointsCalcType.Final,

            OlaMySqlHost = value.Host,
            OlaMySqlPort = value.Port,
            OlaMySqlDatabase = value.Database,
            OlaMySqlUser = value.User,
            OlaMySqlPassword = value.Password,
            OlaEventId = value.EventId

        };

        return conf;
    }
}

public enum PointsCalcType
{
    Normal,
    Final
}

public enum Source
{
    Simulator,
    Meos,
    Ola
}

#pragma warning disable CA1812 // Avoid uninstantiated internal classes

