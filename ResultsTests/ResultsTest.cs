using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Results;
using Results.Contract;
using Results.Model;
using Results.Simulator;

namespace ResultsTests
{
    [TestClass]
    public class ResultsTest
    {
        [TestMethod]
        public void TestWithSimulator()
        {
            var configuration = new Configuration(ResultSource.Simulator)
            {
                SpeedMultiplier = 10,
                NumTeams = 100,
                BasePointsFilePath = "BasePoints.csv"
            };
            File.WriteAllText(configuration.BasePointsFilePath, "Lag X,1000\r\n");
            using var simulatorResultSource = new SimulatorResultSource(configuration);
            var basePointsService = new BasePointsService(configuration, Mock.Of<ILogger<BasePointsService>>(), simulatorResultSource);
            using var resultService = new ResultService(configuration, simulatorResultSource, basePointsService, Mock.Of<ILogger<ResultService>>());
            var teamResults = resultService.GetScoreBoard();
            teamResults.TeamResults.Should().Contain(new TeamResult(1, "Lag X", 1000, false));
        }
    }
}
