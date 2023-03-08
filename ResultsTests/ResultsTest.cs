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
        private readonly ILogger<ResultService> loggerMock = Mock.Of<ILogger<ResultService>>();

        [TestMethod]
        public void TestWithSimulator()
        {
            var configuration = new Configuration(ResultSource.Simulator);
            using var simulatorResultSource = new SimulatorResultSource(configuration);
            using var results = new ResultService(configuration, simulatorResultSource, loggerMock);
            var teamResults = results.GetScoreBoard();
        }
    }
}
