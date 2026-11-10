using System;
using System.Collections.Generic;
using System.Text;

namespace Sheytoonak.Core.Bags
{
    public enum BagSize { Small = 0, Medium = 1, Large = 2, Huge = 3 }

    public sealed class Bag
    {
        public BagSize Size;
        public int CycleIndex;          // which step of the fixed cycle it came from (seeds its contents)
        public long UnlockStartTicks;   // 0 = not started
        public long ReadyTicks;         // when it can be opened
        public bool Unlocking => UnlockStartTicks != 0;
    }

    public struct BagContents
    {
        public int Shards;
        public string[] UpgradeCards; // latent upgrades for tools (activated later with shards, SCENARIO §8)
    }

    /// <summary>
    /// Garbage bags (docs/SCENARIO.fa.md §8): four slots, a full rack means no new bag until one is opened. A bag comes
    /// from a win (not from time), and one extra every 20 curses. Sizes follow a FIXED cycle of 40 wins (30 small,
    /// 8 medium, 2 large in fixed places, not pure RNG — the Paeezan lesson). One bag unlocks at a time; timers 2/6/12/18 h
    /// are divided by the first-week clock (×6 on day one, down to ×1 on day seven). Huge bags only come from clan wars.
    /// ⚠️ The shard amounts are placeholders until the economy is tested.
    /// </summary>
    public sealed class BagRules
    {
        public const int Slots = 4, CursesPerBag = 20;
        public static readonly int[] Hours = { 2, 6, 12, 18 };
        static readonly int[] ShardsMin = { 20, 60, 150, 300 }, ShardsMax = { 40, 110, 260, 500 };
        // User (2026-11-09, levels): level 2 of a tool inside a week at ~5 wins a day. 1/2/4/6 cards spread over 16 tools gave
        // one tool a card every ~30 wins; 4/8/16/24 give each tool ~1.7 cards a day (ToolLevelsTests keeps the calendar).
        static readonly int[] Cards = { 4, 8, 16, 24 };
        /// <summary>Cards per bag size, for the level calendar.</summary>
        public static int CardsIn(BagSize s) => Cards[(int)s];
        /// <summary>What a bag of this size holds, said up front (store rules on random rewards: docs/STORE_RISK.fa.md §2 #8).</summary>
        public static int ShardsMinIn(BagSize s) => ShardsMin[(int)s];
        public static int ShardsMaxIn(BagSize s) => ShardsMax[(int)s];

        /// <summary>40 steps: M at 5,10,15,20,25,30,35,39 (8), L at 19 and 38 (2), the rest S (30).</summary>
        public static readonly BagSize[] Cycle = BuildCycle();
        static BagSize[] BuildCycle()
        {
            var c = new BagSize[40];
            foreach (int i in new[] { 4, 9, 14, 21, 24, 29, 34, 39 }) c[i] = BagSize.Medium;
            c[19] = BagSize.Large; c[37] = BagSize.Large;
            return c;
        }

        public readonly Bag[] Rack = new Bag[Slots];
        public int CycleStep { get; private set; }
        public int CurseMilestone { get; private set; } // curses already turned into bags (multiple of 20)
        public DateTime InstallUtc { get; private set; }

        public BagRules(DateTime installUtc) { InstallUtc = installUtc; }

        /// <summary>×6 on the first day, ×1 from day seven, linear in between (only bag timers).</summary>
        public double ClockFactor(DateTime nowUtc)
        {
            double days = Math.Max(0, (nowUtc - InstallUtc).TotalDays);
            return Math.Max(1.0, 6.0 - 5.0 * Math.Min(1.0, days / 7.0));
        }

        public int FreeSlot() { for (int i = 0; i < Slots; i++) if (Rack[i] == null) return i; return -1; }

        /// <summary>A win puts the next bag of the cycle in the first free slot; a full rack gets nothing (and the cycle does not move).</summary>
        public int AddFromWin()
        {
            int slot = FreeSlot();
            if (slot < 0) return -1;
            Rack[slot] = new Bag { Size = Cycle[CycleStep % Cycle.Length], CycleIndex = CycleStep };
            CycleStep++;
            return slot;
        }

        /// <summary>Every 20 curses (rank) gives one extra small bag; returns how many were added now.</summary>
        public int AddFromCurses(int curses)
        {
            int added = 0;
            while (curses >= CurseMilestone + CursesPerBag)
            {
                int slot = FreeSlot();
                if (slot < 0) break; // stays owed until there is room
                CurseMilestone += CursesPerBag;
                Rack[slot] = new Bag { Size = BagSize.Small, CycleIndex = -1 - CurseMilestone / CursesPerBag };
                added++;
            }
            return added;
        }

