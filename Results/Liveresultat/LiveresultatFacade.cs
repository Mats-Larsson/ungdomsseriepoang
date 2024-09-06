using Microsoft.Extensions.Logging;
using Results.Liveresultat.Model;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Web;

namespace Results.Liveresultat;

[SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
[SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
public class LiveresultatFacade : IDisposable
{
    private static readonly Uri ENDPOINT = new("http://liveresultat.orientering.se/api.php");
    private readonly ILogger<LiveresultatFacade> logger;
    private readonly HttpClient client = new();

    private Cached<ClassList> classListCache = new(default);
    private readonly Dictionary<string, Cached<ClassResultList>> classResultListsCache = [];

    private Cached<LastPassingList> lastPassingListCache = new(default);

    public LiveresultatFacade(ILogger<LiveresultatFacade> logger)
    {
        this.logger = logger;
    }

    public async Task<CompetitionInfo?> GetCompetitionInfoAsync(int competitionId)
    {
        var competitionInfo = await GetDataAsync<CompetitionInfo>(competitionId, Method.GetCompetitionInfo, default).ConfigureAwait(false) ?? null;
        return competitionInfo;

    }

    public async Task<LastPassingList?> GetLastPassingListAsync(int competitionId)
    {
        var passings = await GetDataAsync<LastPassingList>(competitionId, Method.GetLastPassings, lastPassingListCache.Hash).ConfigureAwait(false) ?? null;
        if (passings == null) return lastPassingListCache.Data;
        lastPassingListCache = new Cached<LastPassingList>(passings);
        return passings;
    }

    public async Task<ClassList?> GetClassesAsync(int competitionId)
    {
        var list = await GetDataAsync<ClassList>(competitionId, Method.GetClasses, classListCache.Hash).ConfigureAwait(false);
        if (list == null) return classListCache.Data;
        classListCache = new Cached<ClassList>(list);
        return list;
    }

    public async Task<ClassResultList?> GetClassResultAsync(int competitionId, string className)
    {
        var found = classResultListsCache.TryGetValue(className, out var value);
        if (found) return value!.Data;

        NameValueCollection parameters = new() { ["class"] = className, ["unformattedTimes"] = "true" };
        var list = await GetDataAsync<ClassResultList>(competitionId, Method.GetClassResults, value?.Hash, parameters).ConfigureAwait(false);

        classResultListsCache[className] = new Cached<ClassResultList>(list);
        return list;
    }

    protected virtual async Task<T> GetDataAsync<T>(int competitionId, string method, string? hash, NameValueCollection? parameters = null)
        where T : DeserializationBase
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
            return default!;
        }

        string body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);

        T? data = DeserializeJson<T>(body);

        if (data == null) return default!;
        var statusBase = data as StatusBase;
        if (statusBase is { Status: "OK" }) return data;
        return statusBase == null ? data : default!;
    }

    internal virtual T? DeserializeJson<T>(string body)
    {
        var data = JsonSerializer.Deserialize<T>(body);
        return data;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            client.Dispose();
        }
    }
}

internal record Cached<T> where T : StatusBase
{
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