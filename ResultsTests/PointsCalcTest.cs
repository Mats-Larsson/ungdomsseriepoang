using Results;
using Results.Contract;
using Results.Model;
using Results.Simulator;
using static Results.Model.ParticipantStatus;

namespace ResultsTests;

[TestClass]
public class PointsCalcTest
{
    private readonly Dictionary<string, int> emptyBaseResults = new();
    private readonly Dictionary<string, int> oneBaseResults = new() { { "Other club", 1 } };
    private readonly Configuration normalConfiguration = new(ResultSourceType.Simulator) {SpeedMultiplier = 1};
    private readonly IResultSource normalResultSource;
    private readonly Configuration finalConfiguration = new(ResultSourceType.Simulator) { SpeedMultiplier = 1, IsFinal = true};
    private readonly IResultSource finalResultSource;

    public PointsCalcTest()
    {
        normalResultSource = new SimulatorResultSource(normalConfiguration);
        finalResultSource = new SimulatorResultSource(finalConfiguration);
    }
    [TestMethod]
    public void TestWithSimulatorResults()
    {
        var configuration1 = new Configuration(ResultSourceType.Simulator)
        {
            SpeedMultiplier = 10,
            NumTeams = 100
        };
        using IResultSource resultSource = new SimulatorResultSource(configuration1);

        PointsCalc pointsCalc = new(oneBaseResults, configuration1, resultSource);
        using var simulatorResultSource = new SimulatorResultSource(configuration1);
        var participantResults = simulatorResultSource.GetParticipantResults();
        var scoreBoard = pointsCalc.CalcScoreBoard(participantResults);
        Assert.AreEqual(28, scoreBoard.Count);
    }

    [TestMethod]
    public void TestWithNoResults()
    {
        PointsCalc pointsCalc = new(emptyBaseResults, normalConfiguration, normalResultSource);
        var participantResults = new List<ParticipantResult>();
        var scoreBoard = pointsCalc.CalcScoreBoard(participantResults);
        Assert.AreEqual(0, scoreBoard.Count);
    }

    [TestMethod]
    public void TestBeforeCompetition()
    {
        PointsCalc pointsCalc = new(emptyBaseResults, normalConfiguration, normalResultSource);
        var participantResults = new List<ParticipantResult>
        {
            new("H10", "Adam", "Club A", null, null, NotStarted),
            new("H10", "Rory", "Club B", TimeSpan.FromHours(18), null, NotStarted),
        };
        var scoreBoard = pointsCalc.CalcScoreBoard(participantResults);
        Assert.AreEqual(2, scoreBoard.Count);
        Assert.AreEqual(TeamResult(1, "Club A", 0, false, numNotStarted: 1), scoreBoard[0]);
        Assert.AreEqual(TeamResult(1, "Club B", 0, false, numNotStarted: 1), scoreBoard[1]);
    }

    [TestMethod]
    public void TestWithIgnored()
    {
        PointsCalc pointsCalc = new(emptyBaseResults, normalConfiguration, normalResultSource);
        var participantResults = new List<ParticipantResult>
        {
            new("H10", "Adam", "Club A", null, null, NotStarted),
            new("H10", "Rory", "Club B", TimeSpan.FromHours(18), null, NotStarted),
            new("H10", "Hugo", "Club C", null, null, Ignored),
        };
        var scoreBoard = pointsCalc.CalcScoreBoard(participantResults);
        Assert.AreEqual(2, scoreBoard.Count);
        Assert.AreEqual(0, scoreBoard[0].Points);
        Assert.AreEqual(0, scoreBoard[1].Points);
    }

    [TestMethod]
    public void TestWithChecked()
    {
        PointsCalc pointsCalc = new(emptyBaseResults, normalConfiguration, normalResultSource);
        var participantResults = new List<ParticipantResult>
        {
            new("H10", "Adam", "Club A", null, null, NotStarted),
            new("H10", "Rory", "Club B", null, null, NotStarted),
            new("H10", "Hugo", "Club C", null, null, Started),
        };
        var scoreBoard = pointsCalc.CalcScoreBoard(participantResults);
        Assert.AreEqual(3, scoreBoard.Count);
        Assert.AreEqual(TeamResult(1, "Club A", 0, false, numNotStarted: 1), scoreBoard[0]);
        Assert.AreEqual(TeamResult(1, "Club B", 0, false, numNotStarted: 1), scoreBoard[1]);
        Assert.AreEqual(TeamResult(1, "Club C", 0, false, numStarted: 1), scoreBoard[2]);
    }

    [TestMethod]
    public void TestWithBasePoints()
    {
        PointsCalc pointsCalc = new(new Dictionary<string, int> {{"Club A", 3}, { "Club B", 2 }, { "Club C", 1 }, }, normalConfiguration, normalResultSource);
        var participantResults = new List<ParticipantResult>
        {
            new("H10", "Adam", "Club A", null, null, NotStarted),
            new("H10", "Rory", "Club B", null, null, NotStarted),
            new("H10", "Hugo", "Club C", null, null, NotStarted),
        };
        var scoreBoard = pointsCalc.CalcScoreBoard(participantResults);
        Assert.AreEqual(3, scoreBoard.Count);
        Assert.AreEqual(TeamResult(1, "Club A", 3, false, 0, 3, numNotStarted: 1), scoreBoard[0]);
        Assert.AreEqual(TeamResult(2, "Club B", 2, false, 1, 2, numNotStarted: 1), scoreBoard[1]);
        Assert.AreEqual(TeamResult(3, "Club C", 1, false, 1, 1, numNotStarted: 1), scoreBoard[2]);
    }

