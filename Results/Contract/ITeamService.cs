namespace Results.Contract;

public interface ITeamService
{
    IDictionary<string, int> GetTeamBasePoints();
    ICollection<string>? Teams { get; }
}
