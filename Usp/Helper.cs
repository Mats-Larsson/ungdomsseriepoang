using System.Collections;
using System.Globalization;
using System.Reflection;
using CsvHelper;
using CsvHelper.Configuration;

namespace Usp;

public static class Helper
{
    /// <summary>Version from the release tag, e.g. "1.2.3" (without the "+commit" suffix).</summary>
    public static string AppVersion { get; } =
        (typeof(Helper).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "")
        .Split('+')[0];

    public static string ToCsvText<T>(IEnumerable<T> participantPointsList)
    {
        CsvConfiguration configuration = new(CultureInfo.InvariantCulture)
        {
            Delimiter = "\t"
        };
        using var stream = new MemoryStream();
        using (var writer = new StreamWriter(stream, leaveOpen: true))
        {
            using var csv = new CsvWriter(writer, configuration);
            csv.WriteRecords((IEnumerable)participantPointsList);
            writer.Flush();
        }
        stream.Position = 0;
        using var reader = new StreamReader(stream);
        var text = reader.ReadToEnd();
        return text;
    }
}