    [TestMethod]
    public void TestWithCheckedAndPreliminary()
    {
        PointsCalc pointsCalc = new(emptyBaseResults, normalConfiguration, normalResultSource);
        var participantResults = new List<ParticipantResult>
        {
            new("H10", "Adam", "Club A", TimeSpan.FromHours(18), null, NotStarted),
            new("H10", "Rory", "Club B", TimeSpan.FromHours(18), null, Started),
            new("H10", "Hugo", "Club C", TimeSpan.FromHours(18), TimeSpan.FromMinutes(10), Preliminary),
        };
        var scoreBoard = pointsCalc.CalcScoreBoard(participantResults);
        Assert.AreEqual(3, scoreBoard.Count);
        Assert.AreEqual(TeamResult(1, "Club C", 50, true,  0, numPreliminary: 1), scoreBoard[0]);
        Assert.AreEqual(TeamResult(2, "Club A", 0, false, 50, numNotStarted: 1), scoreBoard[1]);
        Assert.AreEqual(TeamResult(2, "Club B", 0, false, 50, numStarted: 1), scoreBoard[2]);
    }

    [TestMethod]
    public void TestWithCheckedAndPassed()
    {
        PointsCalc pointsCalc = new(emptyBaseResults, normalConfiguration, normalResultSource);
        var participantResults = new List<ParticipantResult>
        {
            new("H10", "Adam", "Club A", TimeSpan.FromHours(18), null, NotStarted),
            new("H10", "Rory", "Club B", TimeSpan.FromHours(18), null, Started),
            new("H10", "Hugo", "Club C", TimeSpan.FromHours(18), TimeSpan.FromMinutes(10), Passed),
        };
        var scoreBoard = pointsCalc.CalcScoreBoard(participantResults);
        Assert.AreEqual(3, scoreBoard.Count);
        Assert.AreEqual(TeamResult(1, "Club C", 50, false, numPassed: 1), scoreBoard[0]);
        Assert.AreEqual(TeamResult(2, "Club A", 0, false, 50, numNotStarted: 1), scoreBoard[1]);
        Assert.AreEqual(TeamResult(2, "Club B", 0, false, 50, numStarted: 1), scoreBoard[2]);
    }

    [TestMethod]
    public void TestWithCheckedPreliminaryAndPassed()
    {
        PointsCalc pointsCalc = new(emptyBaseResults, normalConfiguration, normalResultSource);
        var participantResults = new List<ParticipantResult>
        {
            new("H10", "Adam", "Club A", TimeSpan.FromHours(18), null, NotStarted),
            new("H10", "Rory", "Club B", TimeSpan.FromHours(18), null, Started),
            new("H10", "Hugo", "Club C", TimeSpan.FromHours(18), TimeSpan.FromMinutes(10), Preliminary),
            new("H10", "Hugo", "Club B", TimeSpan.FromHours(18), TimeSpan.FromMinutes(12), Passed),
        };
        var scoreBoard = pointsCalc.CalcScoreBoard(participantResults);
        Assert.AreEqual(3, scoreBoard.Count);
        Assert.AreEqual(TeamResult(1, "Club C", 50, true,  0, numPreliminary: 1), scoreBoard[0]);
        Assert.AreEqual(TeamResult(2, "Club B", 46, false, 4, numStarted: 1, numPassed: 1), scoreBoard[1]);
        Assert.AreEqual(TeamResult(3, "Club A", 0, false, 46, numNotStarted: 1), scoreBoard[2]);
    }

