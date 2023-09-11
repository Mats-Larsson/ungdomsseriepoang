using CsvHelper.Configuration;
using CsvHelper;
using Results.Contract;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Results.Model;
using Results.Simulator;

namespace Results;

public class TeamService : ITeamService
{
    private readonly Dictionary<string, int> teamBasePoints = new();

    public TeamService(Configuration configuration, ILogger<TeamService> logger, IResultSource resultSource)
    {
        if (configuration == null) throw new ArgumentNullException(nameof(configuration));
        if (resultSource == null)  throw new ArgumentNullException(nameof(resultSource));

        var filePath = configuration.TeamsFilePath;
        if (File.Exists(filePath))
        {
            logger.LogInformation("Using base result file: {}", filePath);

            try
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = false, 
                    MissingFieldFound = null
                };
                using var reader = new StreamReader(filePath);
                using var csv = new CsvReader(reader, config);
                var records = csv.GetRecords<Team>().ToList();
                teamBasePoints = records.ToDictionary(br => br.Name, br => br.BasePoints ?? 0);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Could not parse base results file.");
                throw;
            }
            return;
        }

        logger.LogInformation("Base result file: {} not found", filePath);

        if (resultSource is not SimulatorResultSource) return;

        var clubs = resultSource.GetParticipantResults().Select(pr => pr.Club).Distinct().ToList();
        teamBasePoints = clubs.ToDictionary(c => c, _ => Random.Shared.Next(0, 200));
    }

    public IDictionary<string, int> GetTeamBasePoints()
    { 
        return teamBasePoints;
    }
}