using Results.Contract;

namespace BlazorApp1.Data;

public class ResultService
{
    private readonly IResultService resultService;
    public event EventHandler? OnNewResults;

    public ResultService(IResultService resultService)
    {
        this.resultService = resultService ?? throw new ArgumentNullException(nameof(resultService));

        resultService.OnNewResults += ResultService_OnNewResults;
    }


    public Task<Result> GetTeamResultsAsync()
    {
        return Task.FromResult(resultService.GetScoreBoard());
    }


    private void ResultService_OnNewResults(object? sender, EventArgs e)
    {
        OnNewResults?.Invoke(sender, e);
    }
}