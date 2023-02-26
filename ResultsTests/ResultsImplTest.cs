using Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
