using CsvHelper.Configuration;
using CsvHelper;
using Results.Contract;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Results.Model;

namespace Results;

public class BasePointsService : IBasePointsService
{
    private readonly Dictionary<string, int> basePoints = new();

    public BasePointsService(Configuration configuration, ILogger<BasePointsService> logger)
    {
        if (configuration == null) throw new ArgumentNullException(nameof(configuration));

        var filePath = configuration.BasePointsFilePath;
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false
        };
        if (!File.Exists(filePath))
        {
            logger.LogInformation("Base resulte file: {} not found", filePath);
            return;
        }
        logger.LogInformation("Using base resulte file: {}", filePath);

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
    }

    public IDictionary<string, int> GetBasePoints()
    {
        return basePoints;
    }
}