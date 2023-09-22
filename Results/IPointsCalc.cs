using Results.Contract;
using Results.Model;

namespace Results
{
    internal interface IPointsCalc
    {
        List<TeamResult> CalcScoreBoard(TimeSpan currentTimeOfDay, IEnumerable<ParticipantResult> participants);
        IList<ParticipantPoints> GetParticipantPoints(TimeSpan currentTimeOfDay, IEnumerable<ParticipantResult> participants);
    }
}