using Microsoft.Extensions.Logging;
using System.Collections.Specialized;
using System.Net.Http.Json;
using System.Web;

namespace Results.Liveresultat;

public sealed class LiveresultatFacade : IDisposable
{
    private readonly Uri ENDPOINT = new("http://liveresultat.orientering.se/api.php");
    private readonly int comp;
    private readonly ILogger<LiveresultatFacade> logger;
    private readonly HttpClient client = new HttpClient();

    private Cached<ClassList> classListCache = new(default);
    private readonly Dictionary<string,Cached<ClassResultList>> classResultListsCache = new ();

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
        var tuple = this.classResultListsCache[className];
        
        var list =  await GetData<ClassResultList>(Method.GetClasses, tuple.Hash, 
            new NameValueCollection { ["className"] = className }).ConfigureAwait(false);
        
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
        query.Add(parameters ?? new());
        uriBuilder.Query = query.ToString();

        var resp = await client.GetAsync(uriBuilder.Uri).ConfigureAwait(false);
        if (!resp.IsSuccessStatusCode) return default;
        var data = await resp.Content.ReadFromJsonAsync<T>().ConfigureAwait(false);
        
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

    public string Status { get; }
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

public record PersonResult
{
    public int Place { get; }
    public string Name { get; }
    public string Club { get; }
    public string Result { get; }
    public int Status { get; }
    public string Timeplus { get; }
    public int Progress { get; }
    public int Start { get; }

    public PersonResult(
        int place,
        string name,
        string club,
        string result,
        int status,
        string timeplus,
        int progress,
        int start)
    {
        Place = place;
        Name = name;
        Club = club;
        Result = result;
        Status = status;
        Timeplus = timeplus;
        Progress = progress;
        Start = start;
    }
}

public record ClassResultList : ICommon
{
    public string Status { get; }
    public string ClassName { get; }
    public IList<PersonResult> Results { get; }
    public string? Hash { get; }

    public ClassResultList(string status, string className, IList<PersonResult> results, string? hash)
    {
        Status = status;
        ClassName = className;
        Results = results;
        Hash = hash;
    }
}
