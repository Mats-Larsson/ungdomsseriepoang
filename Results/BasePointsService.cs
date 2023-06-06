using CsvHelper.Configuration;
using CsvHelper;
using Results.Contract;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Results.Model;
using Results.Simulator;

namespace Results;

public class BasePointsService : IBasePointsService
{
    private readonly Dictionary<string, int> basePoints = new();

    public BasePointsService(Configuration configuration, ILogger<BasePointsService> logger, IResultSource resultSource)
    {
        if (configuration == null) throw new ArgumentNullException(nameof(configuration));
        if (resultSource == null)  throw new ArgumentNullException(nameof(resultSource));

        var filePath = configuration.BasePointsFilePath;
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false
        };
        if (File.Exists(filePath))
        {
            logger.LogInformation("Using base result file: {}", filePath);

            try
            {
                using var reader = new StreamReader(filePath);
                using var csv = new CsvReader(reader, config);
                var records = csv.GetRecords<BasePoints>();
                basePoints = records.ToDictionary(br => br.Team, br => br.Points);
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
        basePoints = clubs.ToDictionary(c => c, _ => Random.Shared.Next(0, 200));
    }

    public IDictionary<string, int> GetBasePoints()
    {
        return basePoints;
    }
}