using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Utilities;
using Results.MySql;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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

    internal class Poängberäkning
    {
        public List<KlubbResultat> BeräknaPoäng(IList<PersonResultat> deltagare)
        {
            var ledare = deltagare
                .Where(d => d.Status == PersonStatus.Godkänd)
                .GroupBy(d => d.Klass)
                .Select(g => new { Klass = g.Key, Tid = g.Min(d => d.Tid) })
                .ToImmutableDictionary(g => g.Klass, g => g.Tid);

            var pos = 1;
            return deltagare
            .Select(d => new { Klubb = d.Klubb, Poäng = Poäng(d.Klass, d.Tid, d.Status, ledare.GetValueOrDefault(d.Klass)) })
            .Where(d => d.Poäng >= 0)
            .GroupBy(d => d.Klubb)
            .Select(g => new { Klubb = g.Key, Poäng = g.Sum(d => d.Poäng) })
            .OrderByDescending(kp => kp.Poäng)
            .Select(kp => new KlubbResultat(pos++, kp.Klubb, kp.Poäng))
            .ToList();
        }

        private int Poäng(string klass, TimeSpan? tid, PersonStatus status, TimeSpan? bästaTid)
        {
            if (status <= PersonStatus.DeltarEj) return -1;
            if (status <= PersonStatus.EjStart) return 0;

            var poängMall = PoängMall.HämtaPoängMall(klass);

            if (status == PersonStatus.Aktiverad) return poängMall.EjFullföljdPoäng;

            if (status != PersonStatus.Godkänd) throw new InvalidOperationException($"Unexpected status: {status}");
            if (!bästaTid.HasValue || !tid.HasValue) throw new InvalidOperationException($"Unexpected null time");
            var poäng = poängMall.Grundpoäng - poängMall.Minutavdrag * PåbörjadeMinuterEfter(bästaTid.Value, tid.Value);

            return Math.Max(poäng, poängMall.Minpoäng);
        }

        private int PåbörjadeMinuterEfter(TimeSpan bästaTid, TimeSpan tid)
        {
            var sekunderEfter = (tid - bästaTid).TotalSeconds;
            return (int)Math.Truncate((sekunderEfter + 59) / 60.0);
        }
    }

    internal class PoängMall
    {
        public int Grundpoäng { get; }
        public int Minutavdrag { get; }
        public int Minpoäng { get; }
        public int EjFullföljdPoäng { get; }

        private static readonly PoängMall DHMall = new PoängMall(50, 2, 15, 5);
        private static readonly PoängMall UMall  = new PoängMall(40, 2, 10, 5);
        private static readonly PoängMall IMall  = new PoängMall(10, 0, 10, 5);

        private PoängMall(int grundpoäng, int minutavdrag, int minpoäng, int ejFullföljdPoäng)
        {
            Grundpoäng = grundpoäng;
            Minutavdrag = minutavdrag;
            Minpoäng = minpoäng;
            EjFullföljdPoäng = ejFullföljdPoäng;
        }

        public static PoängMall HämtaPoängMall(string klass)
        {
            switch (klass[0])
            {
                case 'D':
                case 'H': return DHMall;
                case 'U': return UMall;
                case 'I': return IMall;
            }
            throw new ArgumentOutOfRangeException(nameof(klass), $"Unknown: {klass}");
        }
    }
}
