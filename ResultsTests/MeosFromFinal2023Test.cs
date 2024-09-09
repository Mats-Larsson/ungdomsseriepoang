using System.Globalization;
using Microsoft.Extensions.Logging;
using Moq;
using Results;
using Results.Meos;
// ReSharper disable StringLiteralTypo

namespace ResultsTests;

[TestClass]
public sealed class MeosFromFinal2023Test : IDisposable
{
    private static readonly IDictionary<string, DateTime> ResultDateTimes = new Dictionary<string, DateTime>
    {
        { "Referens0001.xml", Dt("2023-09-16 11:06") },
        { "Referens0002.xml", Dt("2023-09-16 11:16") },
        { "Referens0003.xml", Dt("2023-09-16 11:26") },
        { "Referens0004.xml", Dt("2023-09-16 11:36") },
        { "Referens0005.xml", Dt("2023-09-16 11:46") },
        { "Referens0006.xml", Dt("2023-09-16 11:56") },
        { "Referens0007.xml", Dt("2023-09-16 12:06") },
        { "Referens0008.xml", Dt("2023-09-16 12:16") },
        { "Referens0009.xml", Dt("2023-09-16 12:26") },
        { "Referens0010.xml", Dt("2023-09-16 12:36") },
        { "Referens0011.xml", Dt("2023-09-16 12:46") },
        { "Referens0012.xml", Dt("2023-09-16 12:56") },
        { "Referens0013.xml", Dt("2023-09-16 13:06") },
        { "Referens0014.xml", Dt("2023-09-16 13:16") },
        { "Referens0015.xml", Dt("2023-09-16 13:26") }
    };

    private readonly ILogger<MeosResultSource> meosResultSourceLoggerMock = Mock.Of<ILogger<MeosResultSource>>();
    private readonly ILogger<TeamService> teamServiceLoggerMock = Mock.Of<ILogger<TeamService>>();
    private readonly ILogger<ResultService> resultServiceLoggerMock = Mock.Of<ILogger<ResultService>>();
    private readonly ResultService results; 
    private readonly Configuration configuration = new()
    {
        TimeUntilNotStated = TimeSpan.FromMinutes(10),
        RefreshInterval = TimeSpan.FromSeconds(10)
    };
    private readonly MeosResultSource meosResultSource;

    public MeosFromFinal2023Test()
    {
        meosResultSource = new MeosResultSource(meosResultSourceLoggerMock);
        ClassFilter classFilter = new(configuration);

        TeamService teamService = new(configuration, teamServiceLoggerMock);
        
        results = new ResultService(configuration, meosResultSource, teamService, resultServiceLoggerMock, classFilter);

    }

    private static DateTime Dt(string val)
    {
        return DateTime.ParseExact(val, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
    }
    
    [TestMethod]
    public async Task ReadAllFiles()
    {

        results.OnNewResults += OnNewResults;

        foreach (string file in MeosResultFiles())
        {
            using var stream = File.OpenRead(file);
            await results.NewResultPostAsync(stream, ResultDateTimes[Path.GetFileName(file)]).ConfigureAwait(true);
            Task.Delay(100).Wait();
        }
        return;

        async void OnNewResults(object? o, EventArgs eventArgs)
        {
            var s = results.GetScoreBoard().Statistics;
            await Console.Out.WriteAsync(s.NumNotActivated + s.NumActivated + s.NumStarted + s.NumPreliminary + s.NumPassed + s.NumNotValid + s.NumNotStarted + " ").ConfigureAwait(false);
            await Console.Out.WriteLineAsync(s.ToString()).ConfigureAwait(false);
        }
    }
    
    private static IEnumerable<string> MeosResultFiles()
    {
        var files = Directory.GetFiles(Path.Combine("..", "..", "..", "Onlineresultat")).OrderBy(f => f);
        return files;
    }

    public void Dispose()
    {
        results.Dispose();
        meosResultSource.Dispose();
    }
}