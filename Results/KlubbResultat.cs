namespace Results
{
    public class KlubbResultat
    {
        public int Pos { get; }
        public string Team { get; }
        public int Poäng { get; }
        public KlubbResultat(int pos, string team, int poäng)
        {
            Pos = pos;
            Team = team;
            Poäng = poäng;
        }

        private bool Equals(KlubbResultat other)
        {
            return Pos == other.Pos && Team == other.Team && Poäng == other.Poäng;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() && Equals((KlubbResultat)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Pos, Team, Poäng);
        }
    }
}