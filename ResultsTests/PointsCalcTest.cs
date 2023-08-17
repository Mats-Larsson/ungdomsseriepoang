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

    [TestMethod]
    public void TestWithSimulatorResults()
    {
        PointsCalc pointsCalc = new(oneBaseResults, false);
        var configuration = new Configuration(ResultSource.Simulator)
        {
            SpeedMultiplier = 10,
            NumTeams = 100
        };
        using var simulatorResultSource = new SimulatorResultSource(configuration);
        var participantResults = simulatorResultSource.GetParticipantResults();
        var scoreBoard = pointsCalc.CalcScoreBoard(participantResults);
        Assert.AreEqual(28, scoreBoard.Count);
    }

    [TestMethod]
    public void TestWithNoResults()
    {
        PointsCalc pointsCalc = new(emptyBaseResults, false);
        var participantResults = new List<ParticipantResult>();
        var scoreBoard = pointsCalc.CalcScoreBoard(participantResults);
        Assert.AreEqual(0, scoreBoard.Count);
    }

    [TestMethod]
    public void TestBeforeCompetition()
    {
        PointsCalc pointsCalc = new(emptyBaseResults, false);
        var participantResults = new List<ParticipantResult>
        {
            new("H10", "Adam", "Club A", null, null, NotStarted),
            new("H10", "Rory", "Club B", TimeSpan.FromHours(18), null, NotStarted),
        };
        var scoreBoard = pointsCalc.CalcScoreBoard(participantResults);
        Assert.AreEqual(2, scoreBoard.Count);
        Assert.AreEqual(new TeamResult(1, "Club A", 0, false), scoreBoard[0]);
        Assert.AreEqual(new TeamResult(1, "Club B", 0, false), scoreBoard[1]);
    }

    [TestMethod]
    public void TestWithIgnored()
    {
        PointsCalc pointsCalc = new(emptyBaseResults, false);
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
        PointsCalc pointsCalc = new(emptyBaseResults, false);
        var participantResults = new List<ParticipantResult>
        {
            new("H10", "Adam", "Club A", null, null, NotStarted),
            new("H10", "Rory", "Club B", null, null, NotStarted),
            new("H10", "Hugo", "Club C", null, null, Started),
        };
        var scoreBoard = pointsCalc.CalcScoreBoard(participantResults);
        Assert.AreEqual(3, scoreBoard.Count);
        Assert.AreEqual(new TeamResult(1, "Club A", 0, false), scoreBoard[0]);
        Assert.AreEqual(new TeamResult(1, "Club B", 0, false), scoreBoard[1]);
        Assert.AreEqual(new TeamResult(1, "Club C", 0, false), scoreBoard[2]);
    }

    [TestMethod]
    public void TestWithBasePoints()
    {
        PointsCalc pointsCalc = new(new Dictionary<string, int> {{"Club A", 3}, { "Club B", 2 }, { "Club C", 1 }, }, false);
        var participantResults = new List<ParticipantResult>
        {
            new("H10", "Adam", "Club A", null, null, NotStarted),
            new("H10", "Rory", "Club B", null, null, NotStarted),
            new("H10", "Hugo", "Club C", null, null, NotStarted),
        };
        var scoreBoard = pointsCalc.CalcScoreBoard(participantResults);
        Assert.AreEqual(3, scoreBoard.Count);
        Assert.AreEqual(new TeamResult(1, "Club A", 3, false, 3), scoreBoard[0]);
        Assert.AreEqual(new TeamResult(2, "Club B", 2, false, 2), scoreBoard[1]);
        Assert.AreEqual(new TeamResult(3, "Club C", 1, false, 1), scoreBoard[2]);
    }

    [TestMethod]
    public void TestWithCheckedAndPreliminary()
    {
        PointsCalc pointsCalc = new(emptyBaseResults, false);
        var participantResults = new List<ParticipantResult>
        {
            new("H10", "Adam", "Club A", TimeSpan.FromHours(18), null, NotStarted),
            new("H10", "Rory", "Club B", TimeSpan.FromHours(18), null, Started),
            new("H10", "Hugo", "Club C", TimeSpan.FromHours(18), TimeSpan.FromMinutes(10), Preliminary),
        };
        var scoreBoard = pointsCalc.CalcScoreBoard(participantResults);
        Assert.AreEqual(3, scoreBoard.Count);
        Assert.AreEqual(new TeamResult(1, "Club C", 50, true), scoreBoard[0]);
        Assert.AreEqual(new TeamResult(2, "Club A", 0, false), scoreBoard[1]);
        Assert.AreEqual(new TeamResult(2, "Club B", 0, false), scoreBoard[2]);
    }

    [TestMethod]
    public void TestWithCheckedAndPassed()
    {
        PointsCalc pointsCalc = new(emptyBaseResults, false);
        var participantResults = new List<ParticipantResult>
        {
            new("H10", "Adam", "Club A", TimeSpan.FromHours(18), null, NotStarted),
            new("H10", "Rory", "Club B", TimeSpan.FromHours(18), null, Started),
            new("H10", "Hugo", "Club C", TimeSpan.FromHours(18), TimeSpan.FromMinutes(10), Passed),
        };
        var scoreBoard = pointsCalc.CalcScoreBoard(participantResults);
        Assert.AreEqual(3, scoreBoard.Count);
        Assert.AreEqual(new TeamResult(1, "Club C", 50, false), scoreBoard[0]);
        Assert.AreEqual(new TeamResult(2, "Club A", 0, false), scoreBoard[1]);
        Assert.AreEqual(new TeamResult(2, "Club B", 0, false), scoreBoard[2]);
    }

    [TestMethod]
    public void TestWithCheckedPreliminaryAndPassed()
    {
        PointsCalc pointsCalc = new(emptyBaseResults, false);
        var participantResults = new List<ParticipantResult>
        {
            new("H10", "Adam", "Club A", TimeSpan.FromHours(18), null, NotStarted),
            new("H10", "Rory", "Club B", TimeSpan.FromHours(18), null, Started),
            new("H10", "Hugo", "Club C", TimeSpan.FromHours(18), TimeSpan.FromMinutes(10), Preliminary),
            new("H10", "Hugo", "Club B", TimeSpan.FromHours(18), TimeSpan.FromMinutes(12), Passed),
        };
        var scoreBoard = pointsCalc.CalcScoreBoard(participantResults);
        Assert.AreEqual(3, scoreBoard.Count);
        Assert.AreEqual(new TeamResult(1, "Club C", 50, true), scoreBoard[0]);
        Assert.AreEqual(new TeamResult(2, "Club B", 46, false), scoreBoard[1]);
        Assert.AreEqual(new TeamResult(3, "Club A", 0, false), scoreBoard[2]);
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

        var normalScoreBoard = new PointsCalc(emptyBaseResults, false).CalcScoreBoard(participantResults);
        Assert.AreEqual(3, normalScoreBoard.Count);
        Assert.AreEqual(new TeamResult(1, "Club B", 40+26, false), normalScoreBoard[0]);
        Assert.AreEqual(new TeamResult(2, "Club C", 50+10, false), normalScoreBoard[1]);
        Assert.AreEqual(new TeamResult(3, "Club A", 38, false), normalScoreBoard[2]);

        var finalScoreBoard = new PointsCalc(emptyBaseResults, true).CalcScoreBoard(participantResults);
        Assert.AreEqual(3, finalScoreBoard.Count);
        Assert.AreEqual(new TeamResult(1, "Club C", 100 + 20, false), finalScoreBoard[0]);
        Assert.AreEqual(new TeamResult(2, "Club B", 80 + 20, false), finalScoreBoard[1]);
        Assert.AreEqual(new TeamResult(3, "Club A", 70, false), finalScoreBoard[2]);
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

    private static int CalcFinalPoints(string @class, string club, string startTime, string time, string bestTime, bool isExtraParticipant = false)
    {
        var participantResult = new PointsCalcParticipantResult(@class, "", club, TimeSpan.Parse(startTime), TimeSpan.Parse(time), Passed)
        {
            IsExtraParticipant = isExtraParticipant
        };

        return PointsCalc.CalcFinalPoints(participantResult, TimeSpan.Parse(bestTime));
    }
}
