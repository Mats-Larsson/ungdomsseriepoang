using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
