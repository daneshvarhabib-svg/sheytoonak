using System;
using System.Security.Cryptography;
using System.Text;

namespace Sheytoonak.Core.Bags
{
    /// <summary>A pack of the premium currency (ليرات) for real money.</summary>
    public struct LiraPack { public string Sku; public string NameAr; public int Lira; public int PriceToman; }
    /// <summary>A pile of shards for lira.</summary>
    public struct ShardPack { public string NameAr; public int Shards; public int Lira; }

    /// <summary>
    /// The rest of the reference game's shop (user 2026-11-11: «نه اونا رو هم بساز همین حالا دیگه»): a premium currency
    /// (ليرة, the reference's gems) bought for money, shard piles and bags bought with it, a rotating half-price special
    /// offer, a «watch an ad» pile and prize codes. The money and the ads go through providers in the game layer
    /// (StorePayments / RewardedAds) that do nothing until a store account (Cafe Bazaar / Myket / Google Play) and an ad
    /// network are set up by the owner -- nothing here can charge anyone.
    /// ⚠️ All prices are placeholders until the economy is tested.
    /// </summary>
    public static class ShopCatalog
    {
        public static readonly LiraPack[] LiraPacks =
        {
            new LiraPack { Sku = "lira_200",   NameAr = "قبضة",   Lira = 200,   PriceToman = 14900 },
            new LiraPack { Sku = "lira_500",   NameAr = "كمشة",   Lira = 500,   PriceToman = 29900 },
            new LiraPack { Sku = "lira_1200",  NameAr = "كيس",    Lira = 1200,  PriceToman = 59900 },
            new LiraPack { Sku = "lira_2500",  NameAr = "صندوق",  Lira = 2500,  PriceToman = 119900 },
            new LiraPack { Sku = "lira_6500",  NameAr = "خزنة",   Lira = 6500,  PriceToman = 299900 },
        };

        public static readonly ShardPack[] ShardPacks =
        {
            new ShardPack { NameAr = "كمشة زجاج", Shards = 1000,  Lira = 60 },
            new ShardPack { NameAr = "سطل زجاج",  Shards = 10000, Lira = 500 },
            new ShardPack { NameAr = "برميل زجاج", Shards = 100000, Lira = 4500 },
        };

        /// <summary>Bags for lira (open at once).</summary>
        public static int BagLira(BagSize s) => s == BagSize.Huge ? 2200 : s == BagSize.Large ? 900 : s == BagSize.Medium ? 300 : 100;

        // ------------------------------------------------------------------ the special offer (half price, 3 days)

        public struct Special { public string Sku; public BagSize Bag; public int Shards; public int PriceToman, FullToman; public DateTime EndsUtc; }
        public const int SpecialDays = 3;
        static readonly Special[] Specials =
        {
            new Special { Sku = "special_a", Bag = BagSize.Huge,  Shards = 13000, PriceToman = 149900, FullToman = 300000 },
            new Special { Sku = "special_b", Bag = BagSize.Large, Shards = 8000,  PriceToman = 99900,  FullToman = 200000 },
        };

        /// <summary>The offer running now: one of two, three days each, the same for everyone at the same time.</summary>
        public static Special SpecialNow(DateTime utcNow)
        {
            long period = (long)Math.Floor((utcNow - new DateTime(2026, 1, 1, 20, 30, 0, DateTimeKind.Utc)).TotalDays / SpecialDays);
            var s = Specials[(int)(((period % Specials.Length) + Specials.Length) % Specials.Length)];
            s.EndsUtc = new DateTime(2026, 1, 1, 20, 30, 0, DateTimeKind.Utc).AddDays((period + 1) * SpecialDays);
            return s;
        }

        // ------------------------------------------------------------------ prize codes

        public struct PrizeCode { public string Hash; public int Lira; public int Shards; public BagSize? Bag; }

        /// <summary>
        /// ⚠️ The repository is public: only the SHA-256 of each code is here, never the code. The owner hands out the codes
        /// (CHANGELOG says where they are listed). Each code works once per device.
        /// </summary>
        public static readonly PrizeCode[] Codes =
        {
            new PrizeCode { Hash = "1e9d212b4fc0cfa2f320bddc259a08431f72b02a1862967d1efdf190f3a3bdc6", Lira = 100 },
            new PrizeCode { Hash = "fbbae3074998bda0bfb2771272f828e03b9f82c844e36e87a36d2150ef6e9a60", Shards = 3000 },
            new PrizeCode { Hash = "7090c9dcf80233b2969bf050d723449e3b4d70ee8512e5b3e5072572adc409c6", Bag = BagSize.Large },
        };

        public static string HashOf(string code)
        {
            var norm = (code ?? "").Trim().ToUpperInvariant();
            using (var sha = SHA256.Create())
            {
                var b = sha.ComputeHash(Encoding.UTF8.GetBytes("sheytoonak:" + norm));
                var sb = new StringBuilder(); foreach (var x in b) sb.Append(x.ToString("x2"));
                return sb.ToString();
            }
        }

        /// <summary>The code's prize, or null if it is not a code (or already used: <paramref name="used"/> holds hashes).</summary>
        public static PrizeCode? Redeem(string code, System.Collections.Generic.ICollection<string> used, PrizeCode[] codes = null)
        {
            var h = HashOf(code);
            if (used != null && used.Contains(h)) return null;
            foreach (var c in codes ?? Codes) if (c.Hash == h) { used?.Add(h); return c; }
            return null;
        }
    }
}
