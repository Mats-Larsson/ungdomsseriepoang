using Results;
using Results.Contract;
using Results.Simulator;

namespace BlazorApp1.Data;

public class ResultService
{
    private IResults resultsource;

    public ResultService()
    {
        resultsource = new ResultsImpl();

        resultsource.OnNewResults += Resultsource_OnNewResults;
    }

    private void Resultsource_OnNewResults(object? sender, EventArgs e)
    {
        Console.Out.WriteLine("Event");
    }

    public Task<TeamResult[]> GetTeamResultsAsync()
    {
        return Task.FromResult(resultsource.GetScoreBoard().TeamResults.ToArray());
    }
}