namespace Results.Model
{
    class BasePoints
    {
        public string Team { get; }
        public int Points { get; }

        public BasePoints(string team, int points)
        {
            Team = team;
            Points = points;
        }
    }
}