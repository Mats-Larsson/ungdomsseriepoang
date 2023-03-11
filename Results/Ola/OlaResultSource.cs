using System.Collections.Immutable;
using System.Data.SqlTypes;
using System.Net.NetworkInformation;
using MySql.Data.MySqlClient;
using Results.Model;

namespace Results.Ola;

public sealed class OlaResultSource : IResultSource
{
    private readonly Configuration configuration;
    private readonly string connectionString;

    public OlaResultSource(Configuration configuration)
    {
        this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        connectionString = $"server={configuration.OlaMySqlHost};" +
                           $"port={configuration.OlaMySqlPort};" +
                           $"userid={configuration.OlaMySqlUser};" +
                           $"password={configuration.OlaMySqlPassword};" +
                           $"database={configuration.OlaMySqlDatabase}";
    }

    public IList<ParticipantResult> GetParticipantResults()
    {
        var list = GetFromOlaDb();
        var extras = GetExtraParticipants(list);

        return list
            .Select(o => new ParticipantResult(o.Class, o.Name, o.Club, o.StartTime, o.Time, o.Status, extras.Contains(o.Id)))
            .ToList();
    }

    // TODO: Hämta från OLA
    public TimeSpan CurrentTimeOfDay => DateTime.Now - DateTime.Now.Date;
    public Task<string> NewResultPostAsync(Stream body, DateTime timestamp)
    {
        throw new NotImplementedException();
    }

    private IList<OlaParticipantResult> GetFromOlaDb()
    {
        using var con = new MySqlConnection(connectionString);
        con.Open();

#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
        using var cmd = new MySqlCommand(Resource1.ParticipantResultSql, con);
        cmd.Parameters.AddWithValue("@EventId", configuration.OlaEventId);
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
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
            var parMedId = reader.IsDBNull(7) ? null : reader.GetFieldValue<int?>(7);

            list.Add(new OlaParticipantResult(@class, name, club, startTime, time, ToParticipantStatus(olaStatus, time), id, parMedId));
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

    // TODO: Klarar endast patruller med två deltagare, med 100% säkerhet. Detta är även en begränsning i Eventor, samt till viss del i OLA.
    internal static ISet<int> GetExtraParticipants(IList<OlaParticipantResult> list)
    {
        var linked = list
            .Where(opr => opr.TogetherWithId.HasValue)
            .Select(opr => new
            {
                Opr = opr,
                Id2 = opr.TogetherWithId!.Value,
                Id2Status = list // TODO: Byt till Dictionary
                    .Where(x => x.Id == opr.TogetherWithId)
                    .Select(x =>  x.Status)
                    .FirstOrDefault(ParticipantStatus.Ignored)
            })
            //.Where(opr => opr.Id2Status >= ParticipantStatus.Activated)
            .Select(ops => new { Id1 = ops.Opr.Id, Id1Status = ops.Opr.Status, ops.Id2, ops.Id2Status })
            .ToHashSet()!;

        // Make sure all are double linked
        foreach (var pair in linked.ToList())
        {
            linked.Add(new { Id1 = pair.Id2, Id1Status = pair.Id2Status, Id2 = pair.Id1, Id2Status = pair.Id1Status });
        }
        var groups = new List<ISet<int>>();
        foreach (var pair in linked)
        {
            foreach (var group in groups)
            {
                if (group.Contains(pair.Id1) && pair.Id2Status >= ParticipantStatus.Activated) {
                    group.Add(pair.Id2);
                    goto inGroup;
                }
                if (group.Contains(pair.Id2) && pair.Id1Status >= ParticipantStatus.Activated)
                {
                    group.Add(pair.Id1);
                    goto inGroup;
                }
            }

            var newGroup = new HashSet<int>();
            if (pair.Id1Status >= ParticipantStatus.Activated) newGroup.Add(pair.Id1);
            if (pair.Id2Status >= ParticipantStatus.Activated) newGroup.Add(pair.Id2);
            if (newGroup.Any()) groups.Add(newGroup);
        inGroup:
            ;
        }

        var extras = groups
            .SelectMany(g => g.Skip(1))
            .ToHashSet();
        return extras;
    }

    public void Dispose()
    {
        // Nothing to dispose
    }
}

internal class OlaParticipantResult
{
    public string Class { get; }
    public string Name { get; }
    public string Club { get; internal set; }
    public TimeSpan? StartTime { get; }
    public TimeSpan? Time { get; internal set; }
    public ParticipantStatus Status { get; internal set; }
    public int Id { get; }
    public int? TogetherWithId { get; }

    public OlaParticipantResult(string @class, string name, string club, TimeSpan? startTime, TimeSpan? time, ParticipantStatus status, int id, int? togetherWithId)
    {
        Class = @class;
        Name = name;
        Club = club;
        StartTime = startTime;
        Time = time;
        Status = status;
        Id = id;
        TogetherWithId = togetherWithId;
    }
}