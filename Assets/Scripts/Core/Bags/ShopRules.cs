using System;
using System.Collections.Generic;

namespace Sheytoonak.Core.Bags
{
    /// <summary>One card offer of the day: a pile of cards of one tool for shards (or free).</summary>
    public struct DailyOffer
    {
        public string ToolId;
        public int Count;
        public int Price;   // shards; 0 = free
    }

    /// <summary>
    /// The shop (user 2026-11-11, with a screen recording of the reference game's shop): «بخش دکان رو از روی ویدیو الهام
    /// بگیری». Only the parts that need no real money: the daily card offers (six piles of one tool's cards each, one of
    /// them free, all changing at Tehran midnight, each bought once) and bags for shards that open at once.
    /// ⚠️ The reference's gems/coins-for-money and watch-an-ad offers are left out on purpose: no purchase of any kind
    /// before 50k installs (SCENARIO §13) and no ad SDK. The free pile stands where the ad pile was.
    /// The day's offers are fixed by the date (same day = same offers, no reroll by reinstalling).
    /// ⚠️ Prices are placeholders until the economy is tested.
    /// </summary>
    public sealed class ShopRules
    {
        public const int Slots = 6;
        /// <summary>
        /// Cards in each slot: slot 0 is the free pile. User (2026-11-11): «توی دکان نباید ۱۰ تا امتیاز از یه کارت بدیم.
        /// باید از هر کارت نهایتاً ۳ و حداقل یک امتیاز بدیم روزانه که چند روز طول بکشه ارتقاء». Each slot is a different
        /// tool, so no tool gets more than 3 cards a day from the shop. ⚠️ It used to be 3/10/20/10/40/20: one shop visit
        /// was a whole level 1→2 and broke ToolLevels' calendar.
        /// </summary>
        public static readonly int[] Counts = { 1, 1, 2, 2, 3, 3 };
        public const int MaxCardsPerTool = 3;
        public const int ShardsPerCard = 25;
        public static int PriceFor(int count) => count * ShardsPerCard;

        /// <summary>Bags for shards (opened at once, no timer): medium and large.</summary>
        public static readonly BagSize[] BagsForSale = { BagSize.Medium, BagSize.Large };
        public static int BagPrice(BagSize s) => s == BagSize.Medium ? 600 : s == BagSize.Large ? 1500 : 250;

        public static DailyOffer[] OffersFor(DateTime day, string[] tools)
        {
            var offers = new DailyOffer[Slots];
            if (tools == null || tools.Length == 0) return offers;
            var rng = new Random(day.Year * 1000 + day.DayOfYear);
            var used = new HashSet<string>();
            for (int i = 0; i < Slots; i++)
            {
                string id;
                int guard = 0;
                do { id = tools[rng.Next(tools.Length)]; } while (used.Contains(id) && ++guard < 50 && used.Count < tools.Length);
                used.Add(id);
                offers[i] = new DailyOffer { ToolId = id, Count = Counts[i], Price = i == 0 ? 0 : PriceFor(Counts[i]) };
            }
            return offers;
        }

        // ------------------------------------------------------------------ what was bought today

        DateTime day;
        bool[] bought = new bool[Slots];

        public bool Bought(DateTime today, int slot) => today.Date == day && bought[slot];

        public enum Reject { None, SoldOut, NotEnoughShards }

        /// <summary>Buys slot <paramref name="slot"/> of today's offers: shards are taken, the cards returned.</summary>
        public Reject Buy(DateTime today, int slot, string[] tools, ref int shards, out DailyOffer offer)
        {
            offer = OffersFor(today.Date, tools)[slot];
            if (today.Date != day) { day = today.Date; bought = new bool[Slots]; }
            if (bought[slot]) return Reject.SoldOut;
            if (shards < offer.Price) return Reject.NotEnoughShards;
            shards -= offer.Price;
            bought[slot] = true;
            return Reject.None;
        }

        public string Serialize()
        {
            var s = day.Ticks.ToString();
            for (int i = 0; i < Slots; i++) s += bought[i] ? "1" : "0";
            return s;
        }

        public static ShopRules Parse(string s)
        {
            var r = new ShopRules();
            if (string.IsNullOrEmpty(s) || s.Length < Slots + 1) return r;
            if (!long.TryParse(s.Substring(0, s.Length - Slots), out long ticks)) return r;
            r.day = new DateTime(ticks);
            for (int i = 0; i < Slots; i++) r.bought[i] = s[s.Length - Slots + i] == '1';
            return r;
        }
    }
}
