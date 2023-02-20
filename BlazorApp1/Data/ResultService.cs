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

        resultsource.OnNyaResultat += Resultsource_OnNyaResultat;
    }

    private void Resultsource_OnNyaResultat(object? sender, EventArgs e)
    {
        NewResutls?.Invoke(this, new EventArgs());
    }

    public Task<TeamResult[]> GetTeamResultsAsync()
    {
        return Task.FromResult(resultsource.GetScoreBoard().ToArray());
    }

    public event EventHandler? NewResutls;
}