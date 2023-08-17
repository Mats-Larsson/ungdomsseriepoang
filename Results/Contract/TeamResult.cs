﻿namespace Results.Contract;

public class TeamResult
{
    public int Pos { get; }
    public string Team { get; }
    public int Points { get; }
    public bool IsPreliminary { get; }

    public int BasePoints { get; }

    public TeamResult(int pos, string team, int points, bool isPreliminary, int basePoints = 0)
    {
        Pos = pos;
        Team = team;
        Points = points;
        IsPreliminary = isPreliminary;
        BasePoints = basePoints;
    }

    public override string ToString()
    {
        return $"{nameof(Pos)}: {Pos}, {nameof(Team)}: {Team}, {nameof(Points)}: {Points}, {nameof(IsPreliminary)}: {IsPreliminary}";
    }

    private bool Equals(TeamResult other)
    {
        return Pos == other.Pos && Team == other.Team && Points == other.Points && IsPreliminary == other.IsPreliminary;
    }

#pragma warning disable IDE0041 // Use 'is null' check
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((TeamResult)obj);
    }
#pragma warning restore IDE0041 // Use 'is null' check

    public override int GetHashCode()
    {
        return HashCode.Combine(Pos, Team, Points, IsPreliminary);
    }
}