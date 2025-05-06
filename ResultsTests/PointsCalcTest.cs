using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Moq;
using Results;
using Results.Contract;
using Results.Model;
using Results.Simulator;
using static System.Math;
using static Results.Contract.ParticipantStatus;

namespace ResultsTests;

[TestClass]
[SuppressMessage("ReSharper", "RedundantArgumentDefaultValue")]
[SuppressMessage("ReSharper", "UselessBinaryOperation")]
[SuppressMessage("ReSharper", "UseObjectOrCollectionInitializer")]
public sealed class PointsCalcTest
{
    private const string Comp = "Comp";
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
            new(Comp, "H10", "Adam", "Club A", null, null, NotStarted),
            new(Comp, "H10", "Rory", "Club B", TimeSpan.FromHours(18), null, NotStarted)
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
            new(Comp, "H10", "Adam", "Club A", null, null, NotStarted),
            new(Comp, "H10", "Rory", "Club B", TimeSpan.FromHours(18), null, NotStarted)
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
            new(Comp, "H10", "Adam", "Club A", null, null, NotStarted),
            new(Comp, "H10", "Rory", "Club B", TimeSpan.FromHours(18), null, NotStarted),
            new(Comp, "H10", "Hugo", "Club C", null, null, Ignored)
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
            new(Comp, "H10", "Adam", "Club A", null, null, NotStarted),
            new(Comp, "H10", "Rory", "Club B", null, null, NotStarted),
            new(Comp, "H10", "Hugo", "Club C", null, null, Started)
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
            new(Comp, "H10", "Adam", "Club A", null, null, NotStarted),
            new(Comp, "H10", "Rory", "Club B", null, null, NotStarted),
            new(Comp, "H10", "Hugo", "Club C", null, null, NotStarted)
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
            new(Comp, "H10", "Adam", "Club A", TimeSpan.FromHours(18), null, NotStarted),
            new(Comp, "H10", "Rory", "Club B", TimeSpan.FromHours(18), null, Started),
            new(Comp, "H10", "Hugo", "Club C", TimeSpan.FromHours(18), TimeSpan.FromMinutes(10), Preliminary)
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
            new(Comp, "H10", "Adam", "Club A", TimeSpan.FromHours(18), null, NotStarted),
            new(Comp, "H10", "Rory", "Club B", TimeSpan.FromHours(18), null, Started),
            new(Comp, "H10", "Hugo", "Club C", TimeSpan.FromHours(18), TimeSpan.FromMinutes(10), Passed)
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

        int a = 0, b = 0, c = 0;
        
