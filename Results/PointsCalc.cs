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
#pragma warning disable CA1822 // Mark members as static
    public List<TeamResult> CalcScoreBoard(IList<ParticipantResult> participant)
#pragma warning restore CA1822 // Mark members as static
    {
        var leaderByClass = participant
            .Where(d => new[] { Preliminary, Passed }.Contains(d.Status))
            .GroupBy(d => d.Class)
            .Select(g => new { Class = g.Key, Time = g.Min(d => d.Time) })
            .ToImmutableDictionary(g => g.Class, g => g.Time);

        var pos = 1;
        var reportPos = 1;
        var prevPoints = 0;
        return participant
            .Select(pr => new
            {
                pr.Club,
                Points = CalcPoints(pr, leaderByClass.GetValueOrDefault(pr.Class)),
                IsPreliminary = pr.Status == Preliminary
            })
            .Where(pr => pr.Points >= 0)
            .GroupBy(pr => pr.Club)
            .Select(g => new { Club = g.Key, Points = g.Sum(d => d.Points), IsPreliminary = g.Max(d => d.IsPreliminary) })
            .OrderByDescending(kp => kp.Points)
            .Select(kp =>
            {
                if (kp.Points != prevPoints) reportPos = pos;
                prevPoints = kp.Points;
                pos++;
                return new TeamResult(reportPos, kp.Club, kp.Points, kp.IsPreliminary);
            })
            .ToList();
    }

    internal static int CalcPoints(ParticipantResult pr, TimeSpan? bestTime)
    {
        if (pr.Status <= Ignored) return -1;
        if (pr.Status == NotStarted) return 0;
        if (pr.Status == NotActivated) return 0;

        var pointsTemplate = PointsTemplate.Get(pr.Class);

        // TODO: Started om Activated och starttiden har passerats
        if (pr.Status == Activated) return pointsTemplate.NotPassedPoints;
        if (pr.Status == Started) return pointsTemplate.NotPassedPoints;
        if (pr.Status == NotValid) return pointsTemplate.NotPassedPoints;

        if (pr.Status != Passed && pr.Status != Preliminary)
            throw new InvalidOperationException($"Unexpected status: {pr.Status}");
        if (!bestTime.HasValue || !pr.Time.HasValue) 
            throw new InvalidOperationException($"Unexpected null time");
        var points = pointsTemplate.BasePoints 
            - pointsTemplate.MinuteReduction * StartedMinutesAfter(bestTime.Value, pr.Time.Value) 
            - (pr.IsExtraParticipant ? pointsTemplate.PatrolExtraPaticipantsReduction : 0);

        return Math.Max(points, pointsTemplate.MinPoints);
    }

    private static int StartedMinutesAfter(TimeSpan bestTime, TimeSpan time)
    {
        var secondsAfter = (time - bestTime).TotalSeconds;
        return (int)Math.Truncate((secondsAfter + 59) / 60.0);
    }
}

internal class PointsTemplate
{
    public int BasePoints { get; }
    public int MinuteReduction { get; }
    public int MinPoints { get; }
    public int NotPassedPoints { get; }
    public int PatrolExtraPaticipantsReduction { get; }

    private static readonly PointsTemplate DhTemplate = new(50, 2, 15, 5);
    private static readonly PointsTemplate UTemplate = new(40, 2, 10, 5, 10);
    private static readonly PointsTemplate InskTemplate = new(10, 0, 10, 5);
    private static readonly PointsTemplate UnknownTemplate = new(0, 0, 0, 0);

    private PointsTemplate(int basePoints, int minuteReduction, int minPoints, int notPassedPoints, int patrolExtraPaticipantsReduction = 0)
    {
        BasePoints = basePoints;
        MinuteReduction = minuteReduction;
        MinPoints = minPoints;
        NotPassedPoints = notPassedPoints;
        PatrolExtraPaticipantsReduction = patrolExtraPaticipantsReduction;
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