using Results.Contract;

namespace Results.Model;

public class ParticipantResult
{
    public string Class { get; }
    public string Name { get; }
    public string Club { get; internal set; }
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

    protected ParticipantResult(ParticipantResult pr)
    {
        if (pr == null) throw new ArgumentNullException(nameof(pr));

        Class = pr.Class;
        Name = pr.Name;
        Club = pr.Club;
        StartTime = pr.StartTime;
        Time = pr.Time;
        Status = pr.Status;
    }
}