using System.Globalization;
using System.IO.Compression;

namespace TestData;

public static class Access
{
    private const string TimestampFormat = "yyyy-MM-dd_HH.mm.ss.fff";

    public static string Final2026ZipPath
    {
        get { return Path.Combine(AppContext.BaseDirectory, "Final 2026-10-04.zip"); }
    }

    /// <summary>
    /// Iterates the xml files in the zip in timestamp order (taken from file names like
    /// "2026-10-04_12.06.56.100.xml"). Each item holds the timestamp from the file name, the file content and
    /// the time until the following file. TimeToNext is null for the last file.
    /// </summary>
    public static IEnumerable<(DateTime Timestamp, string Content, TimeSpan? TimeToNext)> ReadFiles(string zipPath)
    {
        using var archive = ZipFile.OpenRead(zipPath);

        var entries = archive.Entries
            .Select(e => (Entry: e, Time: TryParseTimestamp(e.Name)))
            .Where(x => x.Time.HasValue)
            .OrderBy(x => x.Time)
            .ToList();

        for (int i = 0; i < entries.Count; i++)
        {
            TimeSpan? timeToNext = i + 1 < entries.Count
                ? entries[i + 1].Time - entries[i].Time
                : null;

            using var reader = new StreamReader(entries[i].Entry.Open());
            yield return (entries[i].Time!.Value, reader.ReadToEnd(), timeToNext);
        }
    }

    private static DateTime? TryParseTimestamp(string fileName)
    {
        if (!fileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)) return null;
        return DateTime.TryParseExact(Path.GetFileNameWithoutExtension(fileName), TimestampFormat,
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var time)
            ? time
            : null;
    }
}
