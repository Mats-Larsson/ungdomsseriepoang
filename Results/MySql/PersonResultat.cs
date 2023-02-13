using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Results.MySql;

internal class PersonResultat
{
    public string Klass { get; }
    public string Namn { get; }
    public string Klubb { get; }
    public TimeSpan? Tid { get; }
    public PersonStatus Status { get; }

    public PersonResultat(string klass, string namn, string klubb, TimeSpan? tid, PersonStatus status)
    {
        Klass = klass;
        Namn = namn;
        Klubb = klubb;
        Tid = tid;
        Status = status;
    }
}

internal enum PersonStatus
{
    DeltarEj = 0,
    EjStart,
    Aktiverad,
    Godkänd
}
