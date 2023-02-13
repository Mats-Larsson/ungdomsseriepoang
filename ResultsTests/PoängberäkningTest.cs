using Results;
using Results.MySql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResultsTests
{
    [TestClass]
    public class PoängberäkningTest
    {
        [TestMethod]
        public void Test1()
        {
            IDbResultat resultat = new DbResultatImpl("NUC", null, "kretstavling2019", "root", "kasby");
            var personResultats = resultat.GetPersonResultats();

            var poängberäkning = new Poängberäkning();
            var klubbResultats = poängberäkning.BeräknaPoäng(personResultats);
        }
    }
}
