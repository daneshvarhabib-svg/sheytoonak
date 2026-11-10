using System;
using System.Collections.Generic;
using NUnit.Framework;
using Sheytoonak.Core.Bags;

namespace Sheytoonak.Tests
{
    public class ShopCatalogTests
    {
        [Test]
        public void A_prize_code_works_once_and_ignores_case_and_spaces()
        {
            var codes = new[] { new ShopCatalog.PrizeCode { Hash = ShopCatalog.HashOf("TEST-1"), Lira = 50 } };
            var used = new HashSet<string>();
            var p = ShopCatalog.Redeem("  test-1 ", used, codes);
            Assert.IsNotNull(p); Assert.AreEqual(50, p.Value.Lira);
            Assert.IsNull(ShopCatalog.Redeem("TEST-1", used, codes), "once per device");
            Assert.IsNull(ShopCatalog.Redeem("NOPE", new HashSet<string>(), codes));
        }

        [Test]
        public void The_repository_holds_hashes_not_codes()
        {
            foreach (var c in ShopCatalog.Codes) Assert.AreEqual(64, c.Hash.Length);
        }

        [Test]
        public void The_special_offer_changes_every_three_days_and_is_half_price()
        {
            var t = new DateTime(2026, 11, 11, 10, 0, 0, DateTimeKind.Utc);
            var a = ShopCatalog.SpecialNow(t);
            Assert.Greater(a.EndsUtc, t); Assert.LessOrEqual((a.EndsUtc - t).TotalDays, ShopCatalog.SpecialDays);
            var b = ShopCatalog.SpecialNow(a.EndsUtc.AddMinutes(1));
            Assert.AreNotEqual(a.Sku, b.Sku);
            Assert.LessOrEqual(a.PriceToman * 2, a.FullToman + 1000);
        }

        [Test]
        public void Bigger_lira_packs_are_cheaper_per_lira()
        {
            for (int i = 1; i < ShopCatalog.LiraPacks.Length; i++)
                Assert.Less(ShopCatalog.LiraPacks[i].PriceToman / (double)ShopCatalog.LiraPacks[i].Lira, ShopCatalog.LiraPacks[i - 1].PriceToman / (double)ShopCatalog.LiraPacks[i - 1].Lira);
        }
    }
}
