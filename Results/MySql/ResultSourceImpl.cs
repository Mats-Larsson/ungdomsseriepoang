using System.Collections.Immutable;
using System.Timers;
using MySql.Data.MySqlClient;

namespace Results.MySql;

internal class ResultSourceImpl : IResultSource, IDisposable
{
    private readonly string connectionString;
    private IList<ParticipantResult>? participantResults;
    private readonly System.Timers.Timer timer;

    public ResultSourceImpl(string host, int? port, string database, string user, string password)
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
            ParticipantResultsLocal();
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }

    public IList<ParticipantResult> GetParticipantResults()
    {
        if (participantResults == null)
            ParticipantResultsLocal();
        if (participantResults == null)
            throw new InvalidOperationException();

        return participantResults;
    }

    private void ParticipantResultsLocal()
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
            var time = reader.IsDBNull(3) ? null : new TimeSpan?(reader.GetFieldValue<TimeSpan>(3));
            var status = ToParticipantStatus(reader.GetFieldValue<string>(4), time);

            list.Add(new ParticipantResult(@class, name, club, time, status));
        }

        participantResults = ImmutableList.Create(list.ToArray());
    }

    public event EventHandler? NyaResult;

    private void ThereAreNewResults()
    {
        OnNewResults();
    }

    public void Dispose()
    {
        timer.Dispose();
    }

    private void OnNewResults()
    {
        NyaResult?.Invoke(this, EventArgs.Empty);
    }

    private static ParticipantStatus ToParticipantStatus(string olaStatus, TimeSpan? time)
    {
        return olaStatus switch
        {
            "walkOver" => ParticipantStatus.Ignored,
            "movedUp" => ParticipantStatus.Ignored,
            "notParticipating" => ParticipantStatus.Ignored,

            "notActivated" => ParticipantStatus.NotStarted,
            "notStarted" => ParticipantStatus.NotStarted,

            "started" => ParticipantStatus.Started,
            "disqualified" => ParticipantStatus.Started,
            "notValid" => ParticipantStatus.Started,

            "finished" => time.HasValue ? ParticipantStatus.Preliminary : ParticipantStatus.Started,
            "finishedTimeOk" => time.HasValue ? ParticipantStatus.Preliminary : ParticipantStatus.Started,
            "finishedPunchOk" => time.HasValue ? ParticipantStatus.Preliminary : ParticipantStatus.Started,

            "passed" => time.HasValue ? ParticipantStatus.Passed : ParticipantStatus.Started,
            _ => throw new InvalidOperationException($"Unexpected status: {olaStatus}")
        };
    }
}