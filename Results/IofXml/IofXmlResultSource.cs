using System.Diagnostics.CodeAnalysis;
using Results.Contract;
using Results.Model;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;

namespace Results.IofXml;

public sealed class IofXmlResultSource : IResultSource
{
    private ResultList? resultList;

    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    private readonly Configuration configuration;

    private readonly FileListener fileListener;
    private readonly ILogger<IofXmlResultSource> logger;
    private int lastFileNumber;

    public bool SupportsPreliminary => false;

    public TimeSpan CurrentTimeOfDay => resultList?.createTime.TimeOfDay ?? TimeSpan.Zero;

    public IofXmlResultSource(Configuration configuration, FileListener fileListener,
        ILogger<IofXmlResultSource> logger)
    {
        this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        this.fileListener = fileListener ?? throw new ArgumentNullException(nameof(fileListener));
        this.logger = logger;
        fileListener.NewFile += FileListener_NewFile;
    }

    private void FileListener_NewFile(object? sender, NewFileEventArgs e)
    {
        try
        {
            resultList = LoadResultFile(e.FullName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while loading result file");
        }
    }

    public IList<ParticipantResult> GetParticipantResults()
    {
        return resultList?.ClassResult?
            .SelectMany(cr => (cr.PersonResult ?? [])
                .Select(pr => new ParticipantResult(cr.Class.Name, ToName(pr), pr.Organisation.Name, ToStartTime(pr),
                    ToTimeSpan(pr), MapStatus(pr.Result[0].Status))))
            .ToList() ?? [];
    }

    private static TimeSpan? ToTimeSpan(PersonResult pr)
    {
        double? seconds = pr.Result?[0]?.Time;
        return seconds.HasValue ? TimeSpan.FromSeconds(seconds.Value) : TimeSpan.Zero;
    }

    private static TimeSpan ToStartTime(PersonResult pr)
    {
        var startTime = pr.Result?[0]?.StartTime;
        return startTime?.TimeOfDay ?? TimeSpan.Zero;
    }

    private static string ToName(PersonResult pr)
    {
        PersonName name = pr.Person.Name;
        return $"{name.Given} {name.Family}";
    }

    private static ParticipantStatus MapStatus(ResultStatus status)
    {
        switch (status)
        {
            case ResultStatus.OK:
            case ResultStatus.Finished:
                return ParticipantStatus.Passed;
            case ResultStatus.MissingPunch:
            case ResultStatus.Disqualified:
            case ResultStatus.DidNotFinish:
            case ResultStatus.OverTime:
                return ParticipantStatus.NotValid;
            case ResultStatus.DidNotStart:
            case ResultStatus.NotCompeting:
            case ResultStatus.DidNotEnter:
            case ResultStatus.Moved:
            case ResultStatus.MovedUp:
            case ResultStatus.Cancelled:
            case ResultStatus.SportingWithdrawal:
                return ParticipantStatus.NotStarted;
            case ResultStatus.Active:
            case ResultStatus.Inactive:
            default: throw new NotImplementedException($"{status}");
        }
    }

    public Task<string> NewResultPostAsync(Stream body, DateTime timestamp)
    {
        throw new NotImplementedException();
    }

    private ResultList LoadResultFile(string path)
    {
        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            using XmlTextReader xmlReader = new(stream);
            XmlAttributeOverrides overrides = new();
            XmlAttributes ignore = new() { XmlIgnore = true };
            overrides.Add(typeof(PersonRaceResult), "SplitTime", ignore);
            XmlSerializer xml = new(typeof(ResultList), overrides);
            return (ResultList)(xml.Deserialize(xmlReader)!);
        }
        finally
        {
            RenameFile(path);
        }
    }

    private void RenameFile(string path)
    {
        string folder = Path.GetDirectoryName(path)!;
        string fileName = Path.GetFileName(path);

        for (var i = lastFileNumber; i < 10000; i++)
        {
            string newPath = Path.Combine(folder, $"{fileName}.{i:0000}");
            if (File.Exists(newPath)) continue;

            try
            {
                File.Move(path, newPath);
                lastFileNumber = (i + 1) % 10000;
                return;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while renameing file");
            }
        }
    }

    public void Dispose()
    {
        fileListener.Dispose();
    }
}