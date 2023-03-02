using System.Collections.Immutable;
using MySql.Data.MySqlClient;
using Results.Model;

namespace Results.Ola;

internal class OlaResultSource : IResultSource
{
    private readonly string connectionString;

    public OlaResultSource(string host, int? port, string database, string user, string password)
    {
        connectionString = $"server={host};port={port ?? 3306};userid={user};password={password};database={database}";
    }

    public IList<ParticipantResult> GetParticipantResults()
    {
        using var con = new MySqlConnection(connectionString);
        con.Open();

        using var cmd = new MySqlCommand(Resource1.ParticipantResultSql, con);
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
}