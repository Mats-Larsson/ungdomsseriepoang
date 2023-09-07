using Results.Model;

namespace Results.Contract;

public class ParticipantPoints
{
    public string Class { get; }
    public string Name { get; }
    public string Club { get; }
    public TimeSpan? StartTime { get; }
    public TimeSpan? Time { get; }
    public ParticipantStatus Status { get; }
    public bool IsExtraParticipant { get; }
    public int Points { get; }

    public ParticipantPoints(string @class, string name, string club, TimeSpan? startTime, TimeSpan? time, ParticipantStatus status, bool isExtraParticipant, int points)
    {
        Class = @class;
        Name = name;
        Club = club;
        StartTime = startTime;
        Time = time;
        Status = status;
        IsExtraParticipant = isExtraParticipant;
        Points = points;
    }
}