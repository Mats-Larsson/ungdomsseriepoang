﻿using System.Diagnostics.CodeAnalysis;
using CommandLine;
using CommandLine.Text;
using Results;

namespace Usp;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "StringLiteralTypo")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class Options
{
    // General options
    [Option("listenerport", Default = 8880, HelpText = "Port that the application listens to. Remember to open the firewall for this port if you ar using a browser on another")]
    public int ListenerPort { get; set; }

    [Option('s', "source", Default = Source.Simulator, HelpText = "Select datasource for results to process.")]
    public Source Source { get; set; }

    [Option("refreshseconds", Default = 10, HelpText = "Number of seconds between data refresh.")]
    public int RefreshSeconds { get; set; }
    public TimeSpan RefreshInterval => TimeSpan.FromSeconds(RefreshSeconds);


    // Points calculation
    [Option("pointscalc", Group = "Points", Default = PointsCalcType.Final, HelpText = "How to calculate points.")]
    public PointsCalcType PointsCalc { get; set; }

    [Option("teams", Group = "Points", Default = "Teams.csv", HelpText = "Defines the teams for which points are calculated. If omitted all teams are included. Optionally base points can be entered. Base points is the number of points that the team starts with. Format is comma separated file in UTF-8 format with first team name then points.")]
    public string? TeamsPath { get; set; }

    [Option("maxlatestart", Group = "Points", Default = 1000, HelpText = "Number of minutes to wait after scheduled start time for participant to get activated, until register as not started.")]
    public int MinutesUntilNotStated { get; set; }
    private TimeSpan TimeUntilNotStated => TimeSpan.FromMinutes(MinutesUntilNotStated);

    [Option("maxpatrolinterval", Group = "Points", Default = 10, HelpText = "Number of seconds between start times to be detected as a patrol.")]
    public int MaxPatrolStartIntervalSeconds { get; private set; }
    public TimeSpan MaxPatrolStartInterval => TimeSpan.FromSeconds(MaxPatrolStartIntervalSeconds);

    [Option("include", Group ="Points", Default = new string[0], HelpText =  "Include classes not include by the default rule")]
    public IEnumerable<string>? IncludeClasses { get; set; }

    [Option("exclude", Group = "Points", Default = new string[0], HelpText = "Exclude classes include by the default rule")]
    public IEnumerable<string>? ExcludeClasses { get; set; }

    // Simulator options
    [Option("speed", Group = "Simulator", Default = 10, HelpText = "Simulation speed. Times faster than normal time.")]
    public int Speed { get; set; }

    [Option("numteams", Group = "Simulator", Default = 27, HelpText = "Number of teams to show in simulation.")]
    public int NumTeams { get; set; }

    // MeOS options
    // Not yet

    // Ola options
    [Option('h', "host", Group = "Ola", Default = "localhost", HelpText = "MySQL database server host")]
    public string? Host { get; set; }

    [Option('P', "port", Group = "Ola", Default = 3306, HelpText = "MySQL database server port")]
    public int Port { get; set; }

    [Option('D', "database", Group = "Ola", Required = true, HelpText = "MySQL database name")]
    public string? Database { get; set; }

    [Option('u', "user", Group = "Ola", Required = true, HelpText = "MySQL database server user to run select on database to get results")]
    public string? User { get; set; }

    [Option('p', "password", Group = "Ola", Required = true, HelpText = "MySQL database server password associated with user")]
    public string? Password { get; set; }

    [Option('e', "eventid", Group = "Ola", Default = 1, HelpText = "Event Id för tävlingen i OLA. Starta OLA, öppna tävlingen. Navigera till: Tävling -> Tävlingsuppgifter -> Etapper -> Välj Etapp till vänster och läs av Etapp-id till höger.")]
    public int EventId { get; set; }

    // Liveresultat
    [Option('L', "liveresultatid", Group = "Liveresultat", Default = 0, HelpText = "CompetitionId för tävlingen i Liveresultat. Se t.ex. https://liveresultat.orientering.se/adm/editComp.php?compid=27215")]
    public int? LiveresultatId { get; set; }

    // IofXml options

    [Option('d', "dir", Group = "IofXml", Default = ".", HelpText = "Directory to read IOF XML-files from")]
    public string? InputFolder { get; set; }



    public static HelpText? HelpText { get; private set; }
    public static IEnumerable<Error>? Errors { get; private set; }

    internal static Options? Parse(IEnumerable<string> args)
    {
        using var parser = new Parser(with =>
        {
            with.CaseInsensitiveEnumValues = true;
            with.CaseSensitive = true;
            with.AutoHelp = true;
        });
        ParserResult<Options>? parserResult = parser.ParseArguments<Options>(args);
        Errors  = parserResult.Errors;
        if (Errors.Any())
        {

           HelpText = HelpText.AutoBuild(parserResult, h =>
            {
                h.AddEnumValuesToHelpText = true;
                h.AutoHelp = true;
                return h;
            });
        }

        return parserResult.Value;
    }

    public static Configuration CreateConfiguration(Options value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));

        var conf = new Configuration
        {
            // General
            RefreshInterval = value.RefreshInterval,

            // Points
            TimeUntilNotStated = value.TimeUntilNotStated,
            TeamsFilePath = value.TeamsPath,
            IsFinal = value.PointsCalc == PointsCalcType.Final,
            MaxPatrolStartInterval = value.MaxPatrolStartInterval,
            IncludeClasses = new HashSet<string>(value.IncludeClasses ?? []),
            ExcludeClasses = new HashSet<string>(value.ExcludeClasses ?? []),

            // Simulator
            SpeedMultiplier = value.Speed,
            NumTeams = value.NumTeams,
            
            // Ola
            OlaMySqlHost = value.Host,
            OlaMySqlPort = value.Port,
            OlaMySqlDatabase = value.Database,
            OlaMySqlUser = value.User,
            OlaMySqlPassword = value.Password,
            OlaEventId = value.EventId,

            // Liveresultat
            LiveresultatId = value.LiveresultatId,

            // IofXml
            IofXmlInputFolder = value.InputFolder
        };

        return conf;
    }
}

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public enum PointsCalcType
{
    Normal,
    Final
}

public enum Source
{
    Simulator,
    Meos,
    Ola,
    Liveresultat,
    IofXml
}

#pragma warning disable CA1812 // Avoid uninstantiated internal classes

