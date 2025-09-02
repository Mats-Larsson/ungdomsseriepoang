using System.Web;

namespace Results.Eventor;

public sealed class EventorFacade : IDisposable
{
    private static readonly Uri Endpoint = new("https://eventor.orientering.se/api/results/event/iofxml");
    private readonly HttpClient client = new();

    public EventorFacade(Configuration configuration)
    {
        if (configuration == null) throw new ArgumentNullException(nameof(configuration));

        client.DefaultRequestHeaders.Add("ApiKey", configuration.ApiKey);
    }

    public async Task<Stream> GetIofXmlStream(int eventId)
    {
        UriBuilder uriBuilder = new(Endpoint);
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query["eventId"] = eventId.ToString();
        // query["eventRaceId"] = eventRaceId.ToString();
        query["includeSplitTimes"] = false.ToString();
        query["totalResult"] = false.ToString();

        uriBuilder.Query = query.ToString();
        return await client.GetStreamAsync(uriBuilder.Uri).ConfigureAwait(false);
    }

    public void Dispose()
    {
        client.Dispose();
    }
}