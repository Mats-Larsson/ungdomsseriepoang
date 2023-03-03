using Results;
using Results.Model;
using Results.Ola;

namespace ResultsTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            using IResultSource resultSource = new OlaResultSource(new Configuration());

            var participantResults = resultSource.GetParticipantResults();
        }
    }
}