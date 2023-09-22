using Results.Contract;
using Results.Model;

namespace Results
{
    internal interface IPointsCalc
    {
        IList<TeamResult> CalcScoreBoard(TimeSpan currentTimeOfDay, IEnumerable<ParticipantResult> participants);
        IEnumerable<ParticipantPoints> GetParticipantPoints(TimeSpan currentTimeOfDay, IEnumerable<ParticipantResult> participants);
    }
}