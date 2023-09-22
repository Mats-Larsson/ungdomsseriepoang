namespace Results.Contract;

public interface ITeamService
{
    IDictionary<string, int> TeamBasePoints { get; }
    ICollection<string>? Teams { get; }
}