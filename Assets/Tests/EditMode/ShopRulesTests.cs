using System;
using NUnit.Framework;
using Sheytoonak.Core.Bags;

namespace Sheytoonak.Tests
{
    public class ShopRulesTests
    {
        static readonly string[] Tools = { "pigeon", "bus", "wind", "carpet", "puddle", "rain", "fart", "nails" };
        static readonly DateTime D = new DateTime(2026, 11, 11);

        [Test]
        public void Same_day_same_offers_next_day_new_ones()
        {
            var a = ShopRules.OffersFor(D, Tools); var b = ShopRules.OffersFor(D, Tools);
            for (int i = 0; i < ShopRules.Slots; i++) Assert.AreEqual(a[i].ToolId, b[i].ToolId);
            var c = ShopRules.OffersFor(D.AddDays(1), Tools);
            bool differs = false; for (int i = 0; i < ShopRules.Slots; i++) differs |= a[i].ToolId != c[i].ToolId;
            Assert.IsTrue(differs);
            Assert.AreEqual(0, a[0].Price, "the first pile is free");
        }

        [Test]
        public void Each_offer_is_bought_once_a_day_and_costs_shards()
        {
            var s = new ShopRules();
            int shards = 10000;
            Assert.AreEqual(ShopRules.Reject.None, s.Buy(D, 1, Tools, ref shards, out var o));
            Assert.AreEqual(10000 - o.Price, shards);
            Assert.AreEqual(ShopRules.Reject.SoldOut, s.Buy(D, 1, Tools, ref shards, out _));
            Assert.IsTrue(s.Bought(D, 1));
            Assert.IsFalse(s.Bought(D.AddDays(1), 1), "a new day, a new shop");
            Assert.AreEqual(ShopRules.Reject.None, s.Buy(D.AddDays(1), 1, Tools, ref shards, out _));
            int poor = 0;
            Assert.AreEqual(ShopRules.Reject.NotEnoughShards, s.Buy(D.AddDays(1), 2, Tools, ref poor, out _));
            Assert.AreEqual(ShopRules.Reject.None, s.Buy(D.AddDays(1), 0, Tools, ref poor, out _), "the free pile needs nothing");
        }

        [Test]
        public void Purchases_survive_a_save()
        {
            var s = new ShopRules(); int shards = 5000;
            s.Buy(D, 3, Tools, ref shards, out _);
            var t = ShopRules.Parse(s.Serialize());
            Assert.IsTrue(t.Bought(D, 3)); Assert.IsFalse(t.Bought(D, 2));
            Assert.IsFalse(ShopRules.Parse("junk").Bought(D, 0));
        }
    }
}
