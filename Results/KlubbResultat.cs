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
    }
}