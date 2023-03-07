using Microsoft.Extensions.Logging;
using Results;
using Results.Contract;

namespace ResultsTests
{
    [TestClass]
    public class ResultsTest
    {
        [TestMethod]
        public void TestWithSimulator()
        {
            var configuration = new Configuration(ResultSource.Simulator);
            using var results = new ResultService(configuration, new Logger<ResultService>(new LoggerFactory()));
            var teamResults = results.GetScoreBoard();
        }
    }
}
