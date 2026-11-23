using System.Diagnostics.CodeAnalysis;
using MySql.Data.MySqlClient;
using ResultCalc.Contract;
using ResultCalc.Model;

namespace ResultCalc.Ola;

internal sealed class OlaResultSource(Configuration configuration, TimeProvider timeProvider) : IResultSource
{
    private readonly Configuration configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    private readonly string connectionString = $"server={configuration.OlaMySqlHost};" +
                                               $"port={configuration.OlaMySqlPort};" +
                                               $"userid={configuration.OlaMySqlUser};" +
                                               $"password={configuration.OlaMySqlPassword};" +
                                               $"database={configuration.OlaMySqlDatabase}";

    public bool SupportsPreliminary => true;

    public async Task<IList<ParticipantResult>> GetParticipantResultsAsync()
    {
        var list = await GetFromOlaDbAsync().ConfigureAwait(false);
        const string competitionName = ""; // TODO: Get name from OLA

        return [.. list.Select(o => new ParticipantResult(competitionName, o.Class, o.Name, o.Club, o.StartTime, o.Time, o.Status))];
    }

    // TODO: Hämta från OLA
    public Task<TimeSpan> GetCurrentTimeOfDayAsync() => Task.FromResult(timeProvider.GetLocalNow().TimeOfDay);
    public Task<string> NewResultPostAsync(Stream body, DateTime timestamp)
    {
        throw new InvalidOperationException();
    }

    private async Task<IEnumerable<OlaParticipantResult>> GetFromOlaDbAsync()
    {
        var con = new MySqlConnection(connectionString);
        await using var conAsyncDisposable = con.ConfigureAwait(false);
        await con.OpenAsync().ConfigureAwait(false);

        var cmd = new MySqlCommand(Resource1.ParticipantResultSql, con);
        await using var cmdAsyncDisposable = cmd.ConfigureAwait(false);
        cmd.Parameters.AddWithValue("@EventId", configuration.OlaEventId);

        var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
        await using var readerAsyncDisposable = reader.ConfigureAwait(false);
        var list = new List<OlaParticipantResult>();
        while (await reader.ReadAsync().ConfigureAwait(false))
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
            "walkOver" or "movedUp" or "notParticipating" => ParticipantStatus.Ignored,

            "notActivated" => ParticipantStatus.NotActivated,

            "started" => ParticipantStatus.Started,

            "finished" => time.HasValue ? ParticipantStatus.Preliminary : ParticipantStatus.Started,
            "finishedTimeOk" => time.HasValue ? ParticipantStatus.Preliminary : ParticipantStatus.Started,
            "finishedPunchOk" => time.HasValue ? ParticipantStatus.Preliminary : ParticipantStatus.Started,

            "passed" => time.HasValue ? ParticipantStatus.Passed : ParticipantStatus.Started,
            "disqualified" or "notValid" => ParticipantStatus.NotValid,
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