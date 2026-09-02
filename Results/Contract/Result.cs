namespace Results.Contract
{
    public class Result(IList<TeamResult> teamResults, Statistics statistics)
    {
        public IList<TeamResult> TeamResults { get; } = teamResults;
        public Statistics Statistics { get; } = statistics;
    }
}
