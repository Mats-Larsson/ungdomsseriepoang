using Results.Contract;

namespace BlazorApp1.Data;

public class ResultService
{
    private readonly IResultService resultService;

    public ResultService()
    {
        resultService = new Results.ResultService();

        resultService.OnNewResults += ResultService_OnNewResults;
    }

    private static void ResultService_OnNewResults(object? sender, EventArgs e)
    {
        Console.Out.WriteLine("Event");
    }

    public Task<Result> GetTeamResultsAsync()
    {
        return Task.FromResult(resultService.GetScoreBoard());
    }
}