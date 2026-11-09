using System;
using System.Collections.Generic;
using System.Text;

namespace Sheytoonak.Core.Bags
{
    /// <summary>
    /// Levels 1..5 of every poison and antidote (SCENARIO §8). User (2026-11-09): «از توی کیسه یسری شانس بالقوه برای
    /// قوی‌تر کردن هر سم یا پادسم ایجاد بشه ولی برای بالفعل کردنش باید خرده شیشه خرج کنه … برای تبدیل پرنده از لول یک به
    /// دو شاید کمتر از یک هفته طول بکشه ولی برای بردنش به لول سه یه ماه … لول چهار یه فصل … لول پنج یه سال، مگر این که خیلی
    /// خیلی بازیکنه. این کار رو با گرونتر کردن هر مرحله … و هم‌زمان امتیاز لازم … بیشتر میکنیم».
    /// A card from a bag is a latent upgrade of one tool; when enough cards of that tool are in hand, shards make the
    /// next level real (the cards are spent). Both the cards and the shards of each step grow much faster than linearly.
    /// ⚠️ The numbers are tuned against BagRules' card yield at five wins a day (ToolLevelsTests checks the calendar:
    /// level 2 inside a week, 3 about a month, 4 about a season, 5 about a year). Change the yield, retune these.
    /// </summary>
    public sealed class ToolLevels
    {
        public const int MaxLevel = 5;
        /// <summary>Cards of that tool needed to go from level i+1 to i+2 (index 0 = 1→2).</summary>
        public static readonly int[] CardsFor = { 10, 50, 110, 400 };
        /// <summary>Shards to make that step real. ⚠️ placeholders until the economy is tested.</summary>
        public static readonly int[] ShardsFor = { 200, 1200, 5000, 30000 };

        readonly Dictionary<string, int> level = new Dictionary<string, int>();
        readonly Dictionary<string, int> cards = new Dictionary<string, int>();

        public int Level(string id) => id != null && level.TryGetValue(id, out var l) ? l : 1;
        public int Cards(string id) => id != null && cards.TryGetValue(id, out var c) ? c : 0;

        /// <summary>Cards still needed for the next level, or 0 at the top.</summary>
        public int CardsNeeded(string id) { int l = Level(id); return l >= MaxLevel ? 0 : CardsFor[l - 1]; }
        public int ShardsNeeded(string id) { int l = Level(id); return l >= MaxLevel ? 0 : ShardsFor[l - 1]; }

        public void AddCards(IEnumerable<string> ids)
        {
            if (ids == null) return;
            foreach (var id in ids) if (!string.IsNullOrEmpty(id)) cards[id] = Cards(id) + 1;
        }

        public enum Reject { None, Top, NotEnoughCards, NotEnoughShards }

        public Reject CanUpgrade(string id, int shards)
        {
            if (Level(id) >= MaxLevel) return Reject.Top;
            if (Cards(id) < CardsNeeded(id)) return Reject.NotEnoughCards;
            if (shards < ShardsNeeded(id)) return Reject.NotEnoughShards;
            return Reject.None;
        }

        /// <summary>Spends the cards and the shards of the step and raises the level. Returns what was refused, if anything.</summary>
        public Reject Upgrade(string id, ref int shards)
        {
            var r = CanUpgrade(id, shards);
            if (r != Reject.None) return r;
            cards[id] = Cards(id) - CardsNeeded(id);
            shards -= ShardsNeeded(id);
            level[id] = Level(id) + 1;
            return Reject.None;
        }

        /// <summary>A tool was renamed (open_window → carpet, 2026-11-09): its level and cards move with it.</summary>
        public bool Rename(string from, string to)
        {
            if (!level.ContainsKey(from) && !cards.ContainsKey(from)) return false;
            level[to] = Math.Max(Level(from), Level(to)); cards[to] = Cards(from) + Cards(to);
            level.Remove(from); cards.Remove(from);
            return true;
        }

        /// <summary>Tests and screenshots only.</summary>
        public void SetLevel(string id, int l) { level[id] = Math.Max(1, Math.Min(MaxLevel, l)); }

        // ------------------------------------------------------------------ save: "id:level:cards;..."

        public string Serialize()
        {
            var keys = new SortedSet<string>(level.Keys); keys.UnionWith(cards.Keys);
            var sb = new StringBuilder();
            foreach (var k in keys)
            {
                if (sb.Length > 0) sb.Append(';');
                sb.Append(k).Append(':').Append(Level(k)).Append(':').Append(Cards(k));
            }
            return sb.ToString();
        }

        public static ToolLevels Parse(string s)
        {
            var t = new ToolLevels();
            if (string.IsNullOrEmpty(s)) return t;
            foreach (var part in s.Split(';'))
            {
                var q = part.Split(':');
                if (q.Length != 3 || q[0].Length == 0) continue;
                if (!int.TryParse(q[1], out int l) || !int.TryParse(q[2], out int c)) continue;
                t.level[q[0]] = Math.Max(1, Math.Min(MaxLevel, l));
                t.cards[q[0]] = Math.Max(0, c);
            }
            return t;
        }
    }
}
