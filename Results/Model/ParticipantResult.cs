using Results.Contract;

namespace Results.Model;

public record ParticipantResult(string Class, string Name, string Club, TimeSpan? StartTime, TimeSpan? Time, ParticipantStatus Status)
{
    public string Club { get; internal set; } = Club;
    public TimeSpan? Time { get; internal set; } = Time;
    public ParticipantStatus Status { get; internal set; } = Status;

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