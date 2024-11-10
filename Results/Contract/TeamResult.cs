namespace Results.Contract;

public class TeamResult(
    int pos,
    string team,
    int points,
    bool isPreliminary,
    int diffPointsUp,
    int basePoints,
    Statistics statistics)
{
    public int Pos { get; } = pos;
    public string Team { get; } = team;
    public int Points { get; } = points;
    public bool IsPreliminary { get; } = isPreliminary;
    public int DiffPointsUp { get; } = diffPointsUp;
    public int BasePoints { get; } = basePoints;
    public Statistics Statistics { get; } = statistics;

    public override string ToString()
    {
        return $"{nameof(Pos)}: {Pos}, {nameof(Team)}: {Team}, {nameof(Points)}: {Points}, {nameof(IsPreliminary)}: {IsPreliminary}, {nameof(DiffPointsUp)}: {DiffPointsUp}, {nameof(BasePoints)}: {BasePoints}, {nameof(Statistics)}: {Statistics}";
    }

#pragma warning disable CA1062
    // ReSharper disable once MemberCanBePrivate.Global
    protected bool Equals(TeamResult other)
    {
        return Pos == other.Pos && Team == other.Team && Points == other.Points && IsPreliminary == other.IsPreliminary && DiffPointsUp == other.DiffPointsUp && BasePoints == other.BasePoints && Statistics.Equals(other.Statistics);
    }
#pragma warning restore CA1062

#pragma warning disable IDE0041 // Use 'is null' check
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        return obj.GetType() == this.GetType() && Equals((TeamResult)obj);
    }
#pragma warning restore IDE0041 // Use 'is null' check

    public override int GetHashCode()
    {
        return HashCode.Combine(Pos, Team, Points, IsPreliminary, DiffPointsUp, BasePoints, Statistics);
    }
}