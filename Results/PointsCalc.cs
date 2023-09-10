using Results.Contract;
using Results.Model;
using System.Collections.Immutable;
using static Results.Contract.ParticipantStatus;

namespace Results;

// ReSharper disable CommentTypo
// Kretstävlingar och regiontävlingen
// 50 poäng till segraren i respektive huvudklass oavsett tid, därefter 2 poängs avdrag per påbörjad minut efter segrartiden.

// 40 poäng till segraren i respektive U-klass oavsett tid, därefter 2 poängs avdrag per påbörjad minut efter segrartiden.

// Vid patrull i U-klass får patrullen 40 poäng för löpare nr1 och 30 poäng för extralöparna.I övrigt som ovan.

// 10 poäng för fullföljande av Inskolningsklass.I resultaten för Inskolning ska det stå ”fullföljt” och ingenting annat.

// Minipoäng i huvudklass är 15 poäng och i U-klass 10 poäng.Ej fullföljd tävling ger 5 poäng.OBS! Endast segraren får full poäng!

//Exempel på poängberäkning
//Segrartiden i D10 är 17.45. Segraren får då 50 poäng.Alla som har tider mellan 17.46-18.45 får 48 poäng.Alla som har tider mellan 18.46-19.45 får 46 poäng osv.

// ReSharper restore CommentTypo

internal class PointsCalc
{
    private readonly IDictionary<string, int> basePoints;
    private readonly Configuration configuration;
    private readonly IResultSource resultSource;
    private readonly Func<PointsCalcParticipantResult, TimeSpan?, int> calcPointsFunc;

    public PointsCalc(IDictionary<string, int> basePoints, Configuration configuration, IResultSource resultSource)
    {
        this.basePoints = basePoints;
        this.configuration = configuration;
        this.resultSource = resultSource ?? throw new ArgumentNullException(nameof(resultSource));
        calcPointsFunc = configuration.IsFinal ? CalcFinalPoints : CalcNormalPoints;
    }

    public List<TeamResult> CalcScoreBoard(IEnumerable<ParticipantResult> participants)
    {
        var participantPoints = GetParticipantPoints(participants);

        var teamResults = participantPoints
            .Where(pr => pr.Points >= 0)
            .GroupBy(pr => pr.Club)
            .Select(g => (
                Club: g.Key, 
                Points: g.Sum(d => d.Points), 
                IsPreliminary: g.Max(d => d.Status == Preliminary), 
                Statistics: Statistics.GetStatistics(g.Select(pr => new ParticipantResult(g.Key, "", pr.Club, pr.StartTime, pr.Time, pr.Status)), resultSource.CurrentTimeOfDay, configuration)))
            .ToDictionary(pr => pr.Club, pr => pr);

        var pos = 1;
        var reportPos = 1;
        var prevPoints = 0;
        int upTeamPoints = -1;
        var orderedResults = MergeWithBasePoints(teamResults, basePoints)
            .OrderByDescending(kp => kp.Points)
            .Select(kp =>
            {
                var isSamePos = kp.Points == prevPoints;

                if (upTeamPoints == -1) upTeamPoints = kp.Points;
                else if (!isSamePos) upTeamPoints = prevPoints;
                if (!isSamePos) reportPos = pos;
                prevPoints = kp.Points;
                pos++;
                TeamResult teamResult = new(reportPos, kp.Club, kp.Points, kp.IsPreliminary, upTeamPoints - kp.Points, kp.BasePoints, kp.Statistics);
                return teamResult;
            })
            .ToList();

        return orderedResults;
    }

