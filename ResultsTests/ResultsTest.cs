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
                NumTeams = 100
            };

            using var simulatorResultSource = new SimulatorResultSource(configuration);
            using var results = new ResultService(configuration, simulatorResultSource, new BaseResultService("", Mock.Of<ILogger<BaseResultService>>()), Mock.Of<ILogger<ResultService>>());
            var teamResults = results.GetScoreBoard();
        }
    }
}
