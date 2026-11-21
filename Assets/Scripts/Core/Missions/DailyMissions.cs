using System;
using System.Collections.Generic;

namespace Sheytoonak.Core.Missions
{
    public enum MissionKind { PlayDuels, WinDuels, BreakWindows, UsePoisons, UseAntidotes, OpenBags }

    public struct Mission { public MissionKind Kind; public int Goal; public int Shards; }

    /// <summary>
    /// «مهمّات اليوم» (user 2026-11-19: «بریم سراغ کارهایی که بی‌سرور می‌شه انجامش داد»; docs/META_SCREENS.fa.md #3). Three small
    /// jobs a day, the same for every player (from the Tehran date, no server, like <see cref="Sheytoonak.Core.Duel.DailyCombo"/>).
    /// Each pays shards once; all three pay a few marbles on top. Everything is counted from what the player does in duels
    /// and with bags, on the phone. ⚠️ The numbers are a first guess, to be tuned after testing.
    /// </summary>
    public sealed class DailyMissions
    {
        public const int Count = 3;
        /// <summary>Marbles for finishing all three (a marble is 20 shards).</summary>
        public const int BonusMarbles = 5;

        static readonly Mission[] Table =
        {
            new Mission { Kind = MissionKind.PlayDuels,    Goal = 3,  Shards = 80 },
            new Mission { Kind = MissionKind.WinDuels,     Goal = 2,  Shards = 150 },
            new Mission { Kind = MissionKind.BreakWindows, Goal = 12, Shards = 120 },
            new Mission { Kind = MissionKind.UsePoisons,   Goal = 6,  Shards = 100 },
            new Mission { Kind = MissionKind.UseAntidotes, Goal = 3,  Shards = 100 },
            new Mission { Kind = MissionKind.OpenBags,     Goal = 1,  Shards = 60 },
        };

        /// <summary>The three jobs of that day: distinct kinds, stable for the same day.</summary>
        public static Mission[] For(DateTime day)
        {
            var all = new List<Mission>(Table);
            var rng = new Random((day.Year * 10000 + day.Month * 100 + day.Day) * 6151 + 5);
            for (int i = all.Count - 1; i > 0; i--) { int j = rng.Next(i + 1); (all[i], all[j]) = (all[j], all[i]); }
            return all.GetRange(0, Count).ToArray();
        }

        public static string Key(DateTime day) => day.ToString("yyyyMMdd");

        // «بدّل المهمّة» (research p53, user 2026-11-21: «۵۳ رو بساز / اگر خواست ماموریت های بیشتر تغییر بده باید ۵ تا
        // تیله بسوزونه»): an unfinished job can be traded for one of the three kinds not on today's list. The first
        // trade of the day is free, every later one burns SwapMarbles. The trade is kept for the day (a table index
        // per slot, -1 = the day's own job) and the traded slot starts from zero.
        public const int SwapMarbles = 5;
        readonly int[] swapped = { -1, -1, -1 };
        public int Swaps { get; private set; }
        public int SwapCost(DateTime day) { Roll(day); return Swaps == 0 ? 0 : SwapMarbles; }
        public bool CanSwap(int i, DateTime day) => i >= 0 && i < Count && !Done(i, day) && !claimed[i];

        /// <summary>Today's three jobs as this player has them (the day's list with the trades applied).</summary>
        public Mission[] Jobs(DateTime day)
        {
            Roll(day);
            var m = For(day);
            for (int i = 0; i < Count; i++) if (swapped[i] >= 0 && swapped[i] < Table.Length) m[i] = Table[swapped[i]];
            return m;
        }

        /// <summary>Trades job i for a kind not on the list now. False when it is finished or taken (the caller pays).</summary>
        public bool Swap(int i, DateTime day)
        {
            if (!CanSwap(i, day)) return false;
            var now = Jobs(day);
            var free = new List<int>();
            for (int t = 0; t < Table.Length; t++)
            {
                bool used = false;
                foreach (var j in now) if (j.Kind == Table[t].Kind) used = true;
                if (!used) free.Add(t);
            }
            if (free.Count == 0) return false;
            var rng = new Random((day.Year * 10000 + day.Month * 100 + day.Day) * 31 + Swaps * 7 + i);
            swapped[i] = free[rng.Next(free.Count)];
            progress[i] = 0; Swaps++;
            return true;
        }

