using Results.Contract;
using Results.Model;
using System.Collections.Immutable;
using static Results.Model.ParticipantStatus;

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
    private readonly Func<PointsCalcParticipantResult, TimeSpan?, int> calcPointsFunc;

    public PointsCalc(IDictionary<string, int> basePoints, bool isFinal)
    {
        this.basePoints = basePoints;
        calcPointsFunc = isFinal ? CalcFinalPoints : CalcNormalPoints;
    }

    public List<TeamResult> CalcScoreBoard(IEnumerable<ParticipantResult> participants)
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

        var pos = 1;
        var reportPos = 1;
        var prevPoints = 0;
        var participantResults = participantsWithExtras
            .Select(pr => (pr.Club, Points: calcPointsFunc(pr, leaderByClass.GetValueOrDefault(pr.Class)), IsPreliminary: pr.Status == Preliminary))
            .Where(pr => pr.Points >= 0)
            .GroupBy(pr => pr.Club)
            .Select(g => (Club: g.Key, Points: g.Sum(d => d.Points), IsPreliminary: g.Max(d => d.IsPreliminary)))
            .ToDictionary(pr => pr.Club, pr => pr);

        var orderedResults = MergeWithBasePoints(participantResults, basePoints)
            .OrderByDescending(kp => kp.Points)
            .Select(kp =>
            {
                if (kp.Points != prevPoints) reportPos = pos;
                prevPoints = kp.Points;
                pos++;
                return new TeamResult(reportPos, kp.Club, kp.Points, kp.IsPreliminary);
            })
            .ToList();

        return orderedResults;
    }

    internal static int CalcNormalPoints(PointsCalcParticipantResult pr, TimeSpan? bestTime)
    {
        if (pr.Status <= Ignored) return -1;
        if (pr.Status == NotStarted) return 0;
        if (pr.Status == NotActivated) return 0;
        // TODO: Started om Activated och starttiden har passerats
        if (pr.Status == Activated) return 0;
        if (pr.Status == Started) return 0;
        if (pr.Status == NotValid) return 0;

        var pointsTemplate = PointsTemplate.Get(pr.Class);

        if (pr.Status != Passed && pr.Status != Preliminary)
            throw new InvalidOperationException($"Unexpected status: {pr.Status}");
        if (!bestTime.HasValue || !pr.Time.HasValue)
            throw new InvalidOperationException($"Unexpected null time");
        var points = pointsTemplate.BasePoints
                     - pointsTemplate.MinuteReduction * StartedMinutesAfter(bestTime.Value, pr.Time.Value)
                     - (pr.IsExtraParticipant ? pointsTemplate.PatrolExtraPaticipantsReduction : 0);

        return Math.Max(points, pointsTemplate.MinPoints);
    }

    internal static int CalcFinalPoints(PointsCalcParticipantResult pr, TimeSpan? bestTime)
    {
        if (pr.Status <= Ignored) return -1;
        if (pr.Status == NotStarted) return 0;
        if (pr.Status == NotActivated) return 0;
        // TODO: Started om Activated och starttiden har passerats
        if (pr.Status == Activated) return 0;
        if (pr.Status == Started) return 0;
        if (pr.Status == NotValid) return 0;

        var pointsTemplate = PointsTemplate.Get(pr.Class);

        if (pr.Status != Passed && pr.Status != Preliminary)
            throw new InvalidOperationException($"Unexpected status: {pr.Status}");
        if (!bestTime.HasValue || !pr.Time.HasValue)
            throw new InvalidOperationException($"Unexpected null time");

        if (!pointsTemplate.FinalFullPointsTime.HasValue) return pointsTemplate.FinalFullPoints;

        if (pr.Time.Value <= pointsTemplate.FinalFullPointsTime.Value) return pointsTemplate.FinalFullPoints;

        if (pr.IsExtraParticipant) return pointsTemplate.FinalMinPoints;

        var points = pointsTemplate.FinalFullPoints
                     - (int)Math.Ceiling((pr.Time.Value - pointsTemplate.FinalFullPointsTime.Value) / pointsTemplate.FinalReductionTime);

        return Math.Max(points, pointsTemplate.FinalMinPoints);
    }

    private static int StartedMinutesAfter(TimeSpan bestTime, TimeSpan time)
    {
        var secondsAfter = (time - bestTime).TotalSeconds;
        return (int)Math.Truncate((secondsAfter + 59) / 60.0);
    }

    private static IEnumerable<(string Club, int Points, bool IsPreliminary)> MergeWithBasePoints(IDictionary<string, (string Club, int Points, bool IsPreliminary)> participantResults, IDictionary<string, int> basePointsDictionary)
    {
        var allClubs = participantResults.Keys.Union(basePointsDictionary.Keys);

        var merged = new List<(string Club, int Points, bool IsPreliminary)>();
        foreach (string club in allClubs)
        {
            var points = participantResults.ContainsKey(club) ? participantResults[club].Points : 0;
            var isPreliminary = participantResults.ContainsKey(club) && participantResults[club].IsPreliminary;
            var basePoints = basePointsDictionary.ContainsKey(club) ? basePointsDictionary[club] : 0;

            merged.Add((club, basePoints + points, isPreliminary));
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
    public int PatrolExtraPaticipantsReduction { get; }
    public int FinalFullPoints { get; }
    public int FinalMinPoints { get; }
    public TimeSpan? FinalFullPointsTime { get; }
    public TimeSpan FinalReductionTime { get; }

    private static readonly PointsTemplate DhTemplate       = new(50, 2, 15, 5,  0, 100, 20, TimeSpan.FromMinutes(12), TimeSpan.FromSeconds(6));
    private static readonly PointsTemplate UTemplate        = new(40, 2, 10, 5, 10,  80, 20, TimeSpan.FromMinutes(12), TimeSpan.FromSeconds(6));
    private static readonly PointsTemplate InskTemplate     = new(10, 0, 10, 5,  0,  20, 20, null, TimeSpan.Zero);
    private static readonly PointsTemplate UnknownTemplate  = new( 0, 0,  0, 0,  0,   0,  0, null, TimeSpan.Zero);

    private PointsTemplate(int basePoints, int minuteReduction, int minPoints, int notPassedPoints, int patrolExtraPaticipantsReduction, 
        int finalFullPoints, int finalMinPoints, TimeSpan? finalFullPointsTime, TimeSpan finalReductionTime)
    {
        BasePoints = basePoints;
        MinuteReduction = minuteReduction;
        MinPoints = minPoints;
        NotPassedPoints = notPassedPoints;
        PatrolExtraPaticipantsReduction = patrolExtraPaticipantsReduction;
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
