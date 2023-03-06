﻿using System.Collections;
using System.Diagnostics;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Org.BouncyCastle.Asn1.Ocsp;
using Results.Model;

namespace Results.Meos;

internal class MeosResultSource : IResultSource
{
    internal static XNamespace MopNs => XNamespace.Get("http://www.melin.nu/mop");

    private readonly IDictionary<int, MeosParticipantResult> participantResults = new Dictionary<int, MeosParticipantResult>();
    private readonly IDictionary<int, string> classes = new Dictionary<int, string>();
    private readonly IDictionary<int, string> clubs = new Dictionary<int, string>();

    public IList<ParticipantResult> GetParticipantResults()
    {
        return participantResults.Cast<ParticipantResult>().ToList();
    }

    public TimeSpan CurrentTimeOfDay { get; }

    public async Task<string> NewResultPostAsync(Stream body)
    {
        var doc = await XDocument.LoadAsync(body, LoadOptions.None, CancellationToken.None).ConfigureAwait(false)!;

        if (doc.Root?.Name.LocalName == "MOPDiff")
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

    private void UpdateParticipants(XDocument doc)
    {
        var meosParticipants = doc.Root!.Elements(MopNs + "cmp")
            .ToDictionary(cmp => int.Parse(cmp.Attribute("id")!.Value), cmp =>
            {
                var @base = cmp.Element(MopNs + "base");
                return new
                {
                    IsActivated = cmp.Attribute("competing")?.Value == "true",
                    OrgId = ToInt(@base?.Attribute("org")?.Value) ?? 0,
                    ClsId = ToInt(@base?.Attribute("cls")?.Value) ?? 0,
                    Name = @base?.Value ?? "",
                    StartTime = ToTimeSpan(@base?.Attribute("st")?.Value),
                    Time = ToTimeSpan(@base?.Attribute("rt")?.Value),
                    Stat = ToInt(@base?.Attribute("stat")?.Value) ?? 0
                };
            });
        foreach (var (id, value) in meosParticipants)
        {
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
        return TimeSpan.FromMilliseconds(intValue * 100);
    }

    private static int? ToInt(string? value)
    {
        if (string.IsNullOrEmpty(value)) return null;
        return int.Parse(value);
    }

    private void UpdateClubs(XDocument doc)
    {
        var orgs = doc.Root!
            .Elements(MopNs + "org")
            .ToDictionary(e => int.Parse(e.Attribute("id")!.Value), e => e.Value);
        foreach (var org in orgs)
        {
            clubs[org.Key] = org.Value;
        }

        clubs[0] = "???";
    }

    private void UpdateClasses(XDocument doc)
    {
        var classes = doc.Root!
            .Elements(MopNs + "cls")
            .ToDictionary(e => int.Parse(e.Attribute("id")!.Value), e => e.Value);
        foreach (var cls in classes)
        {
            this.classes[cls.Key] = cls.Value;
        }

        this.classes[0] = "???";
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