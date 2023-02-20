using Results;
using Results.Contract;

namespace BlazorApp1.Data;

public class ResultService
{
    private readonly IResults resultSource;

    public ResultService()
    {
        resultSource = new ResultsImpl();

        resultSource.OnNewResults += ResultSource_OnNewResults;
    }

    private void ResultSource_OnNewResults(object? sender, EventArgs e)
    {
        Console.Out.WriteLine("Event");
    }

    public Task<TeamResult[]> GetTeamResultsAsync()
    {
        return Task.FromResult(resultSource.GetScoreBoard().ToArray());
    }
}