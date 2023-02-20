using Results;
using Results.Contract;
using Results.Model;
using Results.Simulator;

namespace ResultsTests;

[TestClass]
public class PointsCalcTest
{
    private readonly PointsCalc pointsCalc = new();

    [TestMethod]
    public void TestWithSimulatorResults()
    {
        var participantResults = (new SimulatorResultSourceImpl()).GetParticipantResults();
        var scoreBoard = pointsCalc.CalcScoreBoard(participantResults);
        Assert.AreEqual(4, scoreBoard.Count);
    }

    [TestMethod]
    public void TestWithNoResults()
    {
        var participantResults = new List<ParticipantResult>();
        var scoreBoard = pointsCalc.CalcScoreBoard(participantResults);
        Assert.AreEqual(0, scoreBoard.Count);
    }

    [TestMethod]
    public void TestBeforeCompetition()
    {
        var participantResults = new List<ParticipantResult>
        {
            new("H10", "Adam", "Club A", null, ParticipantStatus.NotStarted),
            new("H10", "Rory", "Club B", null, ParticipantStatus.NotStarted),
        };
        var scoreBoard = pointsCalc.CalcScoreBoard(participantResults);
        Assert.AreEqual(2, scoreBoard.Count);
        Assert.AreEqual(new TeamResult(1, "Club A", 0, false), scoreBoard[0]);
        Assert.AreEqual(new TeamResult(1, "Club B", 0, false), scoreBoard[1]);
    }

    [TestMethod]
    public void TestWithIgnored()
    {
        var participantResults = new List<ParticipantResult>
        {
            new("H10", "Adam", "Club A", null, ParticipantStatus.NotStarted),
            new("H10", "Rory", "Club B", null, ParticipantStatus.NotStarted),
            new("H10", "Hugo", "Club C", null, ParticipantStatus.Ignored),
        };
        var scoreBoard = pointsCalc.CalcScoreBoard(participantResults);
        Assert.AreEqual(2, scoreBoard.Count);
        Assert.AreEqual(0, scoreBoard[0].Points);
        Assert.AreEqual(0, scoreBoard[1].Points);
    }

    [TestMethod]
    public void TestWithChecked()
    {
        var participantResults = new List<ParticipantResult>
        {
            new("H10", "Adam", "Club A", null, ParticipantStatus.NotStarted),
            new("H10", "Rory", "Club B", null, ParticipantStatus.NotStarted),
            new("H10", "Hugo", "Club C", null, ParticipantStatus.Started),
        };
        var scoreBoard = pointsCalc.CalcScoreBoard(participantResults);
        Assert.AreEqual(3, scoreBoard.Count);
        Assert.AreEqual(new TeamResult(1, "Club C", 5, false), scoreBoard[0]);
        Assert.AreEqual(new TeamResult(2, "Club A", 0, false), scoreBoard[1]);
        Assert.AreEqual(new TeamResult(2, "Club B", 0, false), scoreBoard[2]);
    }

    [TestMethod]
    public void TestWithCheckedAndPreliminary()
    {
        var participantResults = new List<ParticipantResult>
        {
            new("H10", "Adam", "Club A", null, ParticipantStatus.NotStarted),
            new("H10", "Rory", "Club B", null, ParticipantStatus.Started),
            new("H10", "Hugo", "Club C", TimeSpan.FromMinutes(10), ParticipantStatus.Preliminary),
        };
        var scoreBoard = pointsCalc.CalcScoreBoard(participantResults);
        Assert.AreEqual(3, scoreBoard.Count);
        Assert.AreEqual(new TeamResult(1, "Club C", 50, true), scoreBoard[0]);
        Assert.AreEqual(new TeamResult(2, "Club B", 5, false), scoreBoard[1]);
        Assert.AreEqual(new TeamResult(3, "Club A", 0, false), scoreBoard[2]);
    }

    [TestMethod]
    public void TestWithCheckedAndPassed()
    {
        var participantResults = new List<ParticipantResult>
        {
            new("H10", "Adam", "Club A", null, ParticipantStatus.NotStarted),
            new("H10", "Rory", "Club B", null, ParticipantStatus.Started),
            new("H10", "Hugo", "Club C", TimeSpan.FromMinutes(10), ParticipantStatus.Passed),
        };
        var scoreBoard = pointsCalc.CalcScoreBoard(participantResults);
        Assert.AreEqual(3, scoreBoard.Count);
        Assert.AreEqual(new TeamResult(1, "Club C", 50, false), scoreBoard[0]);
        Assert.AreEqual(new TeamResult(2, "Club B", 5, false), scoreBoard[1]);
        Assert.AreEqual(new TeamResult(3, "Club A", 0, false), scoreBoard[2]);
    }

    [TestMethod]
    public void TestWithCheckedPreliminaryAndPassed()
    {
        var participantResults = new List<ParticipantResult>
        {
            new("H10", "Adam", "Club A", null, ParticipantStatus.NotStarted),
            new("H10", "Rory", "Club B", null, ParticipantStatus.Started),
            new("H10", "Hugo", "Club C", TimeSpan.FromMinutes(10), ParticipantStatus.Preliminary),
            new("H10", "Hugo", "Club B", TimeSpan.FromMinutes(12), ParticipantStatus.Passed),
        };
        var scoreBoard = pointsCalc.CalcScoreBoard(participantResults);
        Assert.AreEqual(3, scoreBoard.Count);
        Assert.AreEqual(new TeamResult(1, "Club B", 51, false), scoreBoard[0]);
        Assert.AreEqual(new TeamResult(2, "Club C", 50, true), scoreBoard[1]);
        Assert.AreEqual(new TeamResult(3, "Club A", 0, false), scoreBoard[2]);
    }

    [TestMethod]
    public void TestCalcPoints()
    {
        // Max
        Assert.AreEqual(50, CalcPoints("H10", "00:10:30", "00:10:30"));
        Assert.AreEqual(50, CalcPoints("D10", "00:10:30", "00:10:30"));
        Assert.AreEqual(40, CalcPoints("U1", "00:10:30", "00:10:30"));
        Assert.AreEqual(10, CalcPoints("Insk", "00:10:30", "00:10:30"));

        // Min
        Assert.AreEqual(15, CalcPoints("H10", "02:10:30", "00:10:30"));
        Assert.AreEqual(15, CalcPoints("D10", "02:10:30", "00:10:30"));
        Assert.AreEqual(10, CalcPoints("U1", "02:10:30", "00:10:30"));
        Assert.AreEqual(10, CalcPoints("Insk", "02:10:30", "00:10:30"));

        // Reduction
        Assert.AreEqual(48, CalcPoints("H10", "00:10:31", "00:10:30"));
        Assert.AreEqual(48, CalcPoints("H10", "00:11:29", "00:10:30"));
        Assert.AreEqual(48, CalcPoints("H10", "00:11:30", "00:10:30"));
        Assert.AreEqual(46, CalcPoints("H10", "00:11:31", "00:10:30"));
        Assert.AreEqual(10, CalcPoints("Insk", "00:10:30", "00:10:30"));
        Assert.AreEqual(10, CalcPoints("Insk", "00:12:30", "00:10:30"));
    }

    private int CalcPoints(string @class, string time, string bestTime)
    {
        return pointsCalc.CalcPoints(@class, TimeSpan.Parse(time), ParticipantStatus.Passed, TimeSpan.Parse(bestTime));
    }
}