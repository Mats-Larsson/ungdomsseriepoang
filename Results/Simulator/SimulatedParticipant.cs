using System.Diagnostics.CodeAnalysis;
using Results.Contract;
using Results.Model;

namespace Results.Simulator
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    internal record SimulatedParticipant : ParticipantResult
    {
        private readonly TimeSpan? startTime;
        private readonly TimeSpan targetTime;
        private readonly ParticipantStatus targetStatus;
        private readonly SimulatorResultSource simulator;

        internal bool SimulationDone { get; private set; }
        public Task? Task { get; internal set; }

        public SimulatedParticipant(SimulatorResultSource simulator, ParticipantResult pr)
            : base(pr.CompititionName, pr.Class, pr.Name, pr.Club, pr.StartTime, null, pr.Status == ParticipantStatus.Ignored ? ParticipantStatus.Ignored : ParticipantStatus.NotActivated)
        {
            this.simulator = simulator;
            startTime = pr.StartTime.HasValue ? (pr.StartTime - simulator.ZeroTime)/ simulator.SpeedMultiplier : null;
            targetTime = pr.Time ?? TimeSpan.FromSeconds(10 * 60 + Random.Shared.Next(0, 30 * 60)) / simulator.SpeedMultiplier;
            targetStatus = pr.Status;
        }

        public async Task RunAsync()
        {
            while (Status != targetStatus && !simulator.TokenSource.IsCancellationRequested)
            {
                // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                switch (Status)
                {
                    case ParticipantStatus.NotActivated:
                        if (targetStatus == ParticipantStatus.NotStarted)
                            goto SimulationDone;
                        await SimulatedDelay(startTime!.Value).ConfigureAwait(false);
                        Status = ParticipantStatus.Started;
                        break;
                    // TODO: Hantera även Checkad. Är nog samma som Started i OLA.
                    case ParticipantStatus.Started:
                        await SimulatedDelay(targetTime).ConfigureAwait(false);
                        Status = ParticipantStatus.Preliminary;
                        Time = targetTime;
                        break;
                    case ParticipantStatus.Preliminary:
                        await SimulatedDelay(TimeSpan.FromSeconds(30)).ConfigureAwait(false);
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
            return Task.Delay(timeSpan / simulator.SpeedMultiplier).WaitAsync(simulator.TokenSource.Token);
        }
    }
}
