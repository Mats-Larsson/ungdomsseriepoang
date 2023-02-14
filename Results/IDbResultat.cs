using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Results.MySql;

namespace Results
{
    internal interface IDbResultat
    {
        IList<PersonResultat> GetPersonResultats();

        event EventHandler? NyaResultat;
    }
}
