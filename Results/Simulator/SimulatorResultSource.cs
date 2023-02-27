using System.Timers;
using Results.Model;

namespace Results.Simulator;

internal class SimulatorResultSource : IResultSource, IDisposable
{
    private readonly System.Timers.Timer timer;

    private readonly ParticipantResult[] participantResults;

    public SimulatorResultSource()
    {
        participantResults = InitParticipantResults();

        timer = new System.Timers.Timer(TimeSpan.FromSeconds(1).TotalMilliseconds);
        // Hook up the Elapsed event for the timer. 
        timer.Elapsed += OnTimedEvent;
        timer.AutoReset = true;
        timer.Enabled = true;
    }

    public IList<ParticipantResult> GetParticipantResults()
    {
        return participantResults.ToArray();
    }

    private ParticipantResult[] InitParticipantResults()
    {
        return TestData.TemplateParticipantResults
            .Select(r => new ParticipantResult(
                r.Class, 
                r.Name, 
                r.Club, 
                null,
                r.Status == ParticipantStatus.Ignored ? ParticipantStatus.Ignored : ParticipantStatus.NotStarted
            ))
            .ToArray();
    }

    private void OnTimedEvent(object? sender, ElapsedEventArgs e)
    {
        for (var n = 0; n < participantResults.Length * 10; n++)
        {
            var i = Random.Shared.Next(0, participantResults.Length);
            var tpr = TestData.TemplateParticipantResults[i];
            var pr = participantResults[i];
            if (pr.Status == tpr.Status) continue;
            pr.Status++;
            if (pr.Status < ParticipantStatus.Preliminary) break;
            pr.Time = tpr.Time;

        }
    }

    public void Dispose()
    {
        timer.Dispose();
    }

}