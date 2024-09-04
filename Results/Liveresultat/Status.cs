using System.Diagnostics.CodeAnalysis;

namespace Results.Liveresultat;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public enum Status
{
    OK = 0,
    /// <summary>(Did Not Start)</summary>
    DNS = 1,
    /// <summary>Did not finish</summary>
    DNF = 2,
    /// <summary>Missing Punch</summary>
    MP = 3,
    /// <summary>Disqualified</summary>
    DSQ = 4,
    /// <summary> Over (max) time</summary>
    OT = 5,
    NotStartedYet1= 9,
    NotStartedYet2 = 10,
    /// <summary>Resigned before the race started</summary>
    WalkOver = 11,
    /// <summary>The runner have been moved to a higher class</summary>
    MovedUp = 12
}