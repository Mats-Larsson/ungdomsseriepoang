using Results.Model;

namespace Results.Simulator;

internal class SimulatorResultSource : IResultSource, IDisposable
{
    private readonly SimulatedParticipant[] simulatedParticipants;
    private readonly CancellationTokenSource cancellationTokenSource = new();
    private TimeSpan currentTimeOfDay = TimeSpan.Zero;
    public int SpeedMultiplier { get; }
    public CancellationToken CancellationToken { get; }
    public TimeSpan MinTime { get; }
    public TimeSpan MaxTime { get; }
    public TimeSpan ZeroTime { get; }
    public TimeSpan CurrentTimeOfDay => currentTimeOfDay;

    public SimulatorResultSource()
    {
        MinTime = TestData.TemplateParticipantResults
            .Where(p => p.StartTime.HasValue && p.StartTime.Value != TimeSpan.Zero && p.Status != ParticipantStatus.Ignored)
            .Min(p => p.StartTime!.Value);
        MaxTime = TestData.TemplateParticipantResults
            .Where(p => p.StartTime.HasValue && p.Time.HasValue && p.Status != ParticipantStatus.Ignored)
            .Max(p => p.StartTime!.Value.Add(p.Time!.Value));
        ZeroTime = MinTime.Subtract(TimeSpan.FromMinutes(15));

        SpeedMultiplier = 10;
        simulatedParticipants = TestData.TemplateParticipantResults
            .Select(r => new SimulatedParticipant(this, r))
            .ToArray();

        CancellationToken = cancellationTokenSource.Token;

        _ = RunClockAsync();
        foreach (var pr in simulatedParticipants)
        {
            pr.Task = pr.RunAsync();
        }
    }

    private async Task RunClockAsync()
    {
        currentTimeOfDay = ZeroTime;
        while (currentTimeOfDay <= MaxTime)
        {
            currentTimeOfDay = currentTimeOfDay.Add(TimeSpan.FromSeconds(1).Divide(SpeedMultiplier));
            await Task.Delay(TimeSpan.FromSeconds(1), CancellationToken);
        }
    }

    public IList<ParticipantResult> GetParticipantResults()
    {
        return simulatedParticipants.Cast<ParticipantResult>().ToList();
    }

    public string Status => $"{simulatedParticipants.Count(pr => pr.Task?.Status == TaskStatus.RanToCompletion)} / {simulatedParticipants.Length} ";

    public void Dispose()
    {
        cancellationTokenSource.Cancel();
        cancellationTokenSource.Dispose();
    }
}
