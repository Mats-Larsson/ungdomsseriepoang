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
            IResultSource resultat = new ResultSourceImpl("NUC", null, "kretstavling2019", "root", "kasby");

            var personResultats = resultat.GetParticipantResults();
        }
    }
}