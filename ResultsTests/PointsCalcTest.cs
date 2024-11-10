using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Moq;
using Results;
using Results.Contract;
using Results.Model;
using Results.Simulator;
using static Results.Contract.ParticipantStatus;

namespace ResultsTests;

[TestClass]
[SuppressMessage("ReSharper", "RedundantArgumentDefaultValue")]
public sealed class PointsCalcTest
{
    private readonly Mock<ITeamService> emptyTeamServiceMock = new();
    private readonly Mock<ITeamService> oneTeamServiceMock = new();
    private readonly Mock<ITeamService> twoTeamServiceMock = new();
    private readonly Mock<ITeamService> threeTeamServiceMock = new();
    private readonly Configuration normalConfiguration = new() { SpeedMultiplier = 1, MaxPatrolStartInterval = TimeSpan.FromSeconds(10) };
    private readonly Configuration finalConfiguration = new() { SpeedMultiplier = 1, MaxPatrolStartInterval = TimeSpan.FromSeconds(10), IsFinal = true };
    private readonly TimeSpan currentTimeOfDay = TimeSpan.FromHours(18);

    public PointsCalcTest()
    {
        emptyTeamServiceMock.Setup(m => m.TeamBasePoints).Returns(new Dictionary<string, int>());
        oneTeamServiceMock.Setup(m => m.TeamBasePoints).Returns(new Dictionary<string, int> { { "Other club", 1 } });
        twoTeamServiceMock.Setup(m => m.TeamBasePoints).Returns(new Dictionary<string, int> { { "Club A", 10 }, { "Club B", 5 } });
        threeTeamServiceMock.Setup(m => m.TeamBasePoints).Returns(new Dictionary<string, int> { { "Club A", 3 }, { "Club B", 2 }, { "Club C", 1 } });
    }

    [TestMethod]
    public void TestWithSimulatorResults()
    {
        var configuration1 = new Configuration
        {
            SpeedMultiplier = 10,
            NumTeams = 100
        };
        using IResultSource resultSource = new SimulatorResultSource(configuration1);

        IPointsCalc pointsCalc = new PointsCalcNormal(oneTeamServiceMock.Object, configuration1);
        using var simulatorResultSource = new SimulatorResultSource(configuration1);
        var participantResults = simulatorResultSource.GetParticipantResults();
        var scoreBoard = pointsCalc.CalcScoreBoard(currentTimeOfDay, participantResults);
        Assert.AreEqual(28, scoreBoard.Count);
    }

    [TestMethod]
    public void TestWithNoResults()
    {
        IPointsCalc pointsCalc = new PointsCalcNormal(emptyTeamServiceMock.Object, normalConfiguration);
        var scoreBoard = pointsCalc.CalcScoreBoard(currentTimeOfDay, []);
        Assert.AreEqual(0, scoreBoard.Count);
    }

    [TestMethod]
    public void TestBeforeCompetition()
    {
        IPointsCalc pointsCalc = new PointsCalcNormal(emptyTeamServiceMock.Object, normalConfiguration);
        var participantResults = new List<ParticipantResult>
        {
            new("H10", "Adam", "Club A", null, null, NotStarted),
            new("H10", "Rory", "Club B", TimeSpan.FromHours(18), null, NotStarted)
        };
        var scoreBoard = pointsCalc.CalcScoreBoard(currentTimeOfDay, participantResults);
        Assert.AreEqual(2, scoreBoard.Count);
        Assert.AreEqual(TeamResult(1, "Club A", 0, false, numNotStarted: 1), scoreBoard[0]);
        Assert.AreEqual(TeamResult(1, "Club B", 0, false, numNotStarted: 1), scoreBoard[1]);
    }

    [TestMethod]
    public void TestBeforeCompetitionWithBasePoints()
    {
        PointsCalcBase pointsCalc = new PointsCalcNormal(twoTeamServiceMock.Object, normalConfiguration);
        var participantResults = new List<ParticipantResult>
        {
            new("H10", "Adam", "Club A", null, null, NotStarted),
            new("H10", "Rory", "Club B", TimeSpan.FromHours(18), null, NotStarted)
        };
        var scoreBoard = pointsCalc.CalcScoreBoard(currentTimeOfDay, participantResults);
        Assert.AreEqual(2, scoreBoard.Count);
        Assert.AreEqual(TeamResult(1, "Club A", 10, false, numNotStarted: 1, basePoints: 10, diffPointsUp: 0), scoreBoard[0]);
        Assert.AreEqual(TeamResult(2, "Club B", 5, false, numNotStarted: 1, basePoints: 5, diffPointsUp: 5), scoreBoard[1]);
    }

