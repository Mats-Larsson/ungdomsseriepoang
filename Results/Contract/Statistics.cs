﻿using System.Diagnostics.CodeAnalysis;
using Results.Model;

namespace Results.Contract;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class Statistics
{
    public Statistics(int numNotActivated = 0, int numActivated = 0, int numStarted = 0, int numPreliminary = 0, 
        int numPassed = 0, int numNotValid = 0, int numNotStarted = 0)
    {
        NumNotActivated = numNotActivated;
        NumActivated = numActivated;
        NumStarted = numStarted;
        NumPreliminary = numPreliminary;
        NumPassed = numPassed;
        NumNotValid = numNotValid;
        NumNotStarted = numNotStarted;
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")] 
    public TimeSpan LastUpdatedTimeOfDay { get; internal set; }
    public TimeSpan LastChangedTimeOfDay { get; private set; }
    public int NumNotActivated { get; private set; }
    public int NumActivated { get; private set; }
    public int NumStarted { get; private set; }
    public int NumPreliminary { get; private set; }
    public int NumPassed { get; private set; }
    public int NumNotValid { get; private set; }
    public int NumNotStarted { get; private set; }

    internal void IncNumNotActivated() { NumNotActivated++; }
    internal void IncNumActivated() { NumActivated++;}
    internal void IncNumStarted() { NumStarted++; }
    internal void IncNumPreliminary() { NumPreliminary++; }
    internal void IncNumPassed() { NumPassed++;}
    internal void IncNumNotValid() { NumNotValid++; }
    internal void IncNumNotStarted() { NumNotStarted++; }

    public static Statistics GetStatistics(IEnumerable<ParticipantResult> participantResults, TimeSpan currentTimeOfDay)
    {
        if (participantResults == null) throw new ArgumentNullException(nameof(participantResults));

        var statistics = new Statistics
        {
            LastChangedTimeOfDay = currentTimeOfDay
        };

        foreach (var pr in participantResults)
        {
            switch (pr.Status)
            {
                case ParticipantStatus.Ignored: break;
                case ParticipantStatus.NotActivated: statistics.IncNumNotActivated(); break;
                case ParticipantStatus.Activated: statistics.IncNumActivated(); break;
                case ParticipantStatus.Started: statistics.IncNumStarted(); break;
                case ParticipantStatus.Preliminary: statistics.IncNumPreliminary(); break;
                case ParticipantStatus.Passed: statistics.IncNumPassed(); break;
                case ParticipantStatus.NotValid: statistics.IncNumNotValid(); break;
                case ParticipantStatus.NotStarted: statistics.IncNumNotStarted(); break;
                default: throw new InvalidOperationException($"Unexpected: {pr.Status}");
            }
        }
        return statistics;
    }

    public override string ToString()
    {
        return $"{nameof(NumNotActivated)}: {NumNotActivated}, {nameof(NumActivated)}: {NumActivated}, {nameof(NumStarted)}: {NumStarted}, {nameof(NumPreliminary)}: {NumPreliminary}, {nameof(NumPassed)}: {NumPassed}, {nameof(NumNotValid)}: {NumNotValid}, {nameof(NumNotStarted)}: {NumNotStarted}";
    }

#pragma warning disable CA1062
    protected bool Equals(Statistics other)
    {
        return NumNotActivated == other.NumNotActivated && NumActivated == other.NumActivated && NumStarted == other.NumStarted && NumPreliminary == other.NumPreliminary && NumPassed == other.NumPassed && NumNotValid == other.NumNotValid && NumNotStarted == other.NumNotStarted;
    }
#pragma warning restore CA1062

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;

        if (ReferenceEquals(this, obj)) return true;

        return obj.GetType() == this.GetType() && Equals((Statistics)obj);
    }

    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public override int GetHashCode()
    {
        return HashCode.Combine(NumNotActivated, NumActivated, NumStarted, NumPreliminary, NumPassed, NumNotValid, NumNotStarted);
    }
}

