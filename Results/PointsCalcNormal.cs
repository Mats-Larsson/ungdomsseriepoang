using Results.Contract;

namespace Results;

internal class PointsCalcNormal(ITeamService teamService, Configuration configuration)
    : PointsCalcBase(teamService, configuration)
{
    protected override int CalcPoints1(PointsTemplate pointsTemplate, TimeSpan time, int pos, TimeSpan bestTime, bool isExtraParticipant)
    {
        var points = pointsTemplate.BasePoints
                     - pointsTemplate.MinuteReduction * MinutesAfter(bestTime, time)
                     - pointsTemplate.PositionReduction * (pos - 1)
                     - (isExtraParticipant ? pointsTemplate.PatrolExtraParticipantsReduction : 0);

        return Math.Max(points, pointsTemplate.MinPoints);
    }

    private static int MinutesAfter(TimeSpan bestTime, TimeSpan time)
    {
        var secondsAfter = (time - bestTime).TotalSeconds;
        return (int)Math.Truncate((secondsAfter + 59) / 60.0);
    }
}