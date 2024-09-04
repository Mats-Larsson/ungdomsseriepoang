using Microsoft.Extensions.Logging;
using System.Collections.Specialized;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;

namespace Results.Liveresultat;

public sealed class LiveresultatFacade : IDisposable
{
    private readonly Uri ENDPOINT = new("http://liveresultat.orientering.se/api.php");
    private readonly int comp;
    private readonly ILogger<LiveresultatFacade> logger;
    private readonly HttpClient client = new();

    private Cached<ClassList> classListCache = new(default);
    private readonly Dictionary<string, Cached<ClassResultList>> classResultListsCache = [];

    public LiveresultatFacade(Configuration configuration, ILogger<LiveresultatFacade> logger)
    {
        if (configuration is null) throw new ArgumentNullException(nameof(configuration));
        if (!configuration.LiveresultatComp.HasValue) throw new InvalidOperationException("LiveresultatComp is missing");

        comp = configuration.LiveresultatComp.Value;
        this.logger = logger;
    }

    public async Task<ClassList?> GetClassesAsync()
    {
        var list =  await GetData<ClassList>(Method.GetClasses, classListCache.Hash).ConfigureAwait(false);
        if (list == null) return classListCache.Data;
        classListCache = new Cached<ClassList>(list);
        return list;
    }

    public async Task<ClassResultList?> GetClassResultAsync(string className)
    {
        var found  = classResultListsCache.TryGetValue(className, out var value);


        NameValueCollection parameters = new() 
        { 
            ["class"] = className, 
            ["unformattedTimes"] = "true" 
        };
        var list =  await GetData<ClassResultList>(Method.GetClassResults, value?.Hash, parameters).ConfigureAwait(false);
        
        classResultListsCache[className] = new Cached<ClassResultList>(list);
        return list;
    }

    private async Task<T?> GetData<T>(string method, string? hash, NameValueCollection? parameters = null) where T : ICommon
    {
        UriBuilder uriBuilder = new(ENDPOINT);
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query["comp"] = comp.ToString();
        query["method"] = method;
        if (hash != null) query["last_hash"] = hash;
        query.Add(parameters ?? []);
        uriBuilder.Query = query.ToString();

        var resp = await client.GetAsync(uriBuilder.Uri).ConfigureAwait(false);
        if (!resp.IsSuccessStatusCode) return default;
       // var str = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);

        var data = await resp.Content.ReadFromJsonAsync<T>(new JsonSerializerOptions() { IncludeFields = true }).ConfigureAwait(false);
        
        if (data != null && data.Status == "OK") return data;
        return default;
    }

    public void Dispose()
    {
        client.Dispose();
    }
}

internal record Cached<T> where T : ICommon {
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
    public const string GetClasses = "getclasses";
    public const string GetClassResults = "getclassresults";
}

public interface ICommon {

    public string? Status { get; }
    public string? Hash { get; }
}

#pragma warning disable CA1716 // Identifiers should not match keywords
public record @Class
#pragma warning restore CA1716 // Identifiers should not match keywords
{
    public string ClassName { get; }

    public @Class(string className)
    {
        ClassName = className;
    }
}

public record ClassList : ICommon {

    public string Status { get; }
    public IList<Class> Classes { get; }
    public string? Hash { get; }

    public ClassList(string status, IList<@Class> classes, string? hash)
    {
        Status = status;
        Classes = classes;
        Hash = hash;
    }
}

public enum Status
{
    OK = 0,
    DNS = 1, //(Did Not Start)
    DNF = 2, // (Did not finish)
    MP = 3, // (Missing Punch)
    DSQ = 4, // (Disqualified)
    OT = 5, // (Over (max) time)
    NotStartedYet1= 9,
    NotStartedYet2 = 10,
    WalkOver = 11, // (Resigned before the race started)
    MovedUp = 12 // (The runner have been moved to a higher class)
}

public record PersonResult
{
    [JsonPropertyName("place")]
    public string? Place { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("club")]
    public string? Club { get; init; }

    [JsonPropertyName("result")]
    public string? Result { private get; init; }
    public TimeSpan? Time => ToTimeSpan(Result);

    [JsonPropertyName("status")]
    public Status Status { get; init; }

    [JsonPropertyName("timeplus")]
    public string? Timeplus { get; init; }

    [JsonPropertyName("progress")]
    public int Progress { get; init; }

    [JsonPropertyName("start")]
    public JsonElement? Start { private get; init; }
    public TimeSpan? StartTime
    {
        get
        {
            if (Start == null) return default;
            if (!Start.HasValue) return default;
            return ToTimeSpan(Start.Value.ToString());
        }
    }


    private static TimeSpan? ToTimeSpan(string? val)
    {
        if (val == null) return default;

        int intVal;
        if (!int.TryParse(val, out intVal)) return default;
        return TimeSpan.FromMilliseconds(10 * intVal);
    }
}

public record ClassResultList : ICommon
{
    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("className")]
    public string? ClassName { get; init; }

    [JsonPropertyName("results")]
    public IList<PersonResult>? Results { get; init; }

    [JsonPropertyName("hash")]
    public string? Hash { get; init; }
}
