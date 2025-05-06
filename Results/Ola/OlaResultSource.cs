using System.Diagnostics.CodeAnalysis;
using MySql.Data.MySqlClient;
using Results.Contract;
using Results.Model;

namespace Results.Ola;

public sealed class OlaResultSource(Configuration configuration) : IResultSource
{
    private readonly Configuration configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    private readonly string connectionString = $"server={configuration.OlaMySqlHost};" +
                                               $"port={configuration.OlaMySqlPort};" +
                                               $"userid={configuration.OlaMySqlUser};" +
                                               $"password={configuration.OlaMySqlPassword};" +
                                               $"database={configuration.OlaMySqlDatabase}";

    public bool SupportsPreliminary => true;

    public IList<ParticipantResult> GetParticipantResults()
    {
        var list = GetFromOlaDb();
        const string competitionName = ""; // TODO: Get name from OLA

        return list
            .Select(o => new ParticipantResult(competitionName, o.Class, o.Name, o.Club, o.StartTime, o.Time, o.Status))
            .ToList();
    }

    // TODO: Hämta från OLA
    public TimeSpan CurrentTimeOfDay => DateTime.Now - DateTime.Now.Date;
    public Task<string> NewResultPostAsync(Stream body, DateTime timestamp)
    {
        throw new InvalidOperationException();
    }

    private IEnumerable<OlaParticipantResult> GetFromOlaDb()
    {
        using var con = new MySqlConnection(connectionString);
        con.Open();

        using var cmd = new MySqlCommand(Resource1.ParticipantResultSql, con);
        cmd.Parameters.AddWithValue("@EventId", configuration.OlaEventId);

        using var reader = cmd.ExecuteReader();
        var list = new List<OlaParticipantResult>();
        while (reader.Read())
        {
            var @class = reader.GetFieldValue<string>(0);
            var name = reader.GetFieldValue<string>(1);
            var club = reader.GetFieldValue<string>(2);
            var startTime = reader.IsDBNull(3) ? null : new TimeSpan?(reader.GetFieldValue<TimeSpan>(3));
            var time = reader.IsDBNull(4) ? null : new TimeSpan?(reader.GetFieldValue<TimeSpan>(4));
            var olaStatus = reader.GetFieldValue<string>(5);
            var id = reader.GetFieldValue<int>(6);

            list.Add(new OlaParticipantResult(@class, name, club, startTime, time, ToParticipantStatus(olaStatus, time), id));
        }
        return list;
    }
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

[SuppressMessage("ReSharper", "UnusedMember.Global")]
internal class OlaParticipantResult(
    string @class,
    string name,
    string club,
    TimeSpan? startTime,
    TimeSpan? time,
    ParticipantStatus status,
    int id)
{
    public string Class { get; } = @class;
    public string Name { get; } = name;
    public string Club { get; } = club;
    public TimeSpan? StartTime { get; } = startTime;
    public TimeSpan? Time { get; } = time;
    public ParticipantStatus Status { get; } = status;

    public int Id { get; } = id;
}