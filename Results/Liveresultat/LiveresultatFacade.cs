using Microsoft.Extensions.Logging;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Web;

namespace Results.Liveresultat;

[SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
public sealed class LiveresultatFacade : IDisposable
{
    private static readonly Uri ENDPOINT = new("http://liveresultat.orientering.se/api.php");
    private readonly ILogger<LiveresultatFacade> logger;
    private readonly HttpClient client = new();

    private Cached<ClassList> classListCache = new(default);
    private readonly Dictionary<string, Cached<ClassResultList>> classResultListsCache = [];

    public LiveresultatFacade(ILogger<LiveresultatFacade> logger)
    {
        this.logger = logger;
    }

    public async Task<CompetitionInfo?> GetCompetitionInfoAsync(int competitionId)
    {
        return await GetDataAsync<CompetitionInfo>(competitionId, Method.GetCompetitionInfo, default).ConfigureAwait(false) ?? null;
    }

    public async Task<LastPassingList?> GetLastPassingListAsync(int competitionId)
    {
        return await GetDataAsync<LastPassingList>(competitionId, Method.GetLastPassings, default).ConfigureAwait(false) ?? null;
    }

    public async Task<ClassList?> GetClassesAsync(int competitionId)
    {
        var list =  await GetDataAsync<ClassList>(competitionId, Method.GetClasses, classListCache.Hash).ConfigureAwait(false);
        if (list == null) return classListCache.Data;
        classListCache = new Cached<ClassList>(list);
        return list;
    }

    public async Task<ClassResultList?> GetClassResultAsync(int competitionId, string className)
    {
        var found  = classResultListsCache.TryGetValue(className, out var value);
        if (found) return value!.Data;

        NameValueCollection parameters = new() 
        { 
            ["class"] = className, 
            ["unformattedTimes"] = "true" 
        };
        var list =  await GetDataAsync<ClassResultList>(competitionId, Method.GetClassResults, value?.Hash, parameters).ConfigureAwait(false);
        
        classResultListsCache[className] = new Cached<ClassResultList>(list);
        return list;
    }

    private async Task<T?> GetDataAsync<T>(int competitionId, string method, string? hash, NameValueCollection? parameters = null)
    {
        UriBuilder uriBuilder = new(ENDPOINT);
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query["comp"] = competitionId.ToString();
        query["method"] = method;
        if (hash != null) query["last_hash"] = hash;
        query.Add(parameters ?? []);
        uriBuilder.Query = query.ToString();

        var resp = await client.GetAsync(uriBuilder.Uri).ConfigureAwait(false);
        if (!resp.IsSuccessStatusCode)
        {
            logger.LogError("Failed to get liveresultat: {StatusCode}; {Uri}", resp.StatusCode, uriBuilder.Uri);
            return default;
        }
       
        var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);

        T? data = DeserializeJson<T>(body);

        if (data == null) return default;
        var statusBase = data as StatusBase;
        if (statusBase is { Status: "OK" }) return data;
        return statusBase == null ? data : default;
    }

#pragma warning disable CA1822
    internal T? DeserializeJson<T>(string body)
#pragma warning restore CA1822
    {
        var data = JsonSerializer.Deserialize<T>(body);
        return data;
    }

    public void Dispose()
    {
        client.Dispose();
    }
}

internal record Cached<T> where T : StatusBase {
    public T? Data { get; }

    public Cached(T? data)
    {
        Data = data;
    }
    
    public string? Hash => Data?.Hash;
}

static class Method
{
    public const string GetCompetitionInfo = "getcompetitioninfo";
    public const string GetLastPassings = "getlastpassings";
    public const string GetClasses = "getclasses";
    public const string GetClassResults = "getclassresults";
}