    [TestMethod]
    public void TestWithIgnored()
    {
        PointsCalcBase pointsCalc = new PointsCalcNormal(emptyTeamServiceMock.Object, normalConfiguration);
        var participantResults = new List<ParticipantResult>
        {
            new("H10", "Adam", "Club A", null, null, NotStarted),
            new("H10", "Rory", "Club B", TimeSpan.FromHours(18), null, NotStarted),
            new("H10", "Hugo", "Club C", null, null, Ignored)
        };
        var scoreBoard = pointsCalc.CalcScoreBoard(currentTimeOfDay, participantResults);
        Assert.AreEqual(2, scoreBoard.Count);
        Assert.AreEqual(0, scoreBoard[0].Points);
        Assert.AreEqual(0, scoreBoard[1].Points);
    }

    [TestMethod]
    public void TestWithChecked()
    {
        PointsCalcBase pointsCalc = new PointsCalcNormal(emptyTeamServiceMock.Object, normalConfiguration);
        var participantResults = new List<ParticipantResult>
        {
            new("H10", "Adam", "Club A", null, null, NotStarted),
            new("H10", "Rory", "Club B", null, null, NotStarted),
            new("H10", "Hugo", "Club C", null, null, Started)
        };
        var scoreBoard = pointsCalc.CalcScoreBoard(currentTimeOfDay, participantResults);
        Assert.AreEqual(3, scoreBoard.Count);
        Assert.AreEqual(TeamResult(1, "Club A", 0, false, numNotStarted: 1), scoreBoard[0]);
        Assert.AreEqual(TeamResult(1, "Club B", 0, false, numNotStarted: 1), scoreBoard[1]);
        Assert.AreEqual(TeamResult(1, "Club C", 0, false, numStarted: 1), scoreBoard[2]);
    }

    [TestMethod]
    public void TestWithBasePoints()
    {
        PointsCalcBase pointsCalc = new PointsCalcNormal(threeTeamServiceMock.Object, normalConfiguration);
        var participantResults = new List<ParticipantResult>
        {
            new("H10", "Adam", "Club A", null, null, NotStarted),
            new("H10", "Rory", "Club B", null, null, NotStarted),
            new("H10", "Hugo", "Club C", null, null, NotStarted)
        };
        var scoreBoard = pointsCalc.CalcScoreBoard(currentTimeOfDay, participantResults);
        Assert.AreEqual(3, scoreBoard.Count);
        Assert.AreEqual(TeamResult(1, "Club A", 3, false, 0, 3, numNotStarted: 1), scoreBoard[0]);
        Assert.AreEqual(TeamResult(2, "Club B", 2, false, 1, 2, numNotStarted: 1), scoreBoard[1]);
        Assert.AreEqual(TeamResult(3, "Club C", 1, false, 1, 1, numNotStarted: 1), scoreBoard[2]);
    }

    [TestMethod]
    public void TestWithCheckedAndPreliminary()
    {
        PointsCalcBase pointsCalc = new PointsCalcNormal(emptyTeamServiceMock.Object, normalConfiguration);
        var participantResults = new List<ParticipantResult>
        {
            new("H10", "Adam", "Club A", TimeSpan.FromHours(18), null, NotStarted),
            new("H10", "Rory", "Club B", TimeSpan.FromHours(18), null, Started),
            new("H10", "Hugo", "Club C", TimeSpan.FromHours(18), TimeSpan.FromMinutes(10), Preliminary)
        };
        var scoreBoard = pointsCalc.CalcScoreBoard(currentTimeOfDay, participantResults);
        Assert.AreEqual(3, scoreBoard.Count);
        Assert.AreEqual(TeamResult(1, "Club C", 50, true, 0, numPreliminary: 1), scoreBoard[0]);
        Assert.AreEqual(TeamResult(2, "Club A", 0, false, 50, numNotStarted: 1), scoreBoard[1]);
        Assert.AreEqual(TeamResult(2, "Club B", 0, false, 50, numStarted: 1), scoreBoard[2]);
    }