        public string DayKey { get; private set; } = "";
        readonly int[] progress = new int[Count];
        readonly bool[] claimed = new bool[Count];
        public bool BonusClaimed { get; private set; }

        /// <summary>A new day wipes yesterday's progress (unclaimed prizes are lost with it).</summary>
        public void Roll(DateTime day)
        {
            string k = Key(day);
            if (k == DayKey) return;
            DayKey = k; BonusClaimed = false;
            Swaps = 0;
            for (int i = 0; i < Count; i++) { progress[i] = 0; claimed[i] = false; swapped[i] = -1; }
        }

        public int Progress(int i, DateTime day) { Roll(day); return Math.Min(progress[i], Jobs(day)[i].Goal); }
        public bool Done(int i, DateTime day) { Roll(day); return progress[i] >= Jobs(day)[i].Goal; }
        public bool Claimed(int i, DateTime day) { Roll(day); return claimed[i]; }
        public bool AllClaimed(DateTime day) { Roll(day); foreach (bool c in claimed) if (!c) return false; return true; }
        /// <summary>Something waits to be collected (the red dot on the home button).</summary>
        public bool AnyReady(DateTime day)
        {
            for (int i = 0; i < Count; i++) if (Done(i, day) && !claimed[i]) return true;
            return AllClaimed(day) && !BonusClaimed;
        }

        /// <summary>Counts n of a kind toward today's jobs. True when a job has just been finished by it.</summary>
        public bool Note(MissionKind kind, int n, DateTime day)
        {
            Roll(day);
            if (n <= 0) return false;
            var m = Jobs(day); bool finished = false;
            for (int i = 0; i < Count; i++)
            {
                if (m[i].Kind != kind || progress[i] >= m[i].Goal) continue;
                progress[i] = Math.Min(m[i].Goal, progress[i] + n);
                if (progress[i] >= m[i].Goal) finished = true;
            }
            return finished;
        }

        /// <summary>Shards of a finished job, once (0 when not finished or already taken).</summary>
        public int Claim(int i, DateTime day)
        {
            if (i < 0 || i >= Count || !Done(i, day) || claimed[i]) return 0;
            claimed[i] = true;
            return Jobs(day)[i].Shards;
        }

        /// <summary>Marbles for all three, once, after every job's own prize has been taken.</summary>
        public int ClaimBonus(DateTime day)
        {
            if (BonusClaimed || !AllClaimed(day)) return 0;
            BonusClaimed = true;
            return BonusMarbles;
        }

        public string Serialize()
        {
            var p = new List<string> { DayKey, BonusClaimed ? "1" : "0" };
            for (int i = 0; i < Count; i++) { p.Add(progress[i].ToString()); p.Add(claimed[i] ? "1" : "0"); }
            p.Add(Swaps.ToString());
            for (int i = 0; i < Count; i++) p.Add(swapped[i].ToString());
            return string.Join(",", p);
        }

        public static DailyMissions Parse(string s)
        {
            var d = new DailyMissions();
            if (string.IsNullOrEmpty(s)) return d;
            var p = s.Split(',');
            int old = 2 + Count * 2; // before the trades were added
            if (p.Length != old && p.Length != old + 1 + Count) return d;
            d.DayKey = p[0]; d.BonusClaimed = p[1] == "1";
            for (int i = 0; i < Count; i++)
            {
                int.TryParse(p[2 + i * 2], out d.progress[i]);
                if (d.progress[i] < 0) d.progress[i] = 0;
                d.claimed[i] = p[3 + i * 2] == "1";
            }
            if (p.Length > old)
            {
                int.TryParse(p[old], out int sw); d.Swaps = Math.Max(0, sw);
                for (int i = 0; i < Count; i++)
                    if (!int.TryParse(p[old + 1 + i], out d.swapped[i]) || d.swapped[i] >= Table.Length) d.swapped[i] = -1;
            }
            return d;
        }
    }
}
