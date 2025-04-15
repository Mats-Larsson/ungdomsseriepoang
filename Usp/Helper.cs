using System.Collections;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace Usp;

public static class Helper
{
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