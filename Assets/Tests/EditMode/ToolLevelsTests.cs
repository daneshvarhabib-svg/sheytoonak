using NUnit.Framework;
using Sheytoonak.Core.Bags;
using Sheytoonak.Core.Duel;

namespace Sheytoonak.Tests
{
    public class ToolLevelsTests
    {
        [Test]
        public void Cards_alone_do_not_upgrade_shards_make_it_real()
        {
            var t = new ToolLevels();
            for (int i = 0; i < 10; i++) t.AddCards(new[] { "pigeon" });
            int shards = 100;
            Assert.AreEqual(ToolLevels.Reject.NotEnoughShards, t.Upgrade("pigeon", ref shards));
            Assert.AreEqual(1, t.Level("pigeon"));
            shards = 250;
            Assert.AreEqual(ToolLevels.Reject.None, t.Upgrade("pigeon", ref shards));
            Assert.AreEqual(2, t.Level("pigeon"));
            Assert.AreEqual(50, shards);
            Assert.AreEqual(0, t.Cards("pigeon"), "the cards of the step are spent");
        }

        [Test]
        public void Shards_alone_do_not_upgrade()
        {
            var t = new ToolLevels();
            int shards = 999999;
            Assert.AreEqual(ToolLevels.Reject.NotEnoughCards, t.Upgrade("bus", ref shards));
            Assert.AreEqual(999999, shards);
        }

        [Test]
        public void Every_step_costs_far_more_than_the_one_before()
        {
            for (int i = 1; i < ToolLevels.CardsFor.Length; i++)
            {
                Assert.Greater(ToolLevels.CardsFor[i], ToolLevels.CardsFor[i - 1] * 2 - 1);
                Assert.Greater(ToolLevels.ShardsFor[i], ToolLevels.ShardsFor[i - 1] * 3);
            }
        }

        /// <summary>User (2026-11-09): level 2 in under a week, 3 in a month, 4 in a season, 5 in a year, at a normal pace.</summary>
        [Test]
        public void The_level_calendar_at_five_wins_a_day()
        {
            int tools = StarterHeroes.All.Length * 2;
            double perWin = 0;
            foreach (var b in BagRules.Cycle) perWin += BagRules.CardsIn(b);
            perWin /= BagRules.Cycle.Length;
            double perToolPerDay = 5 * perWin / tools;
            double need = 0;
            double[] days = new double[4];
            for (int i = 0; i < 4; i++) { need += ToolLevels.CardsFor[i]; days[i] = need / perToolPerDay; }
            Assert.Less(days[0], 7.0, "level 2 inside a week");
            Assert.That(days[1], Is.InRange(25.0, 45.0), "level 3 about a month");
            Assert.That(days[2], Is.InRange(75.0, 120.0), "level 4 about a season");
            Assert.That(days[3], Is.InRange(300.0, 420.0), "level 5 about a year");
        }

        [Test]
        public void Levels_and_cards_survive_a_save()
        {
            var t = new ToolLevels();
            t.SetLevel("bus", 3); t.AddCards(new[] { "bus", "bus", "wind" });
            var u = ToolLevels.Parse(t.Serialize());
            Assert.AreEqual(3, u.Level("bus")); Assert.AreEqual(2, u.Cards("bus"));
            Assert.AreEqual(1, u.Level("wind")); Assert.AreEqual(1, u.Cards("wind"));
            Assert.AreEqual(1, ToolLevels.Parse("garbage").Level("bus"));
        }

        [Test]
        public void The_top_level_is_five()
        {
            var t = new ToolLevels(); t.SetLevel("rain", 5);
            int s = 1000000;
            for (int i = 0; i < 1000; i++) t.AddCards(new[] { "rain" });
            Assert.AreEqual(ToolLevels.Reject.Top, t.Upgrade("rain", ref s));
        }
    }
}
