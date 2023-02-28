using Results;

namespace ResultsTests
{
    [TestClass]
    public class ResultsTest
    {
        [TestMethod]
        public void TestWithSimulator()
        {
            var results = new ResultService();
            var teamResults = results.GetScoreBoard();
        }
    }
}
