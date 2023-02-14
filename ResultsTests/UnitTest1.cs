using Results;
using Results.MySql;

namespace ResultsTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            IDbResultat resultat = new DbResultatImpl("NUC", null, "kretstavling2019", "root", "kasby");

            var personResultats = resultat.GetPersonResultats();
        }
    }
}