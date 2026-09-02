using CsvHelper.Configuration.Attributes;

namespace Results.Model;

[Delimiter(",")]
// ReSharper disable once ClassNeverInstantiated.Global
class Team(string name, int? basePoints)
{
    [Name("Name")]
    public string Name { get; } = name;

    [Name("Points"), Optional]
    public int? BasePoints { get; } = basePoints ?? 0;
}
