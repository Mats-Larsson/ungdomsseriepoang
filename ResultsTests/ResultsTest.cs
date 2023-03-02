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
            using var results = new ResultService(ResultSource.Simulator);
            var teamResults = results.GetScoreBoard();
        }
    }
}
