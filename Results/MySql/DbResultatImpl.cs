using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using MySql.Data.MySqlClient;

namespace Results.MySql;

internal class DbResultatImpl : IDbResultat
{
    private readonly string connectionString;
    private IList<PersonResultat>? personResultats;
    private readonly System.Timers.Timer timer;

    public DbResultatImpl(string host, int? port, string database, string user, string password)
    {
        connectionString = $"server={host};port={port ?? 3306};userid={user};password={password};database={database}";
        // Create a timer with a two second interval.
        timer = new System.Timers.Timer(TimeSpan.FromSeconds(10).TotalMilliseconds);
        // Hook up the Elapsed event for the timer. 
        timer.Elapsed += OnTimedEvent;
        timer.AutoReset = true;
        timer.Enabled = true;
    }

    private void OnTimedEvent(object? sender, ElapsedEventArgs e)
    {
        try
        {
            PersonResultatsLocal();
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }

    public IList<PersonResultat> GetPersonResultats()
    {
        if (personResultats == null)
            PersonResultatsLocal();
        if (personResultats == null)
            throw new InvalidOperationException();

        return personResultats;
    }

    private void PersonResultatsLocal()
    {
        using var con = new MySqlConnection(connectionString);
        con.Open();

        using var cmd = new MySqlCommand(Resource1.PersonResutat, con);
        using var reader = cmd.ExecuteReader();
        var list = new List<PersonResultat>();
        while (reader.Read())
        {
            var klass = reader.GetFieldValue<string>(0);
            var namn = reader.GetFieldValue<string>(1);
            var klubb = reader.GetFieldValue<string>(2);
            var tid = reader.IsDBNull(3) ? null : new TimeSpan?(reader.GetFieldValue<TimeSpan>(3));
            var status = ToPersonStatus(reader.GetFieldValue<string>(4), tid);
            list.Add(new PersonResultat(klass, namn, klubb, tid, status));
        }

        personResultats = ImmutableList.Create(list.ToArray());
    }

    public event EventHandler? NyaResultat;

    private void NyaResultatFinns()
    {
        OnNyaResultat();
    }

    protected virtual void OnNyaResultat()
    {
        NyaResultat?.Invoke(this, EventArgs.Empty);
    }

    private static PersonStatus ToPersonStatus(string v, TimeSpan? tid)
    {
        return v switch
        {
            "walkOver" => PersonStatus.DeltarEj,
            "movedUp" => PersonStatus.DeltarEj,
            "notParticipating" => PersonStatus.DeltarEj,
            "notActivated" => PersonStatus.EjStart,
            "notStarted" => PersonStatus.EjStart,
            "started" => PersonStatus.Aktiverad,
            "disqualified" => PersonStatus.Aktiverad,
            "notValid" => PersonStatus.Aktiverad,
            "finished" => tid.HasValue ? PersonStatus.Godkänd : PersonStatus.Aktiverad,
            "finishedTimeOk" => tid.HasValue ? PersonStatus.Godkänd : PersonStatus.Aktiverad,
            "finishedPunchOk" => tid.HasValue ? PersonStatus.Godkänd : PersonStatus.Aktiverad,
            "passed" => tid.HasValue ? PersonStatus.Godkänd : PersonStatus.Aktiverad,
            _ => throw new InvalidOperationException($"Unexpected status: {v}")
        };
    }
}