    [TestMethod]
    public void TestWithCheckedAndPassed()
    {
        PointsCalcBase pointsCalc = new PointsCalcNormal(emptyTeamServiceMock.Object, normalConfiguration);
        var participantResults = new List<ParticipantResult>
        {
            new("H10", "Adam", "Club A", TimeSpan.FromHours(18), null, NotStarted),
            new("H10", "Rory", "Club B", TimeSpan.FromHours(18), null, Started),
            new("H10", "Hugo", "Club C", TimeSpan.FromHours(18), TimeSpan.FromMinutes(10), Passed)
        };
        var scoreBoard = pointsCalc.CalcScoreBoard(currentTimeOfDay, participantResults);
        Assert.AreEqual(3, scoreBoard.Count);
        Assert.AreEqual(TeamResult(1, "Club C", 50, false, numPassed: 1), scoreBoard[0]);
        Assert.AreEqual(TeamResult(2, "Club A", 0, false, 50, numNotStarted: 1), scoreBoard[1]);
        Assert.AreEqual(TeamResult(2, "Club B", 0, false, 50, numStarted: 1), scoreBoard[2]);
    }

    [TestMethod]
    public void TestWithCheckedPreliminaryAndPassed()
    {
        PointsCalcBase pointsCalc = new PointsCalcNormal(emptyTeamServiceMock.Object, normalConfiguration);
        var participantResults = new List<ParticipantResult>
        {
            new("H10", "Adam", "Club A", TimeSpan.FromHours(18), null, NotStarted),
            new("H10", "Rory", "Club B", TimeSpan.FromHours(18), null, Started),
            new("H10", "Hugo", "Club C", TimeSpan.FromHours(18), TimeSpan.FromMinutes(10), Preliminary),
            new("H10", "Hugo", "Club B", TimeSpan.FromHours(18), TimeSpan.FromMinutes(12), Passed)
        };
        var scoreBoard = pointsCalc.CalcScoreBoard(currentTimeOfDay, participantResults);
        Assert.AreEqual(3, scoreBoard.Count);
        Assert.AreEqual(TeamResult(1, "Club C", 50, true, 0, numPreliminary: 1), scoreBoard[0]);
        Assert.AreEqual(TeamResult(2, "Club B", 46, false, 4, numStarted: 1, numPassed: 1), scoreBoard[1]);
        Assert.AreEqual(TeamResult(3, "Club A", 0, false, 46, numNotStarted: 1), scoreBoard[2]);
    }

    [TestMethod]
    public void TestWithPatrol()
    {
        var participantResults = new List<ParticipantResult>
        {
            new("H10", "Adam", "Club A", TimeSpan.Parse("18:01:00"), null, NotStarted),
            new("H10", "Rory", "Club B", TimeSpan.Parse("18:01:00"), null, Started),
            new("H10", "Hugo", "Club C", TimeSpan.Parse("18:01:00"), TimeSpan.FromMinutes(10), Passed),
            new("U4", "Mats", "Club B", TimeSpan.Parse("18:01:04"), TimeSpan.FromMinutes(12), Passed),
            new("U4", "Rolf", "Club A", TimeSpan.Parse("18:01:00"), TimeSpan.FromMinutes(13), Passed),
            new("U4", "Bill", "Club B", TimeSpan.Parse("18:00:56"), TimeSpan.FromMinutes(14), Passed),
            new("U4", "Egon", "Club C", TimeSpan.Parse("18:02:00"), TimeSpan.FromMinutes(18), Passed),
            new("U4", "Sune", "Club C", TimeSpan.Parse("18:02:11"), TimeSpan.FromMinutes(18), Passed)
        };

        var normalScoreBoard = new PointsCalcNormal(emptyTeamServiceMock.Object, normalConfiguration).CalcScoreBoard(currentTimeOfDay, participantResults);
        normalScoreBoard.Should().HaveCount(3);
        normalScoreBoard.Should().BeEquivalentTo([
                TeamResult(1, "Club C", 50 + 10 + 50, false, 0, numPassed: 3),
                TeamResult(2, "Club B", 40 + 26, false, 44, numStarted: 1, numPassed: 2),
                TeamResult(3, "Club A", 40, false, 26, numNotStarted: 1, numPassed: 1)
            ]
        );

        var finalScoreBoard = new PointsCalcFinal(emptyTeamServiceMock.Object, finalConfiguration).CalcScoreBoard(currentTimeOfDay, participantResults);
        finalScoreBoard.Should().HaveCount(3);
        finalScoreBoard.Should().BeEquivalentTo([
                TeamResult(1, "Club C", 100 + 20 + 44, false, numPassed: 3),
                TeamResult(2, "Club B", 64 + 20, false, 80, numStarted: 1, numPassed: 2),
                TeamResult(3, "Club A", 72, false, 12, numNotStarted: 1, numPassed: 1)
            ]
        );
    }

