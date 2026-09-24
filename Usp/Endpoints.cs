using System.Xml;
using Microsoft.AspNetCore.Http.HttpResults;
using Results.Contract;

namespace Usp;

internal class Endpoints(IResultService resultService, ILogger<Endpoints> logger)
{
    public async Task<IResult> NewResultPostAsync(HttpRequest httpRequest)
    {
        try
        {
            string result = await resultService.NewResultPostAsync(httpRequest.Body, DateTime.Now).ConfigureAwait(false);
            return TypedResults.Text(result, contentType: "application/xml");
        }
        catch (Exception ex) when (ex is XmlException or FormatException or BadHttpRequestException)
        {
            // The posted data could not be read or parsed
            logger.LogWarning(ex, "Invalid result post from {RemoteIp}, ContentLength={ContentLength}",
                httpRequest.HttpContext.Connection.RemoteIpAddress, httpRequest.ContentLength);
            return TypedResults.BadRequest();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to handle result post from {RemoteIp}, ContentLength={ContentLength}",
                httpRequest.HttpContext.Connection.RemoteIpAddress, httpRequest.ContentLength);
            return TypedResults.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    public ContentHttpResult GetTeamsResult()
    {
        var teamResults = resultService.GetScoreBoard().TeamResults;

        return TypedResults.Text(Helper.ToCsvText(teamResults), contentType: "text/csv");
    }

    public ContentHttpResult GetParticipantsResult()
    {
        var participantPointsList = resultService.GetParticipantPointsList();

        return TypedResults.Text(Helper.ToCsvText(participantPointsList), contentType: "text/csv");
    }
}