        var participantResults = new List<ParticipantResult>();
        participantResults.Add(new ParticipantResult(Comp, "H10", "Adam", "Club A", TimeSpan.FromHours(18), null, NotStarted)); a += 0;
        participantResults.Add(new ParticipantResult(Comp, "H10", "Rory", "Club B", TimeSpan.FromHours(18), null, Started)); b += 0;
        participantResults.Add(new ParticipantResult(Comp, "H10", "Hugo", "Club C", TimeSpan.FromHours(18), TimeSpan.FromMinutes(10), Preliminary)); c += 50;
        participantResults.Add(new ParticipantResult(Comp, "H10", "Hugo", "Club B", TimeSpan.FromHours(18), TimeSpan.FromMinutes(12), Passed)); b += 48;
        var scoreBoard = pointsCalc.CalcScoreBoard(currentTimeOfDay, participantResults);
        Assert.AreEqual(3, scoreBoard.Count);
        Assert.AreEqual(TeamResult(1, "Club C", c, true, 0, numPreliminary: 1), scoreBoard[0]);
        Assert.AreEqual(TeamResult(2, "Club B", b, false, c - b, numStarted: 1, numPassed: 1), scoreBoard[1]);
        Assert.AreEqual(TeamResult(3, "Club A", a, false, b - a, numNotStarted: 1), scoreBoard[2]);
    }

    [TestMethod]
    public void TestWithPatrol()
    {
        var participantResults = new List<ParticipantResult>();
        int an = 0, bn = 0, cn = 0, af = 0, bf = 0, cf = 0;
        participantResults.Add(new ParticipantResult(Comp, "H10", "Hugo", "Club C", T("18:01:00"), TimeSpan.FromMinutes(10), Passed)); 
        cn += 50; cf += 100;
        participantResults.Add(new ParticipantResult(Comp, "H10", "Adam", "Club A", T("18:01:00"), null, NotStarted));
        an += 0; af += 0;
        participantResults.Add(new ParticipantResult(Comp, "H10", "Rory", "Club B", T("18:01:00"), null, Started));
        bn += 0; bf += 0;
        
        participantResults.Add(new ParticipantResult(Comp, "U4", "Mats", "Club B", T("18:01:04"), T("00:12:00"), Passed));
        bn += 40 - 0; bf += 80 - (int)Ceiling((0/7.5));
        participantResults.Add(new ParticipantResult(Comp, "U4", "Rolf", "Club A", T("18:01:00"), T("00:13:00"), Passed));
        an += 40 - 2; af += 80 - (int)Ceiling((60/7.5));
        participantResults.Add(new ParticipantResult(Comp, "U4", "Bill", "Club B", T("18:00:56"), T("00:14:00"), Passed));
        bn += 40 - 4; bf += 80 - (int)Ceiling((120/7.5));
        participantResults.Add(new ParticipantResult(Comp, "U4", "Egon", "Club C", T("18:02:00"), T("00:17:59"), Passed));
        cn += 40 - 6; cf += 80 - (int)Ceiling((359/7.5));
        participantResults.Add(new ParticipantResult(Comp, "U4", "Sune", "Club C", T("18:02:09"), T("00:18:00"), Passed));
        cn += 40 - 8; cf += 80 - (int)Ceiling((360/7.5));

        var normalScoreBoard = new PointsCalcNormal(emptyTeamServiceMock.Object, normalConfiguration).CalcScoreBoard(currentTimeOfDay, participantResults);
        normalScoreBoard.Should().HaveCount(3);
        normalScoreBoard[0].Should().BeEquivalentTo(TeamResult(1, "Club C", cn, false, 0, numPassed: 3));
        normalScoreBoard[1].Should().BeEquivalentTo(TeamResult(2, "Club B", bn, false, cn - bn, numStarted: 1, numPassed: 2));
        normalScoreBoard[2].Should().BeEquivalentTo(TeamResult(3, "Club A", an, false, bn - an, numNotStarted: 1, numPassed: 1));

        var finalScoreBoard = new PointsCalcFinal(emptyTeamServiceMock.Object, finalConfiguration).CalcScoreBoard(currentTimeOfDay, participantResults);

        finalScoreBoard[0].Should().BeEquivalentTo(TeamResult(1, "Club C", cf, false, 0, numPassed: 3));
        finalScoreBoard[1].Should().BeEquivalentTo(TeamResult(2, "Club B", bf, false, cf - bf, numStarted: 1, numPassed: 2));
        finalScoreBoard[2].Should().BeEquivalentTo(TeamResult(3, "Club A", af, false, bf - af, numPassed: 1, numNotStarted: 1));
    }

    private static TimeSpan T(string text)
    {
        return TimeSpan.Parse(text);
    }

    [TestMethod]
    public void TestCalcNormalPoints()
    {
        // Max
        Assert.AreEqual(50, CalcNormalPoints("H10", "K", "00:00:00", "00:10:30", 1, "00:10:30"));
        Assert.AreEqual(50, CalcNormalPoints("D10", "K", "00:00:00", "00:10:30", 1, "00:10:30"));
        Assert.AreEqual(40, CalcNormalPoints("U1", "K", "00:00:00", "00:10:30", 1, "00:10:30"));
        Assert.AreEqual(10, CalcNormalPoints("Insk", "K", "00:00:00", "00:10:30", 1, "00:10:30"));

        // Min
        Assert.AreEqual(15, CalcNormalPoints("H10", "K", "00:00:00", "02:10:30", 20, "00:10:30"));
        Assert.AreEqual(15, CalcNormalPoints("D10", "K", "00:00:00", "02:10:30", 20, "00:10:30"));
        Assert.AreEqual(10, CalcNormalPoints("U1", "K", "00:00:00", "02:10:30", 20, "00:10:30"));
        Assert.AreEqual(10, CalcNormalPoints("Insk", "K", "00:00:00", "02:10:30", 20, "00:10:30"));

        // Reduction
        Assert.AreEqual(48, CalcNormalPoints("H10", "K", "00:00:00", "00:10:31", 2, "00:10:30"));
        Assert.AreEqual(48, CalcNormalPoints("H10", "K", "00:01:00", "00:11:29", 2, "00:10:30"));
        Assert.AreEqual(48, CalcNormalPoints("H10", "K", "00:02:00", "00:11:30", 2, "00:10:30"));
        Assert.AreEqual(46, CalcNormalPoints("H10", "K", "00:00:00", "00:11:31", 3, "00:10:30"));
        Assert.AreEqual(10, CalcNormalPoints("Insk", "K", "00:00:00", "00:10:30", 1, "00:10:30"));
        Assert.AreEqual(10, CalcNormalPoints("Insk", "K", "00:01:00", "00:12:30", 5, "00:10:30"));

        // Patrull
        Assert.AreEqual(40, CalcNormalPoints("U1", "K", "00:00:00", "00:12:30", 1, "00:12:30"));
        Assert.AreEqual(38, CalcNormalPoints("U1", "K", "00:00:00", "00:13:30", 2, "00:12:30", true));
        Assert.AreEqual(10, CalcNormalPoints("U1", "K", "00:00:00", "00:59:30", 20, "00:12:30", true));
    }

    private int CalcNormalPoints(string @class, string club, string startTime, string time, int pos, string bestTime,
        bool isExtraParticipant = false)
    {
        var participantResult = new PointsCalcParticipantResult(Comp, @class, "", club, T(startTime), T(time), Passed)
        {
            IsExtraParticipant = isExtraParticipant,
            Pos = pos
        };

        PointsCalcBase pointsCalc = new PointsCalcNormal(emptyTeamServiceMock.Object, normalConfiguration);
        return pointsCalc.CalcPoints(participantResult, T(bestTime));
    }

    [TestMethod]
    public void TestCalcFinalPoints()
    {
        // Max
        Assert.AreEqual(100, CalcFinalPoints("H10", "K", "00:00:00", "00:10:30", 1, "00:10:30"));
        Assert.AreEqual(100, CalcFinalPoints("D10", "K", "00:00:00", "00:10:30", 1, "00:10:30"));
        Assert.AreEqual(80, CalcFinalPoints("U1", "K", "00:00:00", "00:10:30", 1, "00:10:30"));
        Assert.AreEqual(20, CalcFinalPoints("Insk", "K", "00:00:00", "00:10:30", 1, "00:10:30"));

        // Min
        Assert.AreEqual(20, CalcFinalPoints("H10", "K", "00:00:00", "02:10:30", 20, "00:10:30"));
        Assert.AreEqual(20, CalcFinalPoints("D10", "K", "00:00:00", "02:10:30", 20, "00:10:30"));
        Assert.AreEqual(20, CalcFinalPoints("U1", "K", "00:00:00", "02:10:30", 20, "00:10:30"));
        Assert.AreEqual(20, CalcFinalPoints("Insk", "K", "00:00:00", "02:10:30", 20, "00:10:30"));

        // Reduction
        Assert.AreEqual(100, CalcFinalPoints("H10", "K", "00:00:00", "00:12:00", 2, "00:10:30"));
        Assert.AreEqual(99, CalcFinalPoints("H10", "K", "00:01:00", "00:12:01", 3, "00:10:30"));
        Assert.AreEqual(99, CalcFinalPoints("H10", "K", "00:01:00", "00:12:02", 3, "00:10:30"));
        Assert.AreEqual(99, CalcFinalPoints("H10", "K", "00:01:00", "00:12:03", 3, "00:10:30"));
        Assert.AreEqual(99, CalcFinalPoints("H10", "K", "00:01:00", "00:12:04", 3, "00:10:30"));
        Assert.AreEqual(99, CalcFinalPoints("H10", "K", "00:01:00", "00:12:05", 3, "00:10:30"));
        Assert.AreEqual(99, CalcFinalPoints("H10", "K", "00:02:00", "00:12:06", 3, "00:10:30"));
        Assert.AreEqual(99, CalcFinalPoints("H10", "K", "00:00:00", "00:12:07", 3, "00:10:30"));
        Assert.AreEqual(98, CalcFinalPoints("H10", "K", "00:00:00", "00:12:08", 3, "00:10:30"));

        Assert.AreEqual(79, CalcFinalPoints("H10", "K", "00:00:00", "00:14:37", 4, "00:10:30"));
        Assert.AreEqual(78, CalcFinalPoints("H10", "K", "00:00:00", "00:14:38", 4, "00:10:30"));

        Assert.AreEqual(56, CalcFinalPoints("H10", "K", "00:00:00", "00:17:30", 5, "00:10:30"));
        Assert.AreEqual(55, CalcFinalPoints("H10", "K", "00:00:00", "00:17:31", 5, "00:10:30"));

        Assert.AreEqual(55, CalcFinalPoints("H10", "K", "00:00:00", "00:17:37", 6, "00:10:30"));
        Assert.AreEqual(54, CalcFinalPoints("H10", "K", "00:00:00", "00:17:38", 6, "00:10:30"));

        Assert.AreEqual(20, CalcFinalPoints("Insk", "K", "00:00:00", "00:10:30", 7, "00:10:30"));
        Assert.AreEqual(20, CalcFinalPoints("Insk", "K", "00:01:00", "00:12:30", 7, "00:10:30"));

        // Patrull
        Assert.AreEqual(79, CalcFinalPoints("U1", "K", "00:00:00", "00:12:01", 2, "00:12:30"));
        Assert.AreEqual(68, CalcFinalPoints("U1", "K", "00:00:00", "00:13:30", 8, "00:12:30", true)); // 80 - (90/7.5)
        Assert.AreEqual(20, CalcFinalPoints("U1", "K", "00:00:00", "00:59:30", 8, "00:12:30", true));
    }

    private static TeamResult TeamResult(int pos, string club, int points, bool isPreliminary, int diffPointsUp = 0, int basePoints = 0,
        int numNotActivated = 0, int numActivated = 0, int numStarted = 0, int numPreliminary = 0, int numPassed = 0, int numNotValid = 0, int numNotStarted = 0)
    {
        Statistics statistics = new(numNotActivated, numActivated, numStarted, numPreliminary, numPassed, numNotValid, numNotStarted);
        return new TeamResult(pos, club, points, isPreliminary, diffPointsUp, basePoints, statistics);
    }

    private int CalcFinalPoints(string @class, string club, string startTime, string time, int pos, string bestTime, bool isExtraParticipant = false)
    {
        var participantResult = new PointsCalcParticipantResult(Comp, @class, "", club, T(startTime), T(time), Passed)
        {
            IsExtraParticipant = isExtraParticipant,
            Pos = pos
        };
        PointsCalcBase pointsCalc = new PointsCalcFinal(emptyTeamServiceMock.Object, finalConfiguration);
        return pointsCalc.CalcPoints(participantResult, T(bestTime));
    }
}