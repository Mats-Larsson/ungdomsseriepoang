using Microsoft.Extensions.Logging;
using Moq;
using Results;
using Results.Contract;

namespace ResultsTests
{
    [TestClass]
    public class ResultsTest
    {
        private readonly ILogger loggerMock = Mock.Of<ILogger>();

        [TestMethod]
        public void TestWithSimulator()
        {
            var configuration = new Configuration(ResultSource.Simulator);
            using var results = new ResultService(configuration, loggerMock);
            var teamResults = results.GetScoreBoard();
        }
    }
}
