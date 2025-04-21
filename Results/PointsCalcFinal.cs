using Results.Contract;

namespace Results;

internal class PointsCalcFinal(ITeamService teamService, Configuration configuration) : PointsCalcBase(teamService, configuration)
{
    protected override int CalcPoints1(PointsTemplate pointsTemplate, TimeSpan time, int pos, TimeSpan bestTime, bool isExtraParticipant)
    {
        if (time <= pointsTemplate.FinalFullPointsTime) return pointsTemplate.FinalFullPoints;

        var points = pointsTemplate.FinalFullPoints
                     - (int)Math.Ceiling((time.TotalMilliseconds - pointsTemplate.FinalFullPointsTime.TotalMilliseconds) / pointsTemplate.FinalReductionTime.TotalMilliseconds);

        int v = Math.Max(points, pointsTemplate.FinalMinPoints);
        return v;
    }
}