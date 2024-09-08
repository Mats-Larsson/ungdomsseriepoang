using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Results;
using Results.Contract;
using Results.Model;
using System.Diagnostics.CodeAnalysis;
using static Results.Contract.ParticipantStatus;

namespace ResultsTests;

[TestClass]
[SuppressMessage("ReSharper", "StringLiteralTypo")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class ResultServiceTests
{
    private IResultService? resultService;
    private Configuration? configuration;
    private readonly Mock<IResultSource> resultSourceMock = new();
    private readonly Mock<ITeamService> teamServiceMock = new();
    private readonly Mock<ILogger<ResultService>> loggerMock = new();
    private ClassFilter? classFilter;

    private IResultService Setup(bool isFinal, TimeSpan currentTime, IList<ParticipantResult> partisipantResults)
    {
        configuration = new Configuration { TimeUntilNotStated = TS("00:10:00"), IsFinal = isFinal, MaxPatrolStartInterval = TS("00:00:10"), RefreshInterval = TimeSpan.FromSeconds(10) };
        classFilter = new ClassFilter(configuration);
        resultSourceMock.Setup(rs => rs.CurrentTimeOfDay).Returns(currentTime);
        resultSourceMock.Setup(rs => rs.GetParticipantResults()).Returns(partisipantResults);

        var actual = CreateResultService();
        return actual;
    }

    private IResultService CreateResultService()
    {
        teamServiceMock.Setup(ts => ts.TeamBasePoints).Returns(new Dictionary<string,int>());
        resultService = new ResultService(configuration!, resultSourceMock.Object, teamServiceMock.Object, loggerMock.Object, classFilter!);
        return resultService;
    }

    [DataTestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void AllNotActivated(bool isFinal)
    {
        Result actual = Setup(isFinal, TS("9:30:00"), new[]
        {
            new ParticipantResult("H10", "Adam", "A", TS("10:00:00"), null, NotActivated),
            new ParticipantResult("H10", "Bert", "B", TS("10:01:00"), null, NotActivated),
            new ParticipantResult("H10", "Curt", "C", TS("10:02:00"), null, NotActivated),
        }).GetScoreBoard();
        actual.Statistics.Should().BeEquivalentTo(
            new Statistics(3));
        actual.TeamResults.Should().BeEquivalentTo(new[]
            {
                new TeamResult(1, "A", 0, false, 0, 0, new Statistics(1)),
                new TeamResult(1, "B", 0, false, 0, 0, new Statistics(1)),
                new TeamResult(1, "C", 0, false, 0, 0, new Statistics(1)),
            });
    }

    [DataTestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void OneActivated(bool isFinal)
    {
        Result actual = Setup(isFinal, TS("9:30:00"), new[]
        {
            new ParticipantResult("H10", "Adam", "A", TS("10:00:00"), null, Activated),
            new ParticipantResult("H10", "Bert", "B", TS("10:01:00"), null, NotActivated),
            new ParticipantResult("H10", "Curt", "C", TS("10:02:00"), null, NotActivated),
        }).GetScoreBoard();
        actual.Statistics.Should().BeEquivalentTo(
            new Statistics(2, 1));
        actual.TeamResults.Should().BeEquivalentTo(new[]
            {
                new TeamResult(1, "A", 0, false, 0, 0, new Statistics(0, 1)),
                new TeamResult(1, "B", 0, false, 0, 0, new Statistics(1)),
                new TeamResult(1, "C", 0, false, 0, 0, new Statistics(1)),
            });
    }

    [DataTestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void OneActivatedAndOneStarted(bool isFinal)
    {
        Result actual = Setup(isFinal, TS("10:01:30"), new[]
        {
            new ParticipantResult("H10", "Adam", "A", TS("10:00:00"), null, Activated),
            new ParticipantResult("H10", "Bert", "B", TS("10:01:00"), null, NotActivated),
            new ParticipantResult("H10", "Curt", "C", TS("10:02:00"), null, Activated),
        }).GetScoreBoard();
        actual.Statistics.Should().BeEquivalentTo(
            new Statistics(1, 1, 1));
        actual.TeamResults.Should().BeEquivalentTo(new[]
            {
                new TeamResult(1, "A", 0, false, 0, 0, new Statistics(0, 0, 1)),
                new TeamResult(1, "B", 0, false, 0, 0, new Statistics(1)),
                new TeamResult(1, "C", 0, false, 0, 0, new Statistics(0, 1)),
            });
    }

    [DataTestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void OneStartedAndOneMissedStart(bool isFinal)
    {
        Result actual = Setup(isFinal, TS("10:10:30"), new[]
        {
            new ParticipantResult("H10", "Adam", "A", TS("10:00:00"), null, NotActivated),
            new ParticipantResult("H10", "Bert", "B", TS("10:01:00"), null, NotActivated),
            new ParticipantResult("H10", "Curt", "C", TS("10:02:00"), null, Activated),
        }).GetScoreBoard();
        actual.Statistics.Should().BeEquivalentTo(
            new Statistics(numNotActivated: 1, numStarted: 1, numNotStarted: 1));
        actual.TeamResults.Should().BeEquivalentTo(new[]
            {
                new TeamResult(1, "A", 0, false, 0, 0, new Statistics(numNotStarted: 1)),
                new TeamResult(1, "B", 0, false, 0, 0, new Statistics(numNotActivated: 1)),
                new TeamResult(1, "C", 0, false, 0, 0, new Statistics(numStarted: 1)),
            });
    }

    [DataTestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void OneStartedAndOnePassed(bool isFinal)
    {
        Result actual = Setup(isFinal, TS("10:10:30"), new[]
        {
            new ParticipantResult("H10", "Adam", "A", TS("10:00:00"), TS("00:13:00"), Passed),
            new ParticipantResult("H10", "Bert", "B", TS("10:01:00"), null, NotActivated),
            new ParticipantResult("H10", "Curt", "C", TS("10:02:00"), null, Activated),
        }).GetScoreBoard();
        actual.Statistics.Should().BeEquivalentTo(
            new Statistics(numNotActivated: 1, numStarted: 1, numPassed: 1));
        actual.TeamResults.Should().BeEquivalentTo(new[]
            {
                new TeamResult(1, "A", isFinal ? 100-8 : 50, false, 0, 0, new Statistics(numPassed: 1)),
                new TeamResult(2, "B", 0, false, isFinal ? 100-8 : 50, 0, new Statistics(numNotActivated: 1)),
                new TeamResult(2, "C", 0, false, isFinal ? 100-8 : 50, 0, new Statistics(numStarted: 1)),
            });
    }

    [DataTestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void TwoPatrols(bool isFinal)
    {
        ParticipantResult pr0 = new("U2", "Adam", "A", TS("10:01:07"), TS("00:13:00"), Passed);
        ParticipantResult pr1 = new("U2", "Bert", "A", TS("10:01:14"), TS("00:12:53"), Passed);
        ParticipantResult pr2 = new("U2", "Curt", "A", TS("10:01:21"), TS("00:13:00"), Passed);
        ParticipantResult pr3 = new("U2", "Dave", "A", TS("10:01:28"), TS("00:12:53"), Passed);
        Result actual = Setup(isFinal, TS("10:10:30"), new[] { pr0, pr1, pr2, pr3 }).GetScoreBoard();
        actual.Statistics.Should().BeEquivalentTo(
            new Statistics(numPassed: 4));
        actual.TeamResults.Should().BeEquivalentTo(new[]
            {
                new TeamResult(1, "A", isFinal ? 80-8+80-8+20+20 : 40+30+40+30, false, 0, 0, new Statistics(numPassed: 4))
            });

        
        resultService!.GetParticipantPointsList().OrderBy(pr => pr.Name).Should().BeEquivalentTo(new ParticipantPoints[]
        {
            new(new PointsCalcParticipantResult(pr0), isFinal ? 80-8 : 40),
            new(new PointsCalcParticipantResult(pr1) { IsExtraParticipant = true, Time = TS("00:13:00") }, isFinal ? 20 : 30) ,
            new(new PointsCalcParticipantResult(pr2), isFinal ? 80-8 : 40),
            new(new PointsCalcParticipantResult(pr3) { IsExtraParticipant = true, Time = TS("00:13:00") }, isFinal ? 20 : 30),
        });
    }

    static TimeSpan TS(string v)
    {
        return TimeSpan.Parse(v);
    }
}
