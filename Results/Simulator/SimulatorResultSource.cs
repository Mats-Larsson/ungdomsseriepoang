using Results.Model;

namespace Results.Simulator;

internal class SimulatorResultSource : IResultSource, IDisposable
{
    private readonly SimulatedParticipant[] simulatedParticipants;
    private readonly CancellationTokenSource cancellationTokenSource = new();
    public int SpeedMultiplier { get; }
    public CancellationToken CancellationToken { get; }

    public SimulatorResultSource()
    {
        SpeedMultiplier = 10;
        simulatedParticipants = TestData.TemplateParticipantResults
            .Select(r => new SimulatedParticipant(this, r))
            .ToArray();
        CancellationToken = cancellationTokenSource.Token;

        foreach (var pr in simulatedParticipants)
        {
            pr.Task = pr.RunAsync();
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
