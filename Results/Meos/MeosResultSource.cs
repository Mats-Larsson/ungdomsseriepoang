using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Results.Model;

namespace Results.Meos;

public sealed class MeosResultSource : IResultSource
{
    internal static XNamespace MopNs => XNamespace.Get("http://www.melin.nu/mop");

    private readonly IDictionary<int, MeosParticipantResult> participantResults = new Dictionary<int, MeosParticipantResult>();
    private readonly IDictionary<int, string> classes = new Dictionary<int, string>();
    private readonly IDictionary<int, string> clubs = new Dictionary<int, string>();
    private readonly ILogger<MeosResultSource> logger;

    public TimeSpan CurrentTimeOfDay { get; private set; } = DateTime.Now.TimeOfDay;

    public MeosResultSource(ILogger<MeosResultSource> logger)
    {
        this.logger = logger;
    }

    public bool SupportsPreliminary => false;

    public IList<ParticipantResult> GetParticipantResults()
    {
        return participantResults
            .Where(item => item.Key > 0)
            .Select(item => item.Value)
            .Cast<ParticipantResult>()
            .ToList();
    }

    public async Task<string> NewResultPostAsync(Stream body, DateTime timestamp)
    {
        CurrentTimeOfDay = timestamp.TimeOfDay;

        var doc = await XDocument.LoadAsync(body, LoadOptions.None, CancellationToken.None).ConfigureAwait(false);
        logger.LogInformation("{Name} {Count}", doc.Root?.Name.LocalName, doc.Root?.Elements().Count());
        if (doc.Root?.Name.LocalName == "MOPComplete")
        {
            classes.Clear();
            clubs.Clear();
            participantResults.Clear();
        }

        UpdateClasses(doc);
        UpdateClubs(doc);
        UpdateParticipants(doc);

        return MopStatus("OK");
    }

    private void UpdateClasses(XDocument doc)
    {
        var pairs = doc.Root!
            .Elements(MopNs + "cls")
            .ToDictionary(e => int.Parse(e.Attribute("id")!.Value), e => e.Attribute("delete")?.Value != "true" ? e.Value : null);
        foreach (var (id, value) in pairs)
        {
            if (value == null)
                classes.Remove(id);
            else
                classes[id] = value;
        }
        classes[0] = "???";
    }

    private void UpdateClubs(XDocument doc)
    {
        var orgs = doc.Root!
            .Elements(MopNs + "org")
            .ToDictionary(e => int.Parse(e.Attribute("id")!.Value), e => e.Attribute("delete")?.Value != "true" ? e.Value : null);
        foreach (var (id, value) in orgs)
        {
            if (value == null)
                clubs.Remove(id);
            else
                clubs[id] = value;
        }
        clubs[0] = "???";
    }

    private void UpdateParticipants(XDocument doc)
    {// e => e.Attribute("delete")?.Value != "true" ? e.Value : null);
        var meosParticipants = doc.Root!
            .Elements(MopNs + "cmp")
            .ToDictionary(cmp => int.Parse(cmp.Attribute("id")!.Value), cmp =>
            {
                if (cmp.Attribute("delete")?.Value == "true") return null;

                var @base = cmp.Element(MopNs + "base");
                if (@base == null) return null;
                return new
                {
                    IsActivated = cmp.Attribute("competing")?.Value == "true",
                    OrgId = ToInt(@base.Attribute("org")?.Value) ?? 0,
                    ClsId = ToInt(@base.Attribute("cls")?.Value) ?? 0,
                    Name = @base.Value,
                    StartTime = ToTimeSpan(@base.Attribute("st")?.Value),
                    Time = ToTimeSpan(@base.Attribute("rt")?.Value),
                    Stat = ToInt(@base.Attribute("stat")?.Value) ?? 0
                };
            });
        foreach (var (id, value) in meosParticipants)
        {
            if (value == null)
            {
                participantResults.Remove(id);
                continue;
            }
            var meosParticipantResult = new MeosParticipantResult(
                classes[value.ClsId],
                value.Name,
                clubs[value.OrgId],
                value.StartTime,
                value.Time,
                ToParticipantStatus(value.Stat, value.IsActivated),
                false // TODO: Patrol
            );
            participantResults[id] = meosParticipantResult;
        }

        participantResults[0] = new MeosParticipantResult("???", "???", "???", null, null, ParticipantStatus.Ignored);
    }

    private static ParticipantStatus ToParticipantStatus(int stat, bool isActivated)
    {
        return stat switch
        {
            0 => // Unknown.This is the default status until something is known about the competitor. The competing attribute(new in version 3.7) can indicate if a competitor has started.
                isActivated ? ParticipantStatus.Activated : ParticipantStatus.NotActivated,
            1 => // OK The running time is valid if and only if status is OK or OCC.
                ParticipantStatus.Passed,
            2 => // NT(No timing).This indicates a valid result, but no timing or result is desired.New in version 3.7.
                ParticipantStatus.Passed,
            3 => // MP(Missing punch)
                ParticipantStatus.NotValid,
            4 => // DNF(Did not finish)
                ParticipantStatus.NotValid,
            5 => // DQ(Disqualified)
                ParticipantStatus.NotValid,
            6 => // OT(Overtime)
                ParticipantStatus.NotValid,
            15 => //  OCC(Out‐of‐competition)(new in version 3.7)
                ParticipantStatus.Ignored,
            20 => //  DNS(Did not start)
                ParticipantStatus.NotStarted,
            21 => //  CANCEL(Cancelled entry)
                ParticipantStatus.Ignored,
            99 => //  NP(Not participating)
                ParticipantStatus.Ignored,
            _ => ParticipantStatus.NotValid
        };
    }

    private static TimeSpan? ToTimeSpan(string? value)
    {
        if (value == null) return null;

        var intValue = int.Parse(value);
        if (intValue == -1) return null;

        return TimeSpan.FromMilliseconds(intValue * 100);
    }

    private static int? ToInt(string? value)
    {
        if (string.IsNullOrEmpty(value)) return null;
        return int.Parse(value);
    }

    private static string MopStatus(string status)
    {
        return $"<?xml version=\"1.0\"?><MOPStatus status=\"{status}\"></MOPStatus>";
    }

    public void Dispose()
    {
        // Nothing to dispose
    }
}