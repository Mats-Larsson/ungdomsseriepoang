using Results.Contract;

namespace Usp;

internal class Endpoints(IResultService resultService)
{
    public async Task NewResultPostAsync(HttpRequest httpRequest)
    {
        await resultService.NewResultPostAsync(httpRequest.Body, DateTime.Now).ConfigureAwait(false);
    }

    public Task GetTeamsResultAsync(HttpContext context)
    {
        var teamResults = resultService.GetScoreBoard().TeamResults;

        return Microsoft.AspNetCore.Http.Results
            .Content(Helper.ToCsvText(teamResults), contentType: "text/csv")
            .ExecuteAsync(context);
    }
    
    public Task GetParticipantsResultAsync(HttpContext context)
    {
        var participantPointsList = resultService.GetParticipantPointsList();

        return Microsoft.AspNetCore.Http.Results.Content(Helper.ToCsvText(participantPointsList), contentType: "text/csv")
            .ExecuteAsync(context);
    }
}