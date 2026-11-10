using System;
using NUnit.Framework;
using Sheytoonak.Core.Bags;

namespace Sheytoonak.Tests
{
    public class BagRulesTests
    {
        static readonly DateTime T0 = new DateTime(2026, 11, 3, 8, 0, 0, DateTimeKind.Utc);
        static readonly string[] Tools = { "pigeon", "bus", "wind" };

        [Test]
        public void The_cycle_is_30_small_8_medium_2_large()
        {
            int s = 0, m = 0, l = 0;
            foreach (var b in BagRules.Cycle) { if (b == BagSize.Small) s++; else if (b == BagSize.Medium) m++; else if (b == BagSize.Large) l++; }
            Assert.AreEqual(40, BagRules.Cycle.Length);
            Assert.AreEqual(30, s); Assert.AreEqual(8, m); Assert.AreEqual(2, l);
        }

        [Test]
        public void A_full_rack_takes_no_bag_and_the_cycle_waits()
        {
            var r = new BagRules(T0);
            for (int i = 0; i < 4; i++) Assert.AreEqual(i, r.AddFromWin());
            Assert.AreEqual(-1, r.AddFromWin());
            Assert.AreEqual(4, r.CycleStep, "a win with a full rack does not skip a bag of the cycle");
        }

        [Test]
        public void One_unlocks_at_a_time_and_the_first_week_clock_speeds_it_up()
        {
            var r = new BagRules(T0);
            r.AddFromWin(); r.AddFromWin();
            Assert.IsTrue(r.StartUnlock(0, T0));
            Assert.IsFalse(r.StartUnlock(1, T0), "one at a time");
            // user (2026-11-10): at least an hour, even on the first day
            Assert.IsFalse(r.IsReady(0, T0.AddMinutes(59)), "never under an hour");
            Assert.IsTrue(r.IsReady(0, T0.AddHours(1)));
            Assert.IsNull(r.Open(1, T0.AddHours(1), Tools), "the other bag was never started");
            var c = r.Open(0, T0.AddHours(1), Tools);
            Assert.IsNotNull(c); Assert.Greater(c.Value.Shards, 0); Assert.AreEqual(BagRules.CardsIn(BagSize.Small), c.Value.UpgradeCards.Length);
            Assert.AreEqual(1.0, new BagRules(T0).ClockFactor(T0.AddDays(8)), 1e-9);
            var big = new BagRules(T0);
            Assert.AreEqual(8.0, big.UnlockTime(BagSize.Large, T0.AddDays(10)).TotalHours, 1e-9);
            Assert.AreEqual(1.0, big.UnlockTime(BagSize.Medium, T0).TotalHours, 1e-9, "the first-week clock never goes under an hour");
        }

        [Test]
        public void Twenty_curses_give_a_bag_once()
        {
            var r = new BagRules(T0);
            Assert.AreEqual(0, r.AddFromCurses(19));
            Assert.AreEqual(1, r.AddFromCurses(20));
            Assert.AreEqual(0, r.AddFromCurses(25));
            Assert.AreEqual(2, r.AddFromCurses(60));
        }

        [Test]
        public void Same_bag_same_loot_and_a_round_trip_keeps_the_rack()
        {
            var r = new BagRules(T0);
            r.AddFromWin(); r.AddFromWin(); r.StartUnlock(1, T0);
            var back = BagRules.Parse(r.Serialize());
            Assert.AreEqual(r.Serialize(), back.Serialize());
            Assert.AreEqual(BagRules.ContentsOf(r.Rack[0], Tools).Shards, BagRules.ContentsOf(back.Rack[0], Tools).Shards);
            Assert.IsNull(BagRules.Parse("1;2"));
        }
    }
}
