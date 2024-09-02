using Results.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Results.Liveresultat;

public class LiveresultatResultSource : IResultService
{
    public bool SupportsPreliminary => false;

    public event EventHandler? OnNewResults;

    public IEnumerable<ParticipantPoints> GetParticipantPointsList()
    {
        throw new NotImplementedException();
    }

    public Result GetScoreBoard()
    {
        throw new NotImplementedException();
    }

    public Task<string> NewResultPostAsync(Stream body, DateTime timestamp)
    {
        throw new NotImplementedException();
    }
}
