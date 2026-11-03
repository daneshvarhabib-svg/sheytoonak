using System;

namespace Sheytoonak.Core.Challenge
{
    /// <summary>
    /// The 12-wins / 3-losses challenge (docs/SCENARIO.fa.md §10): paid entry in glass shards, heroes dealt at random
    /// from one shared pool with equal tools (money does not help), every win raises the prize one step, win 7 pays the
    /// entry back, more is profit. A run ends at 12 wins, 3 losses, or when its time (48 h) runs out; the prize for the
    /// wins so far is paid then. ⚠️ The prize table is a placeholder: the scenario says the numbers come after testing.
    /// </summary>
    public sealed class ChallengeRun
    {
        public const int MaxWins = 12, MaxLosses = 3;
        public const int DefaultEntry = 100;
        public static readonly TimeSpan Duration = TimeSpan.FromHours(48);

        /// <summary>Prize as a multiple of the entry after N wins (index = wins). Win 7 = exactly the entry.</summary>
        public static readonly double[] PrizeTable = { 0, 0.1, 0.2, 0.35, 0.5, 0.65, 0.8, 1.0, 1.3, 1.7, 2.2, 2.8, 3.5 };

        public int Entry { get; private set; }
        public int Wins { get; private set; }
        public int Losses { get; private set; }
        public DateTime StartedUtc { get; private set; }
        public bool Collected { get; private set; }

        public ChallengeRun(int entry, DateTime startedUtc)
        {
            if (entry < 0) throw new ArgumentOutOfRangeException(nameof(entry));
            Entry = entry; StartedUtc = startedUtc;
        }

        public DateTime EndsUtc => StartedUtc + Duration;
        public bool Expired(DateTime nowUtc) => nowUtc >= EndsUtc;
        public bool IsOver(DateTime nowUtc) => Wins >= MaxWins || Losses >= MaxLosses || Expired(nowUtc);

        public static int PrizeFor(int entry, int wins) => (int)Math.Round(entry * PrizeTable[Math.Max(0, Math.Min(MaxWins, wins))]);
        public int PrizeNow => PrizeFor(Entry, Wins);
        public int PrizeNext => PrizeFor(Entry, Math.Min(MaxWins, Wins + 1));

        /// <summary>Counts one duel. Ignored once the run is over (a duel that ends after the deadline does not count).</summary>
        public bool Record(bool won, DateTime nowUtc)
        {
            if (IsOver(nowUtc) || Collected) return false;
            if (won) Wins++; else Losses++;
            return true;
        }

        /// <summary>Pays the prize once, only when the run is over. Returns the shards to add (0 if not yet / already paid).</summary>
        public int Collect(DateTime nowUtc)
        {
            if (Collected || !IsOver(nowUtc)) return 0;
            Collected = true;
            return PrizeNow;
        }

        public string Serialize() => string.Join(",", Entry, Wins, Losses, StartedUtc.Ticks, Collected ? 1 : 0);

        public static ChallengeRun Parse(string s)
        {
            if (string.IsNullOrEmpty(s)) return null;
            var p = s.Split(',');
            if (p.Length != 5) return null;
            try
            {
                var r = new ChallengeRun(int.Parse(p[0]), new DateTime(long.Parse(p[3]), DateTimeKind.Utc))
                {
                    Wins = Math.Min(MaxWins, Math.Max(0, int.Parse(p[1]))),
                    Losses = Math.Min(MaxLosses, Math.Max(0, int.Parse(p[2]))),
                    Collected = p[4] == "1"
                };
                return r;
            }
            catch (FormatException) { return null; }
            catch (OverflowException) { return null; }
        }
    }
}
