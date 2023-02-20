using Results;

namespace ResultsTests
{
    [TestClass]
    public class ResultsImplTest
    {
        [TestMethod]
        public void TestWithSimulator()
        {
            var results = new ResultsImpl();
            var teamResults = results.GetScoreBoard();
        }
    }
}
