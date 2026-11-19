using System;
using Sheytoonak.Core.Bags;

namespace Sheytoonak.Core.Missions
{
    public enum MapPrizeKind { Shards, Marbles, Bag }
    public struct MapPrize { public MapPrizeKind Kind; public int Amount; public BagSize Bag; }

    /// <summary>
    /// The map of the city (user 2026-11-19: «یه نقشه‌ای از یه شهر خیالی ... با هر برد یه نقطه ش روشن شه ... مثل نقشه پیشروی
    /// جنگ های زمینی ... برای پر شدنش حداقل ۵۰۰۰ فحش ... به ازای هر ۱۰۰ تا فحشی هم که پیش روی داشته باشه یه جایزه‌ای ... اگر هم
    /// ببازه که پس روی می‌کنه و محدوده از دست می‌ده»). The front line IS the curse count: a win is +5 curses = one more house
    /// lit, a loss is -2 and the line falls back. 50 quarters of 20 houses (100 curses) each; a quarter's prize is paid
    /// once, the first time the line passes it (falling back and retaking pays nothing again).
    /// </summary>
    public static class CityMap
    {
        public const int Total = 5000, PerQuarter = 100, PerHouse = 5;
        public const int Quarters = Total / PerQuarter, HousesPerQuarter = PerQuarter / PerHouse;

        /// <summary>Quarters fully held at that curse count.</summary>
        public static int QuartersHeld(int curses) => Math.Max(0, Math.Min(Quarters, curses / PerQuarter));
        /// <summary>Houses lit inside the quarter the line is in (0..19); 0 once the whole city is held.</summary>
        public static int HousesInFront(int curses) => curses >= Total ? 0 : Math.Max(0, curses) % PerQuarter / PerHouse;
        /// <summary>Prizes waiting: quarters ever reached (by the highest count) that were not paid yet.</summary>
        public static int Waiting(int peakCurses, int paid) => Math.Max(0, QuartersHeld(peakCurses) - Math.Max(0, paid));

        /// <summary>Prize of quarter number q (1..50). ⚠️ First guess, to be tuned after testing.</summary>
        public static MapPrize PrizeOf(int q)
        {
            if (q >= Quarters) return new MapPrize { Kind = MapPrizeKind.Bag, Bag = BagSize.Huge };
            if (q % 10 == 0) return new MapPrize { Kind = MapPrizeKind.Bag, Bag = BagSize.Large };
            if (q % 5 == 0) return new MapPrize { Kind = MapPrizeKind.Bag, Bag = BagSize.Medium };
            if (q % 2 == 0) return new MapPrize { Kind = MapPrizeKind.Marbles, Amount = 10 };
            return new MapPrize { Kind = MapPrizeKind.Shards, Amount = 200 };
        }
    }
}
