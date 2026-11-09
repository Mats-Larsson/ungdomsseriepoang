using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ResultCalc;
using ResultCalc.Contract;
using ResultCalc.Model;
using System.Diagnostics.CodeAnalysis;
using static ResultCalc.Contract.ParticipantStatus;

namespace ResultCalcTests;

[TestClass]
[SuppressMessage("ReSharper", "StringLiteralTypo")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class ResultServiceTests
{
    private const string Comp = "Comp";
    private IResultService? resultService;
    private Configuration? configuration;
    private readonly Mock<IResultSource> resultSourceMock = new();
    private readonly Mock<ITeamService> teamServiceMock = new();
    private readonly Mock<ILogger<ResultService>> loggerMock = new();

    private IResultService Setup(bool isFinal, TimeSpan currentTime, IList<ParticipantResult> partisipantResults)
    {
        configuration = new Configuration { TimeUntilNotStated = TS("00:10:00"), IsFinal = isFinal, MaxPatrolStartInterval = TS("00:00:10"), RefreshInterval = TimeSpan.FromSeconds(10) };
        resultSourceMock.Setup(rs => rs.GetCurrentTimeOfDayAsync()).ReturnsAsync(currentTime);
        resultSourceMock.Setup(rs => rs.GetParticipantResultsAsync()).ReturnsAsync(partisipantResults);

        var actual = CreateResultService();
        return actual;
    }

    private IResultService CreateResultService()
    {
        teamServiceMock.Setup(ts => ts.TeamBasePoints).Returns(new Dictionary<string, int>());
        resultService = new ResultService(configuration!, resultSourceMock.Object, teamServiceMock.Object, loggerMock.Object);
        return resultService;
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task AllNotActivated(bool isFinal)
    {
        Result actual = await Setup(isFinal, TS("9:30:00"),
        [
            new ParticipantResult(Comp, "H10", "Adam", "A", TS("10:00:00"), null, NotActivated),
            new ParticipantResult(Comp, "H10", "Bert", "B", TS("10:01:00"), null, NotActivated),
            new ParticipantResult(Comp, "H10", "Curt", "C", TS("10:02:00"), null, NotActivated)
        ]).GetScoreBoardAsync().ConfigureAwait(true);
        actual.Statistics.Should().BeEquivalentTo(
            new Statistics(3));
        actual.TeamResults.Should().BeEquivalentTo(
        [
            new TeamResult(1, "A", 0, false, 0, 0, new Statistics(1)),
            new TeamResult(1, "B", 0, false, 0, 0, new Statistics(1)),
            new TeamResult(1, "C", 0, false, 0, 0, new Statistics(1))
        ]);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task OneActivated(bool isFinal)
    {
        Result actual = await Setup(isFinal, TS("9:30:00"),
        [
            new ParticipantResult(Comp, "H10", "Adam", "A", TS("10:00:00"), null, Activated),
            new ParticipantResult(Comp, "H10", "Bert", "B", TS("10:01:00"), null, NotActivated),
            new ParticipantResult(Comp, "H10", "Curt", "C", TS("10:02:00"), null, NotActivated)
        ]).GetScoreBoardAsync().ConfigureAwait(true);
        actual.Statistics.Should().BeEquivalentTo(
            new Statistics(2, 1));
        actual.TeamResults.Should().BeEquivalentTo(
        [
            new TeamResult(1, "A", 0, false, 0, 0, new Statistics(0, 1)),
            new TeamResult(1, "B", 0, false, 0, 0, new Statistics(1)),
            new TeamResult(1, "C", 0, false, 0, 0, new Statistics(1))
        ]);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task OneActivatedAndOneStarted(bool isFinal)
    {
        Result actual = await Setup(isFinal, TS("10:01:30"),
        [
            new ParticipantResult(Comp, "H10", "Adam", "A", TS("10:00:00"), null, Activated),
            new ParticipantResult(Comp, "H10", "Bert", "B", TS("10:01:00"), null, NotActivated),
            new ParticipantResult(Comp, "H10", "Curt", "C", TS("10:02:00"), null, Activated)
        ]).GetScoreBoardAsync().ConfigureAwait(true);
        actual.Statistics.Should().BeEquivalentTo(
            new Statistics(1, 1, 1));
        actual.TeamResults.Should().BeEquivalentTo(
        [
            new TeamResult(1, "A", 0, false, 0, 0, new Statistics(0, 0, 1)),
            new TeamResult(1, "B", 0, false, 0, 0, new Statistics(1)),
            new TeamResult(1, "C", 0, false, 0, 0, new Statistics(0, 1))
        ]);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task OneStartedAndOneMissedStart(bool isFinal)
    {
        Result actual = await Setup(isFinal, TS("10:10:30"),
        [
            new ParticipantResult(Comp, "H10", "Adam", "A", TS("10:00:00"), null, NotActivated),
            new ParticipantResult(Comp, "H10", "Bert", "B", TS("10:01:00"), null, NotActivated),
            new ParticipantResult(Comp, "H10", "Curt", "C", TS("10:02:00"), null, Activated)
        ]).GetScoreBoardAsync().ConfigureAwait(true);
        actual.Statistics.Should().BeEquivalentTo(
            new Statistics(numNotActivated: 1, numStarted: 1, numNotStarted: 1));
        actual.TeamResults.Should().BeEquivalentTo([
            new TeamResult(1, "A", 0, false, 0, 0, new Statistics(numNotStarted: 1)),
            new TeamResult(1, "B", 0, false, 0, 0, new Statistics(numNotActivated: 1)),
            new TeamResult(1, "C", 0, false, 0, 0, new Statistics(numStarted: 1))
        ]);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task OneStartedAndOnePassed(bool isFinal)
    {
        Result actual = await Setup(isFinal, TS("10:10:30"),
        [
            new ParticipantResult(Comp, "H10", "Adam", "A", TS("10:00:00"), TS("00:13:00"), Passed),
            new ParticipantResult(Comp, "H10", "Bert", "B", TS("10:01:00"), null, NotActivated),
            new ParticipantResult(Comp, "H10", "Curt", "C", TS("10:02:00"), null, Activated)
        ]).GetScoreBoardAsync().ConfigureAwait(true);
        actual.Statistics.Should().BeEquivalentTo(
            new Statistics(numNotActivated: 1, numStarted: 1, numPassed: 1));
        actual.TeamResults.Should().BeEquivalentTo([
            new TeamResult(1, "A", isFinal ? 100 - 8 : 50, false, 0, 0, new Statistics(numPassed: 1)),
            new TeamResult(2, "B", 0, false, isFinal ? 100 - 8 : 50, 0, new Statistics(numNotActivated: 1)),
            new TeamResult(2, "C", 0, false, isFinal ? 100 - 8 : 50, 0, new Statistics(numStarted: 1))
        ]);
    }

    [TestMethod]
    [DataRow(false, 40 + 36 + 40 + 36)]
    [DataRow(true, 4 * (80 - 8))]
    public async Task TwoPatrols(bool isFinal, int expectedPoints)
    {
        ParticipantResult pr0 = new(Comp, "U2", "Adam", "A", TS("10:01:07"), TS("00:13:00"), Passed); // 36
        ParticipantResult pr1 = new(Comp, "U2", "Bert", "A", TS("10:01:14"), TS("00:12:53"), Passed); // 40
        ParticipantResult pr2 = new(Comp, "U2", "Curt", "A", TS("10:01:21"), TS("00:13:00"), Passed); // 36
        ParticipantResult pr3 = new(Comp, "U2", "Dave", "A", TS("10:01:28"), TS("00:12:53"), Passed); // 40

        Result actual = await Setup(isFinal, TS("10:10:30"), [pr0, pr1, pr2, pr3]).GetScoreBoardAsync().ConfigureAwait(true);
        actual.Statistics.Should().BeEquivalentTo(
            new Statistics(numPassed: 4));

        actual.TeamResults.Should().BeEquivalentTo([new TeamResult(1, "A", expectedPoints, false, 0, 0, new Statistics(numPassed: 4))]);

        (await resultService!.GetParticipantPointsListAsync().ConfigureAwait(true)).OrderBy(pr => pr.Name).Should().BeEquivalentTo(new ParticipantPoints[]
        {
            new(new PointsCalcParticipantResult(pr0) { Pos = 3 }, isFinal ? 80 - 8 : 36),
            new(new PointsCalcParticipantResult(pr1) { Pos = 1, IsExtraParticipant = true }, isFinal ? 80 - 8 : 40),
            new(new PointsCalcParticipantResult(pr2) { Pos = 3 }, isFinal ? 80 - 8 : 36),
            new(new PointsCalcParticipantResult(pr3) { Pos = 1, IsExtraParticipant = true }, isFinal ? 80 - 8 : 40)
        });
    }

    static TimeSpan TS(string v)
    {
        return TimeSpan.Parse(v);
    }
}