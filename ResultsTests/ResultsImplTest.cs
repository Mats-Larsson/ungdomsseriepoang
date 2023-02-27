using Results;

namespace ResultsTests
{
    [TestClass]
    public class ResultsImplTest
    {
        [TestMethod]
        public void TestWithSimulator()
        {
            var results = new ResultService();
            var teamResults = results.GetScoreBoard();
        }
    }
}
