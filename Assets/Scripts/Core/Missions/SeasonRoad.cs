using System;
using Sheytoonak.Core.Bags;

namespace Sheytoonak.Core.Missions
{
    /// <summary>
    /// «طريق الموسم», the free row only (docs/META_SCREENS.fa.md #2; the golden row is bought with real money and waits for
    /// the server, docs/LAUNCH.fa.md). A season is a calendar month of the Tehran date, so it needs no server. Points come
    /// from duels (a win 3, any other duel 1) and from each daily job taken (5); every 10 points is a step, 30 steps a
    /// season. A step's prize is taken once, in order; a new month starts from zero and what was not taken is lost.
    /// ⚠️ The numbers are a first guess, to be tuned after testing.
    /// </summary>
    public sealed class SeasonRoad
    {
        public const int Steps = 30, PointsPerStep = 10;
        public const int WinPoints = 3, DuelPoints = 1, MissionPoints = 5;

        public static string Key(DateTime day) => day.ToString("yyyyMM");
        /// <summary>Whole days left of the season, the last day counts as 1.</summary>
        public static int DaysLeft(DateTime day) => DateTime.DaysInMonth(day.Year, day.Month) - day.Day + 1;

        public static MapPrize PrizeOf(int step)
        {
            if (step >= Steps) return new MapPrize { Kind = MapPrizeKind.Bag, Bag = BagSize.Huge };
            if (step % 10 == 0) return new MapPrize { Kind = MapPrizeKind.Bag, Bag = BagSize.Large };
            if (step % 5 == 0) return new MapPrize { Kind = MapPrizeKind.Bag, Bag = BagSize.Medium };
            if (step % 2 == 0) return new MapPrize { Kind = MapPrizeKind.Marbles, Amount = 5 };
            return new MapPrize { Kind = MapPrizeKind.Shards, Amount = 100 };
        }

        public string SeasonKey { get; private set; } = "";
        int points, taken;

        void Roll(DateTime day)
        {
            string k = Key(day);
            if (k == SeasonKey) return;
            SeasonKey = k; points = 0; taken = 0;
        }

        public int Points(DateTime day) { Roll(day); return points; }
        public int StepsReached(DateTime day) { Roll(day); return Math.Min(Steps, points / PointsPerStep); }
        public int Taken(DateTime day) { Roll(day); return taken; }
        public int Waiting(DateTime day) => StepsReached(day) - Taken(day);
        /// <summary>Points inside the step being walked (0..9); 0 once the road is finished.</summary>
        public int PointsInStep(DateTime day) => StepsReached(day) >= Steps ? 0 : points % PointsPerStep;

        public void Add(int n, DateTime day) { Roll(day); if (n > 0) points = Math.Min(Steps * PointsPerStep, points + n); }

        /// <summary>Takes the next reached step's prize; null when nothing waits.</summary>
        public MapPrize? TakeNext(DateTime day)
        {
            if (Waiting(day) <= 0) return null;
            taken++;
            return PrizeOf(taken);
        }

        public string Serialize() => string.Join(",", SeasonKey, points, taken);

        public static SeasonRoad Parse(string s)
        {
            var r = new SeasonRoad();
            if (string.IsNullOrEmpty(s)) return r;
            var p = s.Split(',');
            if (p.Length != 3) return r;
            r.SeasonKey = p[0];
            int.TryParse(p[1], out r.points); int.TryParse(p[2], out r.taken);
            r.points = Math.Max(0, Math.Min(Steps * PointsPerStep, r.points));
            r.taken = Math.Max(0, Math.Min(r.points / PointsPerStep, r.taken));
            return r;
        }
    }
}
