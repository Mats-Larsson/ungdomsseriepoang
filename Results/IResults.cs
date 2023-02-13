using Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Results;

public interface IResults
{
    IList<KlubbResultat> GetKlubbResultats();
}
