using System.Threading;
using System.Timers;
using Org.BouncyCastle.Bcpg.Sig;
using Results.Model;

namespace Results.Simulator;

internal class SimulatorResultSourceImpl : IResultSource, IDisposable
{
    private readonly SimultatedParticipent[] simultatedParticipents;
    private readonly CancellationTokenSource source = new CancellationTokenSource();
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
internal class SimultatedParticipent : ParticipantResult
{
    private readonly TimeSpan startTime;
    private readonly TimeSpan targetTime;
    private readonly ParticipantStatus targetStatus;
    private readonly SimulatorResultSourceImpl simulator;

    public bool SimulationDone { get; private set; } = false;
    public Task? Task { get; internal set; }

    public SimultatedParticipent(SimulatorResultSourceImpl simulator, ParticipantResult pr)
        : base(pr.Class, pr.Name, pr.Club, null, pr.Status == ParticipantStatus.Ignored ? ParticipantStatus.Ignored : ParticipantStatus.NotActivated)
    {
        this.simulator = simulator;
        startTime = TimeSpan.FromMinutes(Random.Shared.Next(0, 60)) / simulator.SpeedMultiplier;
        targetTime = pr.Time ?? TimeSpan.FromSeconds(10 * 60 + Random.Shared.Next(0, 30 * 60)) / simulator.SpeedMultiplier;
        targetStatus = pr.Status;
    }

    public async Task RunAsync()
    {
        while (Status != targetStatus && !simulator.CancellationToken.IsCancellationRequested)
        {
            switch (Status)
            {
                case ParticipantStatus.NotActivated:
                    if (targetStatus == ParticipantStatus.NotStarted)
                        goto SimulationDone;
                    await SimulatedDelay(startTime);
                    Status = ParticipantStatus.Started;
                    break;
                case ParticipantStatus.Started:
                    await SimulatedDelay(targetTime);
                    Status = ParticipantStatus.Preliminary;
                    Time = targetTime;
                    break;
                case ParticipantStatus.Preliminary:
                    await SimulatedDelay(TimeSpan.FromSeconds(30));
                    Time = targetStatus == ParticipantStatus.Passed ? targetTime : null;
                    Status = targetStatus;
                    goto SimulationDone;
            }
        }
SimulationDone:
        SimulationDone = true;
    }

    private Task SimulatedDelay(TimeSpan timeSpan)
    {
        return Task.Delay(timeSpan / simulator.SpeedMultiplier).WaitAsync(simulator.CancellationToken);
    }
}
