﻿using Microsoft.Extensions.Logging;
using Results.Liveresultat.Model;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Web;

namespace Results.Liveresultat;

[SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
[SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
public class LiveresultatFacade(ILogger<LiveresultatFacade> logger) : IDisposable
{
    private static readonly Uri Endpoint = new("http://liveresultat.orientering.se/api.php");
    private readonly HttpClient client = new();

    private Cached<ClassList> classListCache = new(null);
    private readonly Dictionary<string, Cached<ClassResultList>> classResultListsCache = [];

    private Cached<LastPassingList> lastPassingListCache = new(null);

    public async Task<CompetitionInfo?> GetCompetitionInfoAsync(int competitionId)
    {
        var competitionInfo = await GetDataAsync<CompetitionInfo>(competitionId, Method.GetCompetitionInfo, null).ConfigureAwait(false) ?? null;
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

        NameValueCollection parameters = new() { ["class"] = className, ["unformattedTimes"] = "true" };
        var list = await GetDataAsync<ClassResultList>(competitionId, Method.GetClassResults, value?.Hash, parameters).ConfigureAwait(false);
        if (found && list is null) return value!.Data;

        classResultListsCache[className] = new Cached<ClassResultList>(list);
        return list;
    }

    protected virtual async Task<T?> GetDataAsync<T>(int competitionId, string method, string? hash,
        NameValueCollection? parameters = null)
        where T : DeserializationBase
    {
        UriBuilder uriBuilder = new(Endpoint);
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
            return null;
        }

        string body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);

        T? data = DeserializeJson<T>(body);

        if (data == null) return null!;
        var statusBase = data as StatusBase;
        if (statusBase is { Status: "OK" }) return data;
        return statusBase == null ? data : null!;
    }

    [SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global")]
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

    [SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global")]
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            client.Dispose();
        }
    }
}

internal record Cached<T>(T? Data)
    where T : StatusBase
{
    public string? Hash => Data?.Hash;
}

static class Method
{
    public const string GetCompetitionInfo = "getcompetitioninfo";
    public const string GetLastPassings = "getlastpassings";
    public const string GetClasses = "getclasses";
    public const string GetClassResults = "getclassresults";
}