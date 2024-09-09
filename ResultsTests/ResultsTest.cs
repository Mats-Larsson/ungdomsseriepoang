using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Results;
using Results.Contract;
using Results.Simulator;

namespace ResultsTests;

[TestClass]
[SuppressMessage("ReSharper", "StringLiteralTypo")]
public class ResultsTest
{
    private readonly ClassFilter classFilter = new(new Configuration());

    [TestMethod]
    public void TestWithSimulatorTeamsAndBasePoints()
    {
        var configuration = new Configuration
        {
            SpeedMultiplier = 10,
            NumTeams = 100,
            TeamsFilePath = "Teams.csv",
            RefreshInterval = TimeSpan.FromSeconds(10)
        };
        File.WriteAllText(configuration.TeamsFilePath, "Lag A,1000\r\nLag B, 600");
        using var simulatorResultSource = new SimulatorResultSource(configuration);
        var teamService = new TeamService(configuration, Mock.Of<ILogger<TeamService>>());
        using var resultService = new ResultService(configuration, simulatorResultSource, teamService, Mock.Of<ILogger<ResultService>>(), classFilter);
        var teamResults = resultService.GetScoreBoard();
        teamResults.TeamResults.Count.Should().Be(2);
        teamResults.TeamResults.Should().Contain(
            new TeamResult(1, "Lag A", 1000, false, 0, 1000, new Statistics())
        );
        teamResults.TeamResults.Should().Contain(
            new TeamResult(2, "Lag B", 600, false, 400, 600, new Statistics())
        );
    }
    
    [TestMethod]
    public void TestWithSimulatorTeams()
    {
        var configuration = new Configuration
        {
            SpeedMultiplier = 10,
            NumTeams = 100,
            TeamsFilePath = "Teams.csv",
            RefreshInterval = TimeSpan.FromSeconds(10)
        };
        File.WriteAllText(configuration.TeamsFilePath, "Lag A\r\nLag B\r\nSnättringe SK");
        using var simulatorResultSource = new SimulatorResultSource(configuration);
        var teamService = new TeamService(configuration, Mock.Of<ILogger<TeamService>>());
        using var resultService = new ResultService(configuration, simulatorResultSource, teamService, Mock.Of<ILogger<ResultService>>(), classFilter);
        Task.Delay(TimeSpan.FromMilliseconds(10)).Wait(); // Let simulator start
        var teamResults = resultService.GetScoreBoard();
        teamResults.TeamResults.Count.Should().Be(3);
        teamResults.TeamResults.Should().Contain(
            new TeamResult(1, "Lag A", 0, false, 0, 0, new Statistics())
        );
        teamResults.TeamResults.Should().Contain(
            new TeamResult(1, "Lag B", 0, false, 0, 0, new Statistics())
        );
        teamResults.TeamResults.Should().Contain(result => result.Team == "Snättringe SK");
    }
}