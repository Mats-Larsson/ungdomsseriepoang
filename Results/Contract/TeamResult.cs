namespace Results.Contract;

public class TeamResult
{
    public int Pos { get; }
    public string Team { get; }
    public int Points { get; }
    public bool IsPreliminary { get; }
    public TeamResult(int pos, string team, int points, bool isPreliminary)
    {
        Pos = pos;
        Team = team;
        Points = points;
        IsPreliminary = isPreliminary;
    }

    public override string ToString()
    {
        return $"{nameof(Pos)}: {Pos}, {nameof(Team)}: {Team}, {nameof(Points)}: {Points}, {nameof(IsPreliminary)}: {IsPreliminary}";
    }

    private bool Equals(TeamResult other)
    {
        return Pos == other.Pos && Team == other.Team && Points == other.Points;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((TeamResult)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Pos, Team, Points);
    }
}