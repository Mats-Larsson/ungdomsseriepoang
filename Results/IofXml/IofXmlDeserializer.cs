using System.Xml;
using System.Xml.Serialization;
using Results.Contract;
using Results.Model;

namespace Results.IofXml;

public class IofXmlDeserializer : IIofXmlDeserializer
{
    public IofXmlResult Deserialize(Stream xmlStream)
    {
        using XmlTextReader xmlReader = new(xmlStream);
        XmlAttributeOverrides overrides = new();
        XmlAttributes ignore = new() { XmlIgnore = true };
        overrides.Add(typeof(PersonRaceResult), "SplitTime", ignore);
        XmlSerializer xml = new(typeof(ResultList), overrides);
        var resultList = (ResultList)(xml.Deserialize(xmlReader)!);

        var currentTimeOfDay = resultList.createTime.TimeOfDay;
        var competitionName = resultList.Event.Name;
        var participantResults =  resultList.ClassResult?
            .SelectMany(cr => (cr.PersonResult ?? [])
                .Select(pr => new ParticipantResult(competitionName, cr.Class.Name, ToName(pr), pr.Organisation.Name, ToStartTime(pr),
                    ToTimeSpan(pr), MapStatus(pr.Result[0].Status))))
            .ToList() ?? [];
        
        return new IofXmlResult(currentTimeOfDay, competitionName, participantResults);
    }
    private static TimeSpan? ToTimeSpan(PersonResult pr)
    {
        double seconds = pr.Result?[0]?.Time ?? 3600 * 4;
        return TimeSpan.FromSeconds(seconds);
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
}