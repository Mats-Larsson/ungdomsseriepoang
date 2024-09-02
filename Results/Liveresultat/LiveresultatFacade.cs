using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace Results.Liveresultat;

public sealed class LiveresultatFacade : IDisposable
{
    private readonly Uri ENDPOINT = new Uri("http://liveresultat.orientering.se/api.php");
    private readonly int comp;
    private readonly ILogger<LiveresultatFacade> logger;
    private readonly HttpClient client = new HttpClient();

    private ClassList? classList;
    private string? classListHash;

    public LiveresultatFacade(Configuration configuration, ILogger<LiveresultatFacade> logger)
    {
        if (configuration is null) throw new ArgumentNullException(nameof(configuration));
        if (!configuration.LiveresultatComp.HasValue) throw new InvalidOperationException("LiveresultatComp is missing");

        comp = configuration.LiveresultatComp.Value;
        this.logger = logger;
    }

    public async Task<ClassList?> GetClassesAsync()
    {
        var classList =  await GetData<ClassList>(Method.GetClasses, classListHash).ConfigureAwait(false);
        if (classList == null) return this.classList;
        this.classList = classList;
        classListHash = classList.Hash;
        return classList;
    }

    private ClassList? GetLastClasses<T>()
    {
        return classList;
    }

    private async Task<T?> GetData<T>(string method, string? hash) where T : ICommon
    {
        Uri uri = new Uri(ENDPOINT, $"?comp={comp}&method={method}&last_hash={hash}");

        var resp = await client.GetAsync(uri).ConfigureAwait(false);
        var data = await resp.Content.ReadFromJsonAsync<T>().ConfigureAwait(false);
        if (data != null && data.Status == "OK") return data;
        return default;
    }

    private static async Task<bool> IsNewData(HttpResponseMessage resp)
    {
        if (!resp.IsSuccessStatusCode) return false;
        var value = await resp.Content.ReadFromJsonAsync<JsonObject>().ConfigureAwait(false);
        return true;
        // return value != null && value.TryGetPropertyValue("status", out var status) && status!.AsValue().ToString() == "NOT MODIFIED";
    }

    public void Dispose()
    {
        client.Dispose();
    }
}

class Method
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

public record Result
{
    public int Place { get; }
    public string Name { get; }
    public string Club { get; }
    public string Result { get; }
    public int Status { get; }
    public string Timeplus { get; }
    public int Progress { get; }
    public int Start { get; }

    public Result(
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

    public string? Hash { get; }
}
