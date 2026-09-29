using ResultCalc.Contract;
using ResultCalc.Model;

namespace ResultCalc.Meos;

internal record MeosParticipantResult(string CompititionName, string Class, string Name, string Club, TimeSpan? StartTime, TimeSpan? Time, ParticipantStatus Status)
    : ParticipantResult(CompititionName, Class, Name, Club, StartTime, Time, Status);