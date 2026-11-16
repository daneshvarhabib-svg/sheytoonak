using System;
using System.Collections.Generic;
using NUnit.Framework;
using Sheytoonak.Core.Duel;

namespace Sheytoonak.Tests
{
    public class DailyComboTests
    {
        static readonly string[] Pool = { "abu_shahata", "zaatar", "fulla", "rima", "toni", "um_arabiyat", "hamada", "bubakr" };

        [Test]
        public void Three_distinct_heroes_the_same_all_day_whatever_the_pool_order()
        {
            var d = new DateTime(2026, 11, 16);
            var a = DailyCombo.For(d, Pool);
            Assert.AreEqual(3, a.Length);
            Assert.AreEqual(3, new HashSet<string>(a).Count);
            var shuffled = new List<string>(Pool); shuffled.Reverse();
            CollectionAssert.AreEqual(a, DailyCombo.For(d, shuffled), "every player sees the same three");
            foreach (var h in a) CollectionAssert.Contains(Pool, h);
        }

        [Test]
        public void It_changes_from_day_to_day()
        {
            int changed = 0;
            var prev = DailyCombo.For(new DateTime(2026, 11, 1), Pool);
            for (int i = 2; i <= 30; i++)
            {
                var c = DailyCombo.For(new DateTime(2026, 11, i), Pool);
                if (!new HashSet<string>(c).SetEquals(prev)) changed++;
                prev = c;
            }
            Assert.Greater(changed, 25);
        }

        [Test]
        public void Complete_needs_all_three_in_the_team()
        {
            var c = new[] { "zaatar", "toni", "rima" };
            Assert.IsTrue(DailyCombo.Complete(c, new[] { "rima", "zaatar", "toni" }));
            Assert.IsFalse(DailyCombo.Complete(c, new[] { "rima", "zaatar", "fulla" }));
            Assert.IsFalse(DailyCombo.Complete(new string[0], new[] { "rima" }));
        }
    }
}
