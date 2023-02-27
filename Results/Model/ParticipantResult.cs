namespace Results.Model;

internal class ParticipantResult
{
    public string Class { get; }
    public string Name { get; }
    public string Club { get; }
    public TimeSpan? StartTime { get; }
    public TimeSpan? Time { get; internal set; }
    public ParticipantStatus Status { get; internal set; }

    public ParticipantResult(string @class, string name, string club, TimeSpan? startTime, TimeSpan? time, ParticipantStatus status)
    {
        Class = @class;
        Name = name;
        Club = club;
        StartTime = startTime;
        Time = time;
        Status = status;
    }
}