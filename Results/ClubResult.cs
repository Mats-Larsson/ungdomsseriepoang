namespace Results
{
    public class ClubResult
    {
        public int Pos { get; }
        public string Team { get; }
        public int Points { get; }
        public ClubResult(int pos, string team, int points)
        {
            Pos = pos;
            Team = team;
            Points = points;
        }

        private bool Equals(ClubResult other)
        {
            return Pos == other.Pos && Team == other.Team && Points == other.Points;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() && Equals((ClubResult)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Pos, Team, Points);
        }
    }
}