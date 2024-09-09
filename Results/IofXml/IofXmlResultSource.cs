using Results.Contract;
using Results.Model;
using System.Xml;
using System.Xml.Serialization;

namespace Results.IofXml;

public sealed class IofXmlResultSource : IResultSource
{
    private ResultList? resultList;
    private readonly Configuration configuration;
    private readonly FileListener fileListener;

    public bool SupportsPreliminary => false;

    public TimeSpan CurrentTimeOfDay => resultList?.createTime.TimeOfDay ?? TimeSpan.Zero;

    public IofXmlResultSource(Configuration configuration, FileListener fileListener)
    {
        this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        this.fileListener = fileListener ?? throw new ArgumentNullException(nameof(fileListener));
        fileListener.NewFile += FileListener_NewFile;
    }

    private void FileListener_NewFile(object? sender, NewFileEventArgs e)
    {
        resultList = LoadResultFile(e.FullName);
    }

    public IList<ParticipantResult> GetParticipantResults()
    {
        return resultList?.ClassResult
            .SelectMany(cr => cr.PersonResult
            .Select(pr => new ParticipantResult(cr.Class.Name, ToName(pr), pr.Organisation.Name, ToStartTime(pr), ToTimeSpan(pr), MapStatus(pr.Result[0].Status))))
            .ToList() ?? [];
    }

    private static TimeSpan? ToTimeSpan(PersonResult pr)
    {
        double? seconds = pr.Result?[0]?.Time;
        return seconds.HasValue ? TimeSpan.FromSeconds(seconds.Value) : default;
    }

    private static TimeSpan ToStartTime(PersonResult pr)
    {
        var startTime = pr.Result?[0]?.StartTime;
        return startTime?.TimeOfDay ?? default;
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

    public static ResultList LoadResultFile(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        using XmlTextReader xmlReader = new(stream);
        XmlAttributeOverrides overrides = new();
        XmlAttributes ignore = new() { XmlIgnore = true };
        overrides.Add(typeof(PersonRaceResult), "SplitTime", ignore);
        XmlSerializer xml = new(typeof(ResultList), overrides);
        ResultList resultList = (ResultList)(xml.Deserialize(xmlReader)!);

        return resultList;
    }

    public void Dispose()
    {
        fileListener.Dispose();
    }
}
