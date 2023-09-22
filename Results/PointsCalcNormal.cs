namespace Results;

internal class PointsCalcNormal : PointsCalcBase
{
    public PointsCalcNormal(IDictionary<string, int> basePoints, Configuration configuration) : base(basePoints, configuration)
    {
    }

    protected override int CalcPoints1(PointsTemplate pointsTemplate, TimeSpan time, TimeSpan bestTime, bool isExtraParticipant)
    {

        var points = pointsTemplate.BasePoints
                     - pointsTemplate.MinuteReduction * MinutesAfter(bestTime, time)
                     - (isExtraParticipant ? pointsTemplate.PatrolExtraParticipantsReduction : 0);

        return Math.Max(points, pointsTemplate.MinPoints);
    }

    private static int MinutesAfter(TimeSpan bestTime, TimeSpan time)
    {
        var secondsAfter = (time - bestTime).TotalSeconds;
        return (int)Math.Truncate((secondsAfter + 59) / 60.0);
    }
}
