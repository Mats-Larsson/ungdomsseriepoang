using Results.Contract;

namespace Results.Model;

public record ParticipantResult(string CompititionName, string Class, string Name, string Club, TimeSpan? StartTime, TimeSpan? Time, ParticipantStatus Status)
{
    public string CompititionName { get; } = CompititionName; 
    public string Club { get; internal set; } = Club;
    public TimeSpan? Time { get; internal set; } = Time;
    public ParticipantStatus Status { get; internal set; } = Status;

    protected ParticipantResult(ParticipantResult pr)
    {
        if (pr == null) throw new ArgumentNullException(nameof(pr));

        CompititionName = pr.CompititionName;
        Class = pr.Class;
        Name = pr.Name;
        Club = pr.Club;
        StartTime = pr.StartTime;
        Time = pr.Time;
        Status = pr.Status;
    }
}