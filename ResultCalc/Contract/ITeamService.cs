namespace ResultCalc.Contract;

internal interface ITeamService
{
    IDictionary<string, int> TeamBasePoints { get; }
    ICollection<string>? Teams { get; }
}