    [TestMethod]
    public void TestCalcNormalPoints()
    {
        // Max
        Assert.AreEqual(50, CalcNormalPoints("H10", "K", "00:00:00", "00:10:30", "00:10:30"));
        Assert.AreEqual(50, CalcNormalPoints("D10", "K", "00:00:00", "00:10:30", "00:10:30"));
        Assert.AreEqual(40, CalcNormalPoints("U1", "K", "00:00:00", "00:10:30", "00:10:30"));
        Assert.AreEqual(10, CalcNormalPoints("Insk", "K", "00:00:00", "00:10:30", "00:10:30"));

        // Min
        Assert.AreEqual(15, CalcNormalPoints("H10", "K", "00:00:00", "02:10:30", "00:10:30"));
        Assert.AreEqual(15, CalcNormalPoints("D10", "K", "00:00:00", "02:10:30", "00:10:30"));
        Assert.AreEqual(10, CalcNormalPoints("U1", "K", "00:00:00", "02:10:30", "00:10:30"));
        Assert.AreEqual(10, CalcNormalPoints("Insk", "K", "00:00:00", "02:10:30", "00:10:30"));

        // Reduction
        Assert.AreEqual(48, CalcNormalPoints("H10", "K", "00:00:00", "00:10:31", "00:10:30"));
        Assert.AreEqual(48, CalcNormalPoints("H10", "K", "00:01:00", "00:11:29", "00:10:30"));
        Assert.AreEqual(48, CalcNormalPoints("H10", "K", "00:02:00", "00:11:30", "00:10:30"));
        Assert.AreEqual(46, CalcNormalPoints("H10", "K", "00:00:00", "00:11:31", "00:10:30"));
        Assert.AreEqual(10, CalcNormalPoints("Insk", "K", "00:00:00", "00:10:30", "00:10:30"));
        Assert.AreEqual(10, CalcNormalPoints("Insk", "K", "00:01:00", "00:12:30", "00:10:30"));

        // Patrull
        Assert.AreEqual(40, CalcNormalPoints("U1", "K", "00:00:00", "00:12:30", "00:12:30"));
        Assert.AreEqual(28, CalcNormalPoints("U1", "K", "00:00:00", "00:13:30", "00:12:30", true));
        Assert.AreEqual(10, CalcNormalPoints("U1", "K", "00:00:00", "00:59:30", "00:12:30", true));
    }

    private int CalcNormalPoints(string @class, string club, string startTime, string time, string bestTime, bool isExtraParticipant = false)
    {
        var participantResult = new PointsCalcParticipantResult(@class, "", club, TimeSpan.Parse(startTime), TimeSpan.Parse(time), Passed)
        {
            IsExtraParticipant = isExtraParticipant
        };

        PointsCalcBase pointsCalc = new PointsCalcNormal(emptyTeamServiceMock.Object, normalConfiguration);
        return pointsCalc.CalcPoints(participantResult, TimeSpan.Parse(bestTime));
    }

