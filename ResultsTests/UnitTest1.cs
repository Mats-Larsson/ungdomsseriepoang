using Results.MySql;

namespace ResultsTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            IDbResultat resultat = new DbResultatImpl();

            List<PersonResultat> personResultats = resultat.GetPersonResultats();
        }
    }
}