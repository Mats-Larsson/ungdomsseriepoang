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
            using var results = new ResultService(configuration);
            var teamResults = results.GetScoreBoard();
        }
    }
}
