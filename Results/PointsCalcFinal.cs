using Results.Contract;

namespace Results;

internal class PointsCalcFinal : PointsCalcBase
{
    public PointsCalcFinal(ITeamService teamService, Configuration configuration) : base(teamService, configuration)
    {
    }

    protected override int CalcPoints1(PointsTemplate pointsTemplate, TimeSpan time, TimeSpan bestTime, bool isExtraParticipant)
    {
        if (time <= pointsTemplate.FinalFullPointsTime) return pointsTemplate.FinalFullPoints;

        if (isExtraParticipant) return pointsTemplate.FinalMinPoints;

        var points = pointsTemplate.FinalFullPoints
                     - (int)Math.Ceiling((time.TotalMilliseconds - pointsTemplate.FinalFullPointsTime.TotalMilliseconds) / pointsTemplate.FinalReductionTime.TotalMilliseconds);

        int v = Math.Max(points, pointsTemplate.FinalMinPoints);
        return v;
    }
}