        public bool AnyUnlocking(DateTime nowUtc) { foreach (var b in Rack) if (b != null && b.Unlocking && nowUtc.Ticks < b.ReadyTicks) return true; return false; }

        // ⚠️ The clock is back (user 2026-11-10): «زمان باز شدن هر کیسه نباید اینقدر کم باشه. بهتره حداقل ۱ ساعت طول بکشه».
        // Review 2026-11-07 §2.6 had replaced the timers with "the next win opens it", which opened a bag every few
        // minutes. Now 1 / 3 / 8 / 12 hours by size; the first-week clock still speeds the big ones up but never under
        // an hour (MinHours).
        public const double MinHours = 1.0;
        public static readonly double[] OpenHours = { 1, 3, 8, 12 };

        /// <summary>How long this bag takes to open if started now.</summary>
        public TimeSpan UnlockTime(BagSize size, DateTime nowUtc)
            => TimeSpan.FromHours(Math.Max(MinHours, OpenHours[(int)size] / ClockFactor(nowUtc)));

        public bool StartUnlock(int slot, DateTime nowUtc)
        {
            var b = Rack[slot];
            if (b == null || b.Unlocking || AnyUnlocking(nowUtc)) return false;
            b.UnlockStartTicks = nowUtc.Ticks;
            b.ReadyTicks = (nowUtc + UnlockTime(b.Size, nowUtc)).Ticks;
            return true;
        }

        /// <summary>Saves of the "next win opens it" builds: a bag still waiting for a win gets its real timer from now.</summary>
        public void FixWaitingForWin(DateTime nowUtc)
        {
            foreach (var b in Rack) if (b != null && b.Unlocking && b.ReadyTicks == long.MaxValue) b.ReadyTicks = (nowUtc + UnlockTime(b.Size, nowUtc)).Ticks;
        }

        public bool IsReady(int slot, DateTime nowUtc) => Rack[slot] != null && Rack[slot].Unlocking && nowUtc.Ticks >= Rack[slot].ReadyTicks;

        public TimeSpan TimeLeft(int slot, DateTime nowUtc)
            => Rack[slot] == null || !Rack[slot].Unlocking ? TimeSpan.Zero : TimeSpan.FromTicks(Math.Max(0, Rack[slot].ReadyTicks - nowUtc.Ticks));

        /// <summary>Opens a ready bag: contents are fixed by its size and cycle step (same bag = same loot, no reroll by reinstalling).</summary>
        public BagContents? Open(int slot, DateTime nowUtc, string[] tools)
        {
            if (!IsReady(slot, nowUtc)) return null;
            var b = Rack[slot];
            Rack[slot] = null;
            return ContentsOf(b, tools);
        }

        public static BagContents ContentsOf(Bag b, string[] tools)
        {
            var rng = new Random(unchecked(b.CycleIndex * 7919 + (int)b.Size * 104729 + 17));
            int s = (int)b.Size;
            var cards = new string[tools == null || tools.Length == 0 ? 0 : Cards[s]];
            for (int i = 0; i < cards.Length; i++) cards[i] = tools[rng.Next(tools.Length)];
            return new BagContents { Shards = rng.Next(ShardsMin[s], ShardsMax[s] + 1), UpgradeCards = cards };
        }

        // ------------------------------------------------------------------ save

        public string Serialize()
        {
            var sb = new StringBuilder();
            sb.Append(InstallUtc.Ticks).Append(';').Append(CycleStep).Append(';').Append(CurseMilestone);
            foreach (var b in Rack)
            {
                sb.Append(';');
                if (b != null) sb.Append((int)b.Size).Append(',').Append(b.CycleIndex).Append(',').Append(b.UnlockStartTicks).Append(',').Append(b.ReadyTicks);
            }
            return sb.ToString();
        }

        public static BagRules Parse(string s)
        {
            if (string.IsNullOrEmpty(s)) return null;
            var p = s.Split(';');
            if (p.Length != 3 + Slots) return null;
            try
            {
                var r = new BagRules(new DateTime(long.Parse(p[0]), DateTimeKind.Utc)) { CycleStep = int.Parse(p[1]), CurseMilestone = int.Parse(p[2]) };
                for (int i = 0; i < Slots; i++)
                {
                    if (p[3 + i].Length == 0) continue;
                    var q = p[3 + i].Split(',');
                    if (q.Length != 4) return null;
                    int size = int.Parse(q[0]);
                    if (size < 0 || size > 3) return null;
                    r.Rack[i] = new Bag { Size = (BagSize)size, CycleIndex = int.Parse(q[1]), UnlockStartTicks = long.Parse(q[2]), ReadyTicks = long.Parse(q[3]) };
                }
                return r;
            }
            catch (FormatException) { return null; }
            catch (OverflowException) { return null; }
        }
    }
}
