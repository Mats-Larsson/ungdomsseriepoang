using Results;
using Results.Contract;

namespace BlazorApp1.Data;

public class ResultService
{
    private IResultService resultsource;

    public ResultService()
    {
        resultsource = new Results.ResultService();

        resultsource.OnNewResults += Resultsource_OnNewResults;
    }

    private void Resultsource_OnNewResults(object? sender, EventArgs e)
    {
        Console.Out.WriteLine("Event");
    }

    public Task<Result> GetTeamResultsAsync()
    {
        return Task.FromResult(resultsource.GetScoreBoard());
    }
}