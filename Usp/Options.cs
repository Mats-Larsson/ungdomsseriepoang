using CommandLine;
using CommandLine.Text;
using Results;
using Results.Contract;

namespace Usp;

public class Options
{
    private static ParserResult<Options>? parserResult;

    // General options
    [Option("listenerport", Group = "Sim", Default = 8880, HelpText = "Port that the applcation listens to. Remember to open the firewall for this port if you ar using a broeser on another")]
    public int ListenerPort { get; set; }

    [Option('s', "simulator", Group = "Sim", Default = true, HelpText = "Use data from built in simulator.")]
    public bool UseSimulator { get; set; }

    [Option('m', "meos", Group = "Meos", Default = false, HelpText = "Use data from MeOS via HTTP POST")]
    public bool UseMeos { get; set; }

    [Option('o', "ola", Group = "Ola", Default = false, HelpText = "Use data from Ola via MySQL database. Built in H2 database is not supported yet.")]
    public bool UseOla { get; set; }

    [Option("maxlatestart", Default = 10, HelpText = "Number of minutes to wait after schedulated starttime for participeant to get activated, until register as not started.")]
    public int MinutesUntilNotStated { get; set; }
    public TimeSpan TimeUntilNotStated => TimeSpan.FromMinutes(MinutesUntilNotStated);


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
        });
        parserResult = parser.ParseArguments<Options>(args);

        if (parserResult.Errors.Any())
        {
            HelpText = HelpText.AutoBuild(parserResult, h =>
            {
                h.AddEnumValuesToHelpText = true;
                return h;
            });
        }

        return parserResult.Value;
    }

    public Configuration CreateConfiguration()
    {
        Options value = parserResult!.Value;

        var conf = new Configuration()
        {
            ResultSource =
            value.UseSimulator ? ResultSource.Simulator
            : (value.UseOla ? ResultSource.OlaDatabase
                : (value.UseMeos ? ResultSource.Meos
                    : throw new InvalidOperationException("Cannot determin result source"))),

            TimeUntilNotStated = value.TimeUntilNotStated,
            SpeedMultiplier = value.Speed,
            NumTeams = value.NumTeams,

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


#pragma warning disable CA1812 // Avoid uninstantiated internal classes