    public IList<ParticipantPoints> GetParticipantPoints(IEnumerable<ParticipantResult> participants)   
    {
        var participantsWithExtras = participants.Select(pr => new PointsCalcParticipantResult(pr)).ToList();

        participantsWithExtras
            .Where(pr => pr.Status is Preliminary or Passed)
            .GroupBy(pr => new { pr.Class, pr.Club, pr.StartTime })
            .Where(g => g.Count() > 1)
            .SelectMany(patrol => patrol.OrderBy(pp => pp.Time).ToArray()[1..])
            .ForEach(pr => pr.IsExtraParticipant = true);

        var leaderByClass = participantsWithExtras
            .Where(pr => pr.Status is Preliminary or Passed && !pr.IsExtraParticipant)
            .GroupBy(pr => pr.Class)
            .Select(g => new { Class = g.Key, Time = g.Min(d => d.Time) })
            .ToImmutableDictionary(g => g.Class, g => g.Time);

        var participantPoints = participantsWithExtras
            .Select(pr => new ParticipantPoints(pr.Class, pr.Name, pr.Club, pr.StartTime, pr.Time, pr.Status, pr.IsExtraParticipant, calcPointsFunc(pr, leaderByClass.GetValueOrDefault(pr.Class)))) 
            .ToList();
        return participantPoints;
    }

    internal static int CalcNormalPoints(PointsCalcParticipantResult pr, TimeSpan? bestTime)
    {
        if (pr.Status <= Ignored) return -1;
        if (pr.Status == NotStarted) return 0;
        if (pr.Status == NotActivated) return 0;
        // TODO: Started om Activated och starttiden har passerats
        if (pr.Status == Activated) return 0;
        if (pr.Status == Started) return 0;

        var pointsTemplate = PointsTemplate.Get(pr.Class);
        if (pr.Status == NotValid) return pointsTemplate.NotPassedPoints;

        if (pr.Status != Passed && pr.Status != Preliminary)
            throw new InvalidOperationException($"Unexpected status: {pr.Status}");
        if (!bestTime.HasValue || !pr.Time.HasValue)
            throw new InvalidOperationException("Unexpected null time");
        var points = pointsTemplate.BasePoints
                     - pointsTemplate.MinuteReduction * StartedMinutesAfter(bestTime.Value, pr.Time.Value)
                     - (pr.IsExtraParticipant ? pointsTemplate.PatrolExtraParticipantsReduction : 0);

        return Math.Max(points, pointsTemplate.MinPoints);
    }

    internal static int CalcFinalPoints(PointsCalcParticipantResult pr, TimeSpan? bestTime)
    {
        switch (pr.Status)
        {
            case <= Ignored:
                return -1;
            case NotStarted:
            case NotActivated:
            // TODO: Started om Activated och starttiden har passerats
            case Activated:
            case Started:
                return 0;
        }

        var pointsTemplate = PointsTemplate.Get(pr.Class);

        if (pr.Status == NotValid) return pointsTemplate.NotPassedPoints;

        if (pr.Status != Passed && pr.Status != Preliminary)
            throw new InvalidOperationException($"Unexpected status: {pr.Status}");
        if (!bestTime.HasValue || !pr.Time.HasValue)
            throw new InvalidOperationException("Unexpected null time");

        if (pr.Time.Value <= pointsTemplate.FinalFullPointsTime) return pointsTemplate.FinalFullPoints;

        if (pr.IsExtraParticipant) return pointsTemplate.FinalMinPoints;

        int v = CalcFinalPoints(pointsTemplate, pr.Time.Value);
        return v;
    }

    private static int CalcFinalPoints(PointsTemplate pointsTemplate, TimeSpan time)
    {
        var points = pointsTemplate.FinalFullPoints
                     - (int)Math.Ceiling((time.TotalMilliseconds - pointsTemplate.FinalFullPointsTime.TotalMilliseconds) / pointsTemplate.FinalReductionTime.TotalMilliseconds);

        int v = Math.Max(points, pointsTemplate.FinalMinPoints);
        return v;
    }

    private static int StartedMinutesAfter(TimeSpan bestTime, TimeSpan time)
    {
        var secondsAfter = (time - bestTime).TotalSeconds;
        return (int)Math.Truncate((secondsAfter + 59) / 60.0);
    }

