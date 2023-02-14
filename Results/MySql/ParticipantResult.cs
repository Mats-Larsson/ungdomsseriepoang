namespace Results.MySql;

internal class ParticipantResult
{
    public string Class { get; }
    public string Name { get; }
    public string Club { get; }
    public TimeSpan? Time { get; }
    public ParticipantStatus Status { get; }

    public ParticipantResult(string @class, string name, string club, TimeSpan? time, ParticipantStatus status)
    {
        Class = @class;
        Name = name;
        Club = club;
        Time = time;
        Status = status;
    }
}

internal enum ParticipantStatus
{
    Ignored = 0,
    NotStarted,
    Started,
    Passed
}
