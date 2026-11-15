using System;
using System.Collections.Generic;

namespace Sheytoonak.Core
{
    /// <summary>
    /// The «how to play» lines shown as a subtitle while the player waits (user 2026-11-15: «۱۰۰ تا دستورالعمل استفاده
    /// از بازی در قالب ۱۰۰ جمله کوتاه آماده کنیم و هربار که کاربر منتظر لود یه صفحه است یا منتظر اومدن رقیبشه در قالب
    /// زیرنویس یک جمله رو نشونش بدیم»). A shuffled deck, not a random pick: every line is seen once before any repeats,
    /// and a new round never starts with the line that ended the last one.
    /// </summary>
    public sealed class TipDeck
    {
        readonly string[] tips;
        readonly Random rng;
        readonly List<int> order = new List<int>();
        int next, last = -1;

        public TipDeck(IEnumerable<string> lines, int seed = 0)
        {
            var list = new List<string>();
            foreach (var l in lines ?? new string[0]) { var t = l?.Trim(); if (!string.IsNullOrEmpty(t)) list.Add(t); }
            tips = list.ToArray();
            rng = seed != 0 ? new Random(seed) : new Random();
        }

        public int Count => tips.Length;

        /// <summary>Lines of a text file, one tip per line (blank lines ignored).</summary>
        public static TipDeck FromText(string text, int seed = 0) => new TipDeck((text ?? "").Replace("\r", "").Split('\n'), seed);

        public string Next()
        {
            if (tips.Length == 0) return "";
            if (next >= order.Count)
            {
                order.Clear();
                for (int i = 0; i < tips.Length; i++) order.Add(i);
                for (int i = order.Count - 1; i > 0; i--) { int j = rng.Next(i + 1); (order[i], order[j]) = (order[j], order[i]); }
                if (order.Count > 1 && order[0] == last) (order[0], order[1]) = (order[1], order[0]);
                next = 0;
            }
            last = order[next++];
            return tips[last];
        }
    }
}
