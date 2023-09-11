using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

namespace Results.Model;

[Delimiter(",")]
// ReSharper disable once ClassNeverInstantiated.Global
class Team
{
    [Name("Name")]
    public string Name { get; }

    [Name("Points"), Optional]
    public int? BasePoints { get; }

    public Team(string name)
    {
        Name = name;
    }

    public Team(string name, int? basePoints)
    {
        Name = name;
        BasePoints = basePoints ?? 0;
    }
}