    [TestMethod]
    public void TestCalcFinalPoints()
    {
        // Max
        Assert.AreEqual(100, CalcFinalPoints("H10", "K", "00:00:00", "00:10:30", "00:10:30"));
        Assert.AreEqual(100, CalcFinalPoints("D10", "K", "00:00:00", "00:10:30", "00:10:30"));
        Assert.AreEqual(80, CalcFinalPoints("U1", "K", "00:00:00", "00:10:30", "00:10:30"));
        Assert.AreEqual(20, CalcFinalPoints("Insk", "K", "00:00:00", "00:10:30", "00:10:30"));

        // Min
        Assert.AreEqual(20, CalcFinalPoints("H10", "K", "00:00:00", "02:10:30", "00:10:30"));
        Assert.AreEqual(20, CalcFinalPoints("D10", "K", "00:00:00", "02:10:30", "00:10:30"));
        Assert.AreEqual(20, CalcFinalPoints("U1", "K", "00:00:00", "02:10:30", "00:10:30"));
        Assert.AreEqual(20, CalcFinalPoints("Insk", "K", "00:00:00", "02:10:30", "00:10:30"));

        // Reduction
        Assert.AreEqual(100, CalcFinalPoints("H10", "K", "00:00:00", "00:12:00", "00:10:30"));
        Assert.AreEqual(99, CalcFinalPoints("H10", "K", "00:01:00", "00:12:01", "00:10:30"));
        Assert.AreEqual(99, CalcFinalPoints("H10", "K", "00:01:00", "00:12:02", "00:10:30"));
        Assert.AreEqual(99, CalcFinalPoints("H10", "K", "00:01:00", "00:12:03", "00:10:30"));
        Assert.AreEqual(99, CalcFinalPoints("H10", "K", "00:01:00", "00:12:04", "00:10:30"));
        Assert.AreEqual(99, CalcFinalPoints("H10", "K", "00:01:00", "00:12:05", "00:10:30"));
        Assert.AreEqual(99, CalcFinalPoints("H10", "K", "00:02:00", "00:12:06", "00:10:30"));
        Assert.AreEqual(99, CalcFinalPoints("H10", "K", "00:00:00", "00:12:07", "00:10:30"));
        Assert.AreEqual(98, CalcFinalPoints("H10", "K", "00:00:00", "00:12:08", "00:10:30"));

        Assert.AreEqual(79, CalcFinalPoints("H10", "K", "00:00:00", "00:14:37", "00:10:30"));
        Assert.AreEqual(78, CalcFinalPoints("H10", "K", "00:00:00", "00:14:38", "00:10:30"));

        Assert.AreEqual(56, CalcFinalPoints("H10", "K", "00:00:00", "00:17:30", "00:10:30"));
        Assert.AreEqual(55, CalcFinalPoints("H10", "K", "00:00:00", "00:17:31", "00:10:30"));

        Assert.AreEqual(55, CalcFinalPoints("H10", "K", "00:00:00", "00:17:37", "00:10:30"));
        Assert.AreEqual(54, CalcFinalPoints("H10", "K", "00:00:00", "00:17:38", "00:10:30"));

        Assert.AreEqual(20, CalcFinalPoints("Insk", "K", "00:00:00", "00:10:30", "00:10:30"));
        Assert.AreEqual(20, CalcFinalPoints("Insk", "K", "00:01:00", "00:12:30", "00:10:30"));

        // Patrull
        Assert.AreEqual(79, CalcFinalPoints("U1", "K", "00:00:00", "00:12:01", "00:12:30"));
        Assert.AreEqual(20, CalcFinalPoints("U1", "K", "00:00:00", "00:13:30", "00:12:30", true));
        Assert.AreEqual(20, CalcFinalPoints("U1", "K", "00:00:00", "00:59:30", "00:12:30", true));
    }

    private static TeamResult TeamResult(int pos, string club, int points, bool isPreliminary, int diffPointsUp = 0, int basePoints = 0,
        int numNotActivated = 0, int numActivated = 0, int numStarted = 0, int numPreliminary = 0, int numPassed = 0, int numNotValid = 0, int numNotStarted = 0)
    {
        Statistics statistics = new(numNotActivated, numActivated, numStarted, numPreliminary, numPassed, numNotValid, numNotStarted);
        return new TeamResult(pos, club, points, isPreliminary, diffPointsUp, basePoints, statistics);
    }

    private int CalcFinalPoints(string @class, string club, string startTime, string time, string bestTime, bool isExtraParticipant = false)
    {
        var participantResult = new PointsCalcParticipantResult(@class, "", club, TimeSpan.Parse(startTime), TimeSpan.Parse(time), Passed)
        {
            IsExtraParticipant = isExtraParticipant
        };
        PointsCalcBase pointsCalc = new PointsCalcFinal(emptyTeamServiceMock.Object, finalConfiguration);
        return pointsCalc.CalcPoints(participantResult, TimeSpan.Parse(bestTime));
    }
}