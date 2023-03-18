using CsvHelper.Configuration;
using CsvHelper;
using Results.Contract;
using System.Globalization;
using Microsoft.Extensions.Logging;

namespace Results;

public class BaseResultService : IBaseResultService
{
    private readonly Dictionary<string, int> baseResults = new();

    public BaseResultService(string filePath, ILogger<BaseResultService> logger)
    {

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            PrepareHeaderForMatch = args => args.Header.ToLower(),
        };
        if (!File.Exists(filePath))
        {
            logger.LogInformation("Base resulte file: {0} not found", filePath);
            return;
        }
        logger.LogInformation("Using base resulte file: {0}", filePath);

        try
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, config);
            var records = csv.GetRecords<BaseResult>();
            baseResults = records.ToDictionary(br => br.Team, br => br.Points);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not parse base results file.");
            throw;
        }
    }

    public IDictionary<string, int> GetBaseResults()
    {
        return baseResults;
    }
}

class BaseResult
{
    public string Team { get; }
    public int Points { get; }

    public BaseResult(string team, int points)
    {
        Team = team;
        Points = points;
    }
}