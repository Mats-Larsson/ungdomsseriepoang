using System.Diagnostics.CodeAnalysis;

namespace Results.Contract;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
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

    public ParticipantPoints(PointsCalcParticipantResult pr, int points)
    {
        if (pr is null) throw new ArgumentNullException(nameof(pr));

        Class = pr.Class;
        Name = pr.Name;
        Club = pr.Club;
        StartTime = pr.StartTime;
        Time = pr.Time;
        Status = pr.Status;
        IsExtraParticipant = pr.IsExtraParticipant;
        Points = points;
    }
}