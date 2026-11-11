using System;

namespace Sheytoonak.Core.Bags
{
    /// <summary>
    /// When to show the phone notification «your bag is ready» (research proposal 25; user 2026-11-11: «پ۲۵ اجرا بشه
    /// باقیش نه»). Only one bag unlocks at a time, so there is at most one notification waiting, and it is scheduled
    /// only while the game is closed (cancelled as soon as it is back).
    /// ⚠️ Never at night: 23:00 to 09:00 Tehran moves it to 09:00. ⚠️ At most one a day: if one already fired on that
    /// Tehran day, the next waits for 09:00 of the day after -- a game that nags is a game whose notifications get
    /// switched off for good.
    /// </summary>
    public static class BagNotice
    {
        public static readonly TimeSpan Tehran = TimeSpan.FromHours(3.5);
        public const int QuietFrom = 23, QuietUntil = 9;

        /// <summary>
        /// The UTC time to fire, or null for none (nothing unlocking, or already ready -- the player saw it before leaving).
        /// <paramref name="lastFireUtc"/> is the last notification that fired (null = never).
        /// </summary>
        public static DateTime? When(long readyTicks, DateTime nowUtc, DateTime? lastFireUtc)
        {
            if (readyTicks <= 0 || readyTicks == long.MaxValue || readyTicks <= nowUtc.Ticks) return null;
            var fire = OutOfQuiet(new DateTime(readyTicks, DateTimeKind.Utc));
            if (lastFireUtc.HasValue && LocalDay(lastFireUtc.Value) == LocalDay(fire))
                fire = new DateTime(LocalDay(fire).AddDays(1).AddHours(QuietUntil).Ticks - Tehran.Ticks, DateTimeKind.Utc);
            return fire;
        }

        static DateTime LocalDay(DateTime utc) => (utc + Tehran).Date;

        static DateTime OutOfQuiet(DateTime utc)
        {
            var local = utc + Tehran;
            DateTime moved;
            if (local.Hour >= QuietFrom) moved = local.Date.AddDays(1).AddHours(QuietUntil);
            else if (local.Hour < QuietUntil) moved = local.Date.AddHours(QuietUntil);
            else return utc;
            return new DateTime(moved.Ticks - Tehran.Ticks, DateTimeKind.Utc);
        }
    }
}
