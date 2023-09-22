using Microsoft.Extensions.Logging;
using Results.Simulator;

namespace Results
{
    public class SimulatorTeamService : TeamService
    {
        public SimulatorTeamService(Configuration configuration, ILogger<TeamService> logger) : base(configuration, logger)
        {
            TeamBasePoints = TestData.Clubs.Take(configuration.NumTeams).ToDictionary(c => c, _ => Random.Shared.Next(0, 200));
        }
    }
}