    private static IEnumerable<(string Club, int Points, bool IsPreliminary, int BasePoints, Statistics Statistics)> MergeWithBasePoints(IDictionary<string, (string Club, int Points, bool IsPreliminary, Statistics Statistics)> participantResults, IDictionary<string, int> basePointsDictionary)
    {
        var allClubs = participantResults.Keys.Union(basePointsDictionary.Keys);

        var merged = new List<(string Club, int Points, bool IsPreliminary, int BasePoints, Statistics Statistics)>();
        foreach (string club in allClubs)
        {
            int points = 0;
            var isPreliminary = false;
            Statistics? statistics = null;
            if (participantResults.TryGetValue(club, out (string Club, int Points, bool IsPreliminary, Statistics Statistics) result))
            {
                points = result.Points;
                isPreliminary = result.IsPreliminary;
                statistics = result.Statistics;
            }
            var basePoints = basePointsDictionary.TryGetValue(club, out int value) ? value : 0;

            merged.Add((club, basePoints + points, isPreliminary, basePoints, statistics ?? new Statistics()));
        }
        return merged;
    }

}

internal class PointsCalcParticipantResult : ParticipantResult
{
    public bool IsExtraParticipant { get; internal set; }

    public PointsCalcParticipantResult(string @class, string name, string club, TimeSpan? startTime, TimeSpan? time, ParticipantStatus status) 
        : base(@class, name, club, startTime, time, status)
    {
    }

    public PointsCalcParticipantResult(ParticipantResult participantResult) 
        : base(participantResult.Class, participantResult.Name, participantResult.Club, participantResult.StartTime, participantResult.Time, participantResult.Status)
    {
    }
}

internal class PointsTemplate
{
    public int BasePoints { get; }
    public int MinuteReduction { get; }
    public int MinPoints { get; }
    public int NotPassedPoints { get; }
    public int PatrolExtraParticipantsReduction { get; }
    public int FinalFullPoints { get; }
    public int FinalMinPoints { get; }
    public TimeSpan FinalFullPointsTime { get; }
    public TimeSpan FinalReductionTime { get; }

    private static readonly PointsTemplate DhTemplate       = new(50, 2, 15, 5,  0, 100, 20, TimeSpan.FromMinutes(12), TimeSpan.FromSeconds(7.5));
    private static readonly PointsTemplate UTemplate        = new(40, 2, 10, 5, 10,  80, 20, TimeSpan.FromMinutes(12), TimeSpan.FromSeconds(7.5));
    private static readonly PointsTemplate InskTemplate     = new(10, 0, 10, 5,  0,  20, 20, TimeSpan.MaxValue, TimeSpan.Zero);
    private static readonly PointsTemplate UnknownTemplate  = new( 0, 0,  0, 0,  0,   0,  0, TimeSpan.Zero, TimeSpan.Zero);

    private PointsTemplate(int basePoints, int minuteReduction, int minPoints, int notPassedPoints, int patrolExtraParticipantsReduction, 
        int finalFullPoints, int finalMinPoints, TimeSpan finalFullPointsTime, TimeSpan finalReductionTime)
    {
        BasePoints = basePoints;
        MinuteReduction = minuteReduction;
        MinPoints = minPoints;
        NotPassedPoints = notPassedPoints;
        PatrolExtraParticipantsReduction = patrolExtraParticipantsReduction;
        FinalFullPoints = finalFullPoints;
        FinalMinPoints = finalMinPoints;
        FinalFullPointsTime = finalFullPointsTime;
        FinalReductionTime = finalReductionTime;
    }

    public static PointsTemplate Get(string @class)
    {
        return @class[0] switch
        {
            'D' => DhTemplate,
            'H' => DhTemplate,
            'U' => UTemplate,
            'I' => InskTemplate,
            // ReSharper disable once LocalizableElement
            _ => UnknownTemplate
        };
    }
}

internal static class Helper
{
    public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
    {
        foreach (var item in items)
        {
            action(item);
        }
    }
}
