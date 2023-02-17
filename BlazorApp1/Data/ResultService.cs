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
    }
public Task<TeamResult[]> GetTeamResultsAsync()
    {
        return Task.FromResult(resultsource.GetScoreBoard().ToArray());
    }
}