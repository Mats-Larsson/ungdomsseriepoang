using Results.Model;

namespace Results.Simulator;

internal class SimulatorResultSourceImpl : IResultSource, IDisposable
{
    private readonly SimultatedParticipent[] simultatedParticipents;
    private readonly CancellationTokenSource source = new();
    public int SpeedMultiplier { get; private set; }
    public CancellationToken CancellationToken { get; private set; }

    public SimulatorResultSourceImpl()
    {
        SpeedMultiplier = 10;
        simultatedParticipents = TestData.TemplateParticipantResults
            .Select(r => new SimultatedParticipent(this, r))
            .ToArray();
        CancellationToken = source.Token;

        foreach (var pr in simultatedParticipents)
        {
            pr.Task = pr.RunAsync();
        }
    }

    public IList<ParticipantResult> GetParticipantResults()
    {
        return simultatedParticipents;
    }

    public string Status => $"{simultatedParticipents.Count(pr => pr.Task?.Status == TaskStatus.RanToCompletion)} / {simultatedParticipents.Length} ";

    public void Dispose()
    {
        source.Cancel();
        source.Dispose();
    }
}
