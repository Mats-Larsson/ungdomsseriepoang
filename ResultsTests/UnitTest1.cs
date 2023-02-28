using Results;
using Results.Ola;

namespace ResultsTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            IResultSource resultSource = new OlaResultSource("NUC", null, "kretstavling2019", "root", "kasby");

            var participantResults = resultSource.GetParticipantResults();
        }
    }
}