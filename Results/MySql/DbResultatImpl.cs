using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Results.MySql;

internal class DbResultatImpl : IDbResultat
{
    public List<PersonResultat> GetPersonResultats()
    {
        string cs = @"server=nuc;userid=root;password=kasby;database=kretstavling2019";

        using var con = new MySqlConnection(cs);
        con.Open();
        var x = Resource1.PersonResutat;
        Console.WriteLine($"MySQL version : {con.ServerVersion}");

        using var cmd = new MySqlCommand(Resource1.PersonResutat, con);
        using var reader = cmd.ExecuteReader();
        var list = new List<PersonResultat>();
        while (reader.Read())
        {
            var klass = reader.GetFieldValue<string>(0);
            var namn = reader.GetFieldValue<string>(1);
            var klubb = reader.GetFieldValue<string>(2);
            var tid = reader.IsDBNull(3) ? null : reader.GetFieldValue<TimeSpan?>(3);
            var status = ToPersonStatus(reader.GetFieldValue<string>(4), tid);
            list.Add(new PersonResultat(klass, namn, klubb, tid, status));
        }
        return list;
    }

    private PersonStatus ToPersonStatus(string v, TimeSpan? tid)
    {
        switch (v)
        {
            case "walkOver":
            case "movedUp":
            case "notParticipating": return PersonStatus.DeltarEj;

            case "notActivated":
            case "notStarted": return PersonStatus.EjStart;

            case "started":
            case "disqualified":
            case "notValid": return PersonStatus.Aktiverad;

            case "finished":
            case "finishedTimeOk":
            case "finishedPunchOk":
            case "passed": return tid.HasValue ?  PersonStatus.Godkänd : PersonStatus.Aktiverad;

            default: throw new InvalidOperationException($"Unexpeted status: {v}");
        }
    }
}