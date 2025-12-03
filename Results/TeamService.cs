using CsvHelper.Configuration;
using CsvHelper;
using Results.Contract;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Results.Model;

namespace Results;

public class TeamService : ITeamService
{
    public IDictionary<string, int> TeamBasePoints { get; protected init; } = new Dictionary<string, int>();

    public TeamService(Configuration configuration, ILogger<TeamService> logger)
    {
        ArgumentNullException.ThrowIfNull(configuration);

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
                TeamBasePoints = records.ToDictionary(br => br.Name, br => br.BasePoints ?? 0);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Could not parse base results file.");
                throw;
            }
            return;
        }

        logger.LogInformation("Base result file: {} not found", filePath);

    }

    public ICollection<string>? Teams
    {
        get
        {
            return TeamBasePoints.Any() ? TeamBasePoints.Keys : null;
        }
    }
}