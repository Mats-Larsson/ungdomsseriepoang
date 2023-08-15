namespace Results.Contract
{
    public class Result
    {
        public IList<TeamResult> TeamResults { get; }
        public Statistics Statistics { get; }

        public Result(IList<TeamResult> teamResults, Statistics statistics)
        {
            TeamResults = teamResults;
            Statistics = statistics;
        }
    }
}
