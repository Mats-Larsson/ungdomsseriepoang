using System.Collections.Immutable;
using MySql.Data.MySqlClient;
using Results.Model;

namespace Results.Ola;

internal class OlaResultSource : IResultSource
{
    private readonly Configuration configuration;
    private readonly string connectionString;

    public OlaResultSource(Configuration configuration)
    {
        this.configuration = configuration;
        connectionString = $"server={configuration.OlaMySqlHost};" +
                           $"port={configuration.OlaMySqlPort ?? 3306};" +
                           $"userid={configuration.OlaMySqlUser};" +
                           $"password={configuration.OlaMySqlPassword};" +
                           $"database={configuration.OlaMySqlDatabase}";
    }

    public IList<ParticipantResult> GetParticipantResults()
    {
        using var con = new MySqlConnection(connectionString);
        con.Open();

#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
        using var cmd = new MySqlCommand(Resource1.ParticipantResultSql, con);
        cmd.Parameters.AddWithValue("@EventId", configuration.OlaEventId);
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
        using var reader = cmd.ExecuteReader();
        var list = new List<ParticipantResult>();
        while (reader.Read())
        {
            var @class = reader.GetFieldValue<string>(0);
            var name = reader.GetFieldValue<string>(1);
            var club = reader.GetFieldValue<string>(2);
            var startTime = reader.IsDBNull(3) ? null : new TimeSpan?(reader.GetFieldValue<TimeSpan>(3));
            var time = reader.IsDBNull(4) ? null : new TimeSpan?(reader.GetFieldValue<TimeSpan>(4));
            var olaStatus = reader.GetFieldValue<string>(5);

            list.Add(new ParticipantResult(@class, name, club, startTime, time, ToParticipantStatus(olaStatus, time)));
        }

        return ImmutableList.Create(list.ToArray());
    }

    // TODO: Hämta från OLA
    public TimeSpan CurrentTimeOfDay => DateTime.Now - DateTime.Now.Date;

    private static ParticipantStatus ToParticipantStatus(string olaStatus, TimeSpan? time)
    {
        return olaStatus switch
        {
            "walkOver" => ParticipantStatus.Ignored,
            "movedUp" => ParticipantStatus.Ignored,
            "notParticipating" => ParticipantStatus.Ignored,

            "notActivated" => ParticipantStatus.NotActivated,

            "started" => ParticipantStatus.Started,

            "finished" => time.HasValue ? ParticipantStatus.Preliminary : ParticipantStatus.Started,
            "finishedTimeOk" => time.HasValue ? ParticipantStatus.Preliminary : ParticipantStatus.Started,
            "finishedPunchOk" => time.HasValue ? ParticipantStatus.Preliminary : ParticipantStatus.Started,

            "passed" => time.HasValue ? ParticipantStatus.Passed : ParticipantStatus.Started,
            "disqualified" => ParticipantStatus.NotValid,
            "notValid" => ParticipantStatus.NotValid,
            "notStarted" => ParticipantStatus.NotStarted,

            _ => throw new InvalidOperationException($"Unexpected status: {olaStatus}")
        };
    }

    public void Dispose()
    {
        // Nothing to dispose
    }
}