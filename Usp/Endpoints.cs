using System.Globalization;
using System.IO.Abstractions;
using System.Xml;
using Microsoft.AspNetCore.Http.HttpResults;
using ResultCalc;
using ResultCalc.Contract;

namespace Usp;

internal class Endpoints(IResultService resultService, Configuration configuration, IFileSystem fileSystem, ILogger<Endpoints> logger)
{
    public async Task<IResult> NewResultPostAsync(HttpRequest httpRequest)
    {
        try
        {
            var timestamp = DateTime.Now;
            var body = httpRequest.Body;
            if (configuration.PostLogFolder != null)
            {
                body = await SavePostBodyAsync(body, configuration.PostLogFolder, timestamp).ConfigureAwait(false);
            }

            string result = await resultService.NewResultPostAsync(body, timestamp).ConfigureAwait(false);
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

    /// <summary>
    /// Saves the body to e.g. "2026-09-27_14.05.09.123.xml" in <paramref name="folder"/> and returns a copy of it
    /// to be processed, since the request body can only be read once.
    /// </summary>
    private async Task<Stream> SavePostBodyAsync(Stream body, string folder, DateTime timestamp)
    {
        var copy = new MemoryStream();
        await body.CopyToAsync(copy).ConfigureAwait(false);
        copy.Position = 0;

        try
        {
            fileSystem.Directory.CreateDirectory(folder);
            var fileName = timestamp.ToString("yyyy-MM-dd_HH.mm.ss.fff", CultureInfo.InvariantCulture) + ".xml";
            var path = fileSystem.Path.Combine(folder, fileName);
            await using (var file = fileSystem.File.Create(path))
            {
                await copy.CopyToAsync(file).ConfigureAwait(false);
            }
            logger.LogDebug("Saved post body to {Path}", path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Saving is only for troubleshooting, the post is still processed
            logger.LogWarning(ex, "Could not save post body to {Folder}", folder);
        }

        copy.Position = 0;
        return copy;
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