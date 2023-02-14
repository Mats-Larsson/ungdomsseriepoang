using Results.MySql;
using System.Collections.Immutable;

namespace Results
{
// Kretstävlingar och regiontävlingen
// 50 poäng till segraren i respektive huvudklass oavsett tid, därefter 2 poängs avdrag per påbörjad minut efter segrartiden.

// 40 poäng till segraren i respektive U-klass oavsett tid, därefter 2 poängs avdrag per påbörjad minut efter segrartiden.

// Vid patrull i U-klass får patrullen 40 poäng för löpare nr1 och 30 poäng för extralöparna.I övrigt som ovan.

// 10 poäng för fullföljande av Inskolningsklass.I resultaten för Inskolning ska det stå ”fullföljt” och ingenting annat.

// Minipoäng i huvudklass är 15 poäng och i U-klass 10 poäng.Ej fullföljd tävling ger 5 poäng.OBS! Endast segraren får full poäng!

//Exempel på poängberäkning
//Segrartiden i D10 är 17.45. Segraren får då 50 poäng.Alla som har tider mellan 17.46-18.45 får 48 poäng.Alla som har tider mellan 18.46-19.45 får 46 poäng osv.

    internal class PointsCalc
    {
        public List<ClubResult> CalcScoreBoard(IList<ParticipantResult> participant)
        {
            var leader = participant
                .Where(d => d.Status == ParticipantStatus.Passed)
                .GroupBy(d => d.Class)
                .Select(g => new { Klass = g.Key, Tid = g.Min(d => d.Time) })
                .ToImmutableDictionary(g => g.Klass, g => g.Tid);

            var pos = 1;
            var reportPos = 1;
            var prevPoints = 0;
            return participant
            .Select(d => new { d.Club, Points = CalcPoints(d.Class, d.Time, d.Status, leader.GetValueOrDefault(d.Class)) })
            .Where(d => d.Points >= 0)
            .GroupBy(d => d.Club)
            .Select(g => new { Club = g.Key, Points = g.Sum(d => d.Points) })
            .OrderByDescending(kp => kp.Points)
            .Select(kp =>
            {
                if (kp.Points != prevPoints) reportPos = pos;
                prevPoints = kp.Points;
                pos++;
                return new ClubResult(reportPos, kp.Club, kp.Points);
            })
            .ToList();
        }

        internal int CalcPoints(string @class, TimeSpan? time, ParticipantStatus status, TimeSpan? bestTime)
        {
            if (status <= ParticipantStatus.Ignored) return -1;
            if (status <= ParticipantStatus.NotStarted) return 0;

            var pointsTemplate = PointsTemplate.Get(@class);

            if (status == ParticipantStatus.Started) return pointsTemplate.NotPassedPoints;

            if (status != ParticipantStatus.Passed) throw new InvalidOperationException($"Unexpected status: {status}");
            if (!bestTime.HasValue || !time.HasValue) throw new InvalidOperationException($"Unexpected null time");
            var points = pointsTemplate.BasePoints - pointsTemplate.MinuteReduction * StartedMinutesAfter(bestTime.Value, time.Value);

            return Math.Max(points, pointsTemplate.MinePoints);
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
        public int MinePoints { get; }
        public int NotPassedPoints { get; }

        private static readonly PointsTemplate DhTemplate = new(50, 2, 15, 5);
        private static readonly PointsTemplate UTemplate  = new(40, 2, 10, 5);
        private static readonly PointsTemplate InskTemplate  = new(10, 0, 10, 5);

        private PointsTemplate(int basePoints, int minuteReduction, int minePoints, int notPassedPoints)
        {
            BasePoints = basePoints;
            MinuteReduction = minuteReduction;
            MinePoints = minePoints;
            NotPassedPoints = notPassedPoints;
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
                _ => throw new ArgumentOutOfRangeException(nameof(@class), $"Unknown: {@class}")
            };
        }
    }
}