    [TestMethod]
    public void TestWithPatrol()
    {
        var participantResults = new List<ParticipantResult>
        {
            new("H10", "Adam", "Club A", TimeSpan.FromHours(18), null, NotStarted),
            new("H10", "Rory", "Club B", TimeSpan.FromHours(18), null, Started),
            new("H10", "Hugo", "Club C", TimeSpan.FromHours(18), TimeSpan.FromMinutes(10), Passed),
            new("U4", "Mats", "Club B", TimeSpan.FromHours(18), TimeSpan.FromMinutes(12), Passed),
            new("U4", "Rolf", "Club A", TimeSpan.FromHours(18), TimeSpan.FromMinutes(13), Passed),
            new("U4", "Bill", "Club B", TimeSpan.FromHours(18), TimeSpan.FromMinutes(14), Passed),
            new("U4", "Egon", "Club C", TimeSpan.FromHours(18), TimeSpan.FromMinutes(99), Passed),
        };

        var normalScoreBoard = new PointsCalc(emptyBaseResults, normalConfiguration, normalResultSource).CalcScoreBoard(participantResults);
        Assert.AreEqual(3, normalScoreBoard.Count);
        Assert.AreEqual(TeamResult(1, "Club B", 40+26, false, numStarted: 1, numPassed: 2), normalScoreBoard[0]);
        Assert.AreEqual(TeamResult(2, "Club C", 50+10, false, 6, numPassed: 2), normalScoreBoard[1]);
        Assert.AreEqual(TeamResult(3, "Club A", 38, false, 22, numNotStarted: 1, numPassed: 1), normalScoreBoard[2]);

        var finalScoreBoard = new PointsCalc(emptyBaseResults, finalConfiguration, finalResultSource).CalcScoreBoard(participantResults);
        Assert.AreEqual(3, finalScoreBoard.Count);
        Assert.AreEqual(TeamResult(1, "Club C", 100 + 20, false, numPassed: 2), finalScoreBoard[0]);
        Assert.AreEqual(TeamResult(2, "Club B", 80 + 20, false, 20, numStarted: 1, numPassed: 2), finalScoreBoard[1]);
        Assert.AreEqual(TeamResult(3, "Club A", 70, false, 30, numNotStarted: 1, numPassed: 1), finalScoreBoard[2]);
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

    private static int CalcNormalPoints(string @class, string club, string startTime, string time, string bestTime, bool isExtraParticipant = false)
    {
        var participantResult = new PointsCalcParticipantResult( @class, "", club, TimeSpan.Parse(startTime), TimeSpan.Parse(time), Passed)
        {
            IsExtraParticipant = isExtraParticipant
        };

        return PointsCalc.CalcNormalPoints(participantResult, TimeSpan.Parse(bestTime));
    }

    [TestMethod]
    public void TestCalcFinalPoints()
    {
        // Max
        Assert.AreEqual(100, CalcFinalPoints("H10", "K", "00:00:00", "00:10:30", "00:10:30"));
        Assert.AreEqual(100, CalcFinalPoints("D10", "K", "00:00:00", "00:10:30", "00:10:30"));
        Assert.AreEqual( 80, CalcFinalPoints("U1",  "K", "00:00:00", "00:10:30", "00:10:30"));
        Assert.AreEqual( 20, CalcFinalPoints("Insk","K", "00:00:00", "00:10:30", "00:10:30"));

        // Min
        Assert.AreEqual(20, CalcFinalPoints("H10", "K", "00:00:00", "02:10:30", "00:10:30"));
        Assert.AreEqual(20, CalcFinalPoints("D10", "K", "00:00:00", "02:10:30", "00:10:30"));
        Assert.AreEqual(20, CalcFinalPoints("U1",  "K", "00:00:00", "02:10:30", "00:10:30"));
        Assert.AreEqual(20, CalcFinalPoints("Insk","K", "00:00:00", "02:10:30", "00:10:30"));

        // Reduction
        Assert.AreEqual(100, CalcFinalPoints("H10", "K", "00:00:00", "00:12:00", "00:10:30"));
        Assert.AreEqual( 99, CalcFinalPoints("H10", "K", "00:01:00", "00:12:01", "00:10:30"));
        Assert.AreEqual( 99, CalcFinalPoints("H10", "K", "00:02:00", "00:12:06", "00:10:30"));
        Assert.AreEqual( 98, CalcFinalPoints("H10", "K", "00:00:00", "00:12:07", "00:10:30"));
        Assert.AreEqual( 20, CalcFinalPoints("Insk", "K", "00:00:00", "00:10:30", "00:10:30"));
        Assert.AreEqual( 20, CalcFinalPoints("Insk", "K", "00:01:00", "00:12:30", "00:10:30"));

        // Patrull
        Assert.AreEqual(79, CalcFinalPoints("U1", "K", "00:00:00", "00:12:01", "00:12:30"));
        Assert.AreEqual(20, CalcFinalPoints("U1", "K", "00:00:00", "00:13:30", "00:12:30", true));
        Assert.AreEqual(20, CalcFinalPoints("U1", "K", "00:00:00", "00:59:30", "00:12:30", true));
    }

    private static TeamResult TeamResult(int pos, string club, int points, bool isPreliminary, int diffPointsUp = 0, int basePoints = 0,
        int numNotActivated = 0, int numActivated = 0, int numStarted = 0, int numPreliminary = 0, int numPassed = 0, int numNotValid = 0, int numNotStarted = 0)
    {
        Statistics statistics = new Statistics(numNotActivated, numActivated, numStarted, numPreliminary, numPassed, numNotValid, numNotStarted);
        return new TeamResult(pos, club, points, isPreliminary, diffPointsUp, basePoints, statistics);
    }

    private static int CalcFinalPoints(string @class, string club, string startTime, string time, string bestTime, bool isExtraParticipant = false)
    {
        var participantResult = new PointsCalcParticipantResult(@class, "", club, TimeSpan.Parse(startTime), TimeSpan.Parse(time), Passed)
        {
            IsExtraParticipant = isExtraParticipant
        };

        return PointsCalc.CalcFinalPoints(participantResult, TimeSpan.Parse(bestTime));
    }
}
