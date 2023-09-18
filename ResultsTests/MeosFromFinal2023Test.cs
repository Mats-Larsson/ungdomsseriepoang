using System.Globalization;
using Microsoft.Extensions.Logging;
using Moq;
using Results;
using Results.Meos;

namespace ResultsTests;

[TestClass]
public class MeosFromFinal2023Test
{
    private readonly ILogger<MeosResultSource> meosResultSourceLoggerMock = Mock.Of<ILogger<MeosResultSource>>();
    private readonly ILogger<TeamService> teamServiceLoggerMock = Mock.Of<ILogger<TeamService>>();
    private readonly ILogger<ResultService> resultServiceLoggerMock = Mock.Of<ILogger<ResultService>>();
    private readonly Configuration configuration = new()
    {
        TimeUntilNotStated = TimeSpan.FromMinutes(10)
    };

    private static readonly IDictionary<string, DateTime> resultDateTimes = new Dictionary<string, DateTime>()
    {
        { "Referens0001.xml", dt("2023-09-16 11:06") },
        { "Referens0002.xml", dt("2023-09-16 11:16") },
        { "Referens0003.xml", dt("2023-09-16 11:26") },
        { "Referens0004.xml", dt("2023-09-16 11:36") },
        { "Referens0005.xml", dt("2023-09-16 11:46") },
        { "Referens0006.xml", dt("2023-09-16 11:56") },
        { "Referens0007.xml", dt("2023-09-16 12:06") },
        { "Referens0008.xml", dt("2023-09-16 12:16") },
        { "Referens0009.xml", dt("2023-09-16 12:26") },
        { "Referens0010.xml", dt("2023-09-16 12:36") },
        { "Referens0011.xml", dt("2023-09-16 12:46") },
        { "Referens0012.xml", dt("2023-09-16 12:56") },
        { "Referens0013.xml", dt("2023-09-16 13:06") },
        { "Referens0014.xml", dt("2023-09-16 13:16") },
        { "Referens0015.xml", dt("2023-09-16 13:26") }
    };

    private static DateTime dt(string val)
    {
        return DateTime.ParseExact(val, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
    }
    
    [TestMethod]
    public async Task ReadAllFiles()
    {
        using var meosResultSource = new MeosResultSource(meosResultSourceLoggerMock);
        var teamService = new TeamService(configuration, teamServiceLoggerMock, meosResultSource);
        using var results = new ResultService(configuration, meosResultSource, teamService, resultServiceLoggerMock);

        results.OnNewResults += async (sender, args) =>
        {
            var s = results.GetScoreBoard().Statistics;
            await Console.Out.WriteAsync(s.NumNotActivated + s.NumActivated + s.NumStarted + s.NumPreliminary + s.NumPassed + s.NumNotValid + s.NumNotStarted + " ").ConfigureAwait(false);
            await Console.Out.WriteLineAsync(s.ToString()).ConfigureAwait(false);
        };

        foreach (string file in MeosResultFiles())
        {
            using (var stream = File.OpenRead(file)) {
                await results.NewResultPostAsync(stream, resultDateTimes[Path.GetFileName(file)]).ConfigureAwait(false);
                Task.Delay(100).Wait();
            }

        }
    }

    
    private static IEnumerable<string> MeosResultFiles()
    {
        var files = Directory.GetFiles(Path.Combine("..", "..", "..", "Onlineresultat")).OrderBy(f => f);
        return files;
    }
}