using System;
using System.Collections.Generic;

namespace Sheytoonak.Core.Duel
{
    /// <summary>
    /// Research proposal 39, approved 2026-11-16 («هر سه رو بسازیم»): «تركيبة اليوم». Three heroes a day, the same for every
    /// player (from the Tehran date, no server). Win a duel with all three in your team: a one-time prize that day. It sends
    /// players to heroes they leave on the bench. The mechanic is Hamster Kombat's daily combo (three cards a day); nothing
    /// else of it is used.
    /// </summary>
    public static class DailyCombo
    {
        public const int Size = 3;
        public const int Prize = 300;

        /// <summary>Three distinct heroes of the pool for that day (stable for the same day and pool).</summary>
        public static string[] For(DateTime day, IList<string> pool)
        {
            var ids = new List<string>(pool ?? new string[0]);
            ids.Sort(StringComparer.Ordinal); // the order of the pool must not change the pick
            if (ids.Count <= Size) return ids.ToArray();
            int seed = day.Year * 10000 + day.Month * 100 + day.Day;
            var rng = new Random(seed * 7919 + 17);
            for (int i = ids.Count - 1; i > 0; i--) { int j = rng.Next(i + 1); (ids[i], ids[j]) = (ids[j], ids[i]); }
            return ids.GetRange(0, Size).ToArray();
        }

        /// <summary>True when the team holds every hero of the combo.</summary>
        public static bool Complete(IList<string> combo, IList<string> team)
        {
            if (combo == null || team == null || combo.Count == 0) return false;
            foreach (var c in combo) if (!team.Contains(c)) return false;
            return true;
        }

        /// <summary>The day key kept in the save ("20261116").</summary>
        public static string Key(DateTime day) => day.ToString("yyyyMMdd");
    }
}
