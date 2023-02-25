namespace Results.Model;

internal class ParticipantResult
{
    public string Class { get; }
    public string Name { get; }
    public string Club { get; }
    public TimeSpan? Time { get; internal set; }
    public ParticipantStatus Status { get; internal set; }

    public ParticipantResult(string @class, string name, string club, TimeSpan? time, ParticipantStatus status)
    {
        Class = @class;
        Name = name;
        Club = club;
        Time = time;
        Status = status;
    }

    public ParticipantResult(ParticipantResult pr) : this(pr.Club, pr.Name, pr.Club, pr.Time, pr.Status) { }
}

internal enum ParticipantStatus
{
    Ignored = 0,
    NotActivated,
    Started,
    Preliminary,
    Passed,
    NotValid,
    NotStarted
}
