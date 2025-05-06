using Results.Contract;
using Results.Model;

namespace Results.Meos;

internal record MeosParticipantResult(string CompititionName, string Class, string Name, string Club, TimeSpan? StartTime, TimeSpan? Time, ParticipantStatus Status)
    : ParticipantResult(CompititionName, Class, Name, Club, StartTime, Time, Status);