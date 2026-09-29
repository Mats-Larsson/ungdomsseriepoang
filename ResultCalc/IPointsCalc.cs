using ResultCalc.Contract;
using ResultCalc.Model;

namespace ResultCalc
{
    internal interface IPointsCalc
    {
        IList<TeamResult> CalcScoreBoard(TimeSpan currentTimeOfDay, IEnumerable<ParticipantResult> participants);
        IEnumerable<ParticipantPoints> GetParticipantPoints(IEnumerable<ParticipantResult> participants);
    }
}