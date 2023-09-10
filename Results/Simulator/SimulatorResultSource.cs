using Results.Contract;
using Results.Model;

namespace Results.Simulator;

public sealed class SimulatorResultSource : IResultSource
{
    private readonly Configuration configuration;
    private readonly SimulatedParticipant[] simulatedParticipants;
    private readonly TestData testData;
    internal CancellationTokenSource TokenSource { get; } = new();
    private TimeSpan currentTimeOfDay = TimeSpan.Zero;
    public int SpeedMultiplier { get; }
    public TimeSpan MinTime { get; }
    public TimeSpan MaxTime { get; }
    public TimeSpan ZeroTime { get; }
    public TimeSpan CurrentTimeOfDay => currentTimeOfDay;
    public Task<string> NewResultPostAsync(Stream body, DateTime timestamp)
    {
        throw new NotImplementedException();
    }

    public SimulatorResultSource(Configuration configuration)
    {
        this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        testData = new TestData(configuration.NumTeams);

        SpeedMultiplier = this.configuration.SpeedMultiplier;

        MinTime = testData._templateParticipantResults
            .Where(p => p.StartTime.HasValue && p.StartTime.Value != TimeSpan.Zero && p.Status != ParticipantStatus.Ignored)
            .Min(p => p.StartTime!.Value);
        MaxTime = testData._templateParticipantResults
            .Where(p => p is { StartTime: not null, Time: not null, Status: not ParticipantStatus.Ignored })
            .Max(p => p.StartTime!.Value.Add(p.Time!.Value));
        ZeroTime = MinTime.Subtract(TimeSpan.FromMinutes(15));

        simulatedParticipants = testData._templateParticipantResults
            .Select(r => new SimulatedParticipant(this, r))
            .ToArray();

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
            currentTimeOfDay = currentTimeOfDay.Add(TimeSpan.FromSeconds(1));
            await Task.Delay(TimeSpan.FromSeconds(1).Divide(SpeedMultiplier), TokenSource.Token).ConfigureAwait(false);
        }
    }

    public bool SupportsPreliminary => true;

    public IList<ParticipantResult> GetParticipantResults()
    {
        return simulatedParticipants.Cast<ParticipantResult>().ToList();
    }

    public string Status => $"{simulatedParticipants.Count(pr => pr.Task?.Status == TaskStatus.RanToCompletion)} / {simulatedParticipants.Length} ";

    public void Dispose()
    {
        TokenSource.Dispose();
    }
}
