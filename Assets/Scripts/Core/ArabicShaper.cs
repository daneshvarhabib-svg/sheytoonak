using System.Collections.Generic;
using System.Text;

namespace Sheytoonak.Core
{
    /// <summary>
    /// Turns logical Arabic text into the glyph sequence a left-to-right, shaping-unaware renderer
    /// (Unity's IMGUI / legacy Text) must draw: contextual presentation forms (U+FE70..FEFF),
    /// lam-alef ligatures, and the visual (reversed) order with digit and Latin runs kept left-to-right.
    /// Pure C# so it is unit-tested. Needs a font that has the presentation-form glyphs (Vazirmatn does).
    /// </summary>
    public static class ArabicShaper
    {
        // base code point -> isolated, final, initial, medial (0 = the letter has no such form)
        static readonly Dictionary<char, char[]> Forms = new Dictionary<char, char[]>
        {
            ['ء'] = new[] { 'ﺀ', '\0', '\0', '\0' },             // hamza
            ['آ'] = new[] { 'ﺁ', 'ﺂ', '\0', '\0' },         // alef madda
            ['أ'] = new[] { 'ﺃ', 'ﺄ', '\0', '\0' },         // alef hamza above
            ['ؤ'] = new[] { 'ﺅ', 'ﺆ', '\0', '\0' },         // waw hamza
            ['إ'] = new[] { 'ﺇ', 'ﺈ', '\0', '\0' },         // alef hamza below
            ['ئ'] = new[] { 'ﺉ', 'ﺊ', 'ﺋ', 'ﺌ' }, // yeh hamza
            ['ا'] = new[] { 'ﺍ', 'ﺎ', '\0', '\0' },         // alef
            ['ب'] = new[] { 'ﺏ', 'ﺐ', 'ﺑ', 'ﺒ' }, // beh
            ['ة'] = new[] { 'ﺓ', 'ﺔ', '\0', '\0' },         // teh marbuta
            ['ت'] = new[] { 'ﺕ', 'ﺖ', 'ﺗ', 'ﺘ' }, // teh
            ['ث'] = new[] { 'ﺙ', 'ﺚ', 'ﺛ', 'ﺜ' }, // theh
            ['ج'] = new[] { 'ﺝ', 'ﺞ', 'ﺟ', 'ﺠ' }, // jeem
            ['ح'] = new[] { 'ﺡ', 'ﺢ', 'ﺣ', 'ﺤ' }, // hah
            ['خ'] = new[] { 'ﺥ', 'ﺦ', 'ﺧ', 'ﺨ' }, // khah
            ['د'] = new[] { 'ﺩ', 'ﺪ', '\0', '\0' },         // dal
            ['ذ'] = new[] { 'ﺫ', 'ﺬ', '\0', '\0' },         // thal
            ['ر'] = new[] { 'ﺭ', 'ﺮ', '\0', '\0' },         // reh
            ['ز'] = new[] { 'ﺯ', 'ﺰ', '\0', '\0' },         // zain
            ['س'] = new[] { 'ﺱ', 'ﺲ', 'ﺳ', 'ﺴ' }, // seen
            ['ش'] = new[] { 'ﺵ', 'ﺶ', 'ﺷ', 'ﺸ' }, // sheen
            ['ص'] = new[] { 'ﺹ', 'ﺺ', 'ﺻ', 'ﺼ' }, // sad
            ['ض'] = new[] { 'ﺽ', 'ﺾ', 'ﺿ', 'ﻀ' }, // dad
            ['ط'] = new[] { 'ﻁ', 'ﻂ', 'ﻃ', 'ﻄ' }, // tah
            ['ظ'] = new[] { 'ﻅ', 'ﻆ', 'ﻇ', 'ﻈ' }, // zah
            ['ع'] = new[] { 'ﻉ', 'ﻊ', 'ﻋ', 'ﻌ' }, // ain
            ['غ'] = new[] { 'ﻍ', 'ﻎ', 'ﻏ', 'ﻐ' }, // ghain
            ['ف'] = new[] { 'ﻑ', 'ﻒ', 'ﻓ', 'ﻔ' }, // feh
            ['ق'] = new[] { 'ﻕ', 'ﻖ', 'ﻗ', 'ﻘ' }, // qaf
            ['ك'] = new[] { 'ﻙ', 'ﻚ', 'ﻛ', 'ﻜ' }, // kaf
            ['ل'] = new[] { 'ﻝ', 'ﻞ', 'ﻟ', 'ﻠ' }, // lam
            ['م'] = new[] { 'ﻡ', 'ﻢ', 'ﻣ', 'ﻤ' }, // meem
            ['ن'] = new[] { 'ﻥ', 'ﻦ', 'ﻧ', 'ﻨ' }, // noon
            ['ه'] = new[] { 'ﻩ', 'ﻪ', 'ﻫ', 'ﻬ' }, // heh
            ['و'] = new[] { 'ﻭ', 'ﻮ', '\0', '\0' },         // waw
            ['ى'] = new[] { 'ﻯ', 'ﻰ', '\0', '\0' },         // alef maksura
            ['ي'] = new[] { 'ﻱ', 'ﻲ', 'ﻳ', 'ﻴ' }, // yeh
            ['پ'] = new[] { 'ﭖ', 'ﭗ', 'ﭘ', 'ﭙ' }, // peh
            ['چ'] = new[] { 'ﭺ', 'ﭻ', 'ﭼ', 'ﭽ' }, // tcheh
            ['ژ'] = new[] { 'ﮊ', 'ﮋ', '\0', '\0' },         // jeh
            ['ک'] = new[] { 'ﮎ', 'ﮏ', 'ﮐ', 'ﮑ' }, // keheh
            ['گ'] = new[] { 'ﮒ', 'ﮓ', 'ﮔ', 'ﮕ' }, // gaf
            ['ی'] = new[] { 'ﯼ', 'ﯽ', 'ﯾ', 'ﯿ' }, // farsi yeh
        };

        // lam + alef variants -> isolated, final ligature
        static readonly Dictionary<char, char[]> LamAlef = new Dictionary<char, char[]>
        {
            ['آ'] = new[] { 'ﻵ', 'ﻶ' },
            ['أ'] = new[] { 'ﻷ', 'ﻸ' },
            ['إ'] = new[] { 'ﻹ', 'ﻺ' },
            ['ا'] = new[] { 'ﻻ', 'ﻼ' },
        };

        /// <summary>Every presentation glyph the shaper can emit, plus digits and marks: for warming a dynamic font atlas.</summary>
        public static string AllGlyphs()
        {
            var sb = new StringBuilder();
            foreach (var kv in Forms) { sb.Append(kv.Key); foreach (char c in kv.Value) if (c != '\0') sb.Append(c); }
            foreach (var kv in LamAlef) foreach (char c in kv.Value) sb.Append(c);
            sb.Append("٠١٢٣٤٥٦٧٨٩0123456789:.-()/%،؟!✦♥♡←→ ّ");
            return sb.ToString();
        }

        static bool IsTashkeel(char c) => (c >= 'ً' && c <= 'ٟ') || c == 'ٰ' || c == 'ـ' && false;
        static bool IsArabicLetter(char c) => Forms.ContainsKey(c) || c == 'ـ';
        static bool JoinsNext(char c) => c == 'ـ' || (Forms.TryGetValue(c, out var f) && f[2] != '\0');
        static bool JoinsPrev(char c) => c == 'ـ' || Forms.ContainsKey(c);
        public static bool IsArabic(char c) => (c >= '؀' && c <= 'ۿ') || (c >= 'ﭐ' && c <= '﷿') || (c >= 'ﹰ' && c <= '﻿');
        static bool IsLtrRun(char c) => (c >= '0' && c <= '9') || (c >= '\u0660' && c <= '\u0669') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '%' || c == '/' || c == ':';

        /// <summary>Western digits to Eastern Arabic digits (٠١٢٣٤٥٦٧٨٩), as used in the Levant.</summary>
        public static string EasternDigits(string s)
        {
            var sb = new StringBuilder(s.Length);
            foreach (char c in s) sb.Append(c >= '0' && c <= '9' ? (char)('٠' + (c - '0')) : c);
            return sb.ToString();
        }

        /// <summary>Contextual forms in logical order (no reordering).</summary>
        public static string Shape(string text)
        {
            if (string.IsNullOrEmpty(text)) return text ?? "";
            var chars = new List<char>(text);
            var sb = new StringBuilder(text.Length);
            for (int i = 0; i < chars.Count; i++)
            {
                char c = chars[i];
                if (!Forms.ContainsKey(c)) { sb.Append(c); continue; }

                int prev = i - 1; while (prev >= 0 && IsTashkeel(chars[prev])) prev--;
                int next = i + 1; while (next < chars.Count && IsTashkeel(chars[next])) next++;
                bool joinedFromPrev = prev >= 0 && IsArabicLetter(chars[prev]) && JoinsNext(chars[prev]);

                if (c == 'ل' && next < chars.Count && LamAlef.TryGetValue(chars[next], out var lig))
                {
                    sb.Append(joinedFromPrev ? lig[1] : lig[0]);
                    for (int k = i + 1; k < next; k++) sb.Append(chars[k]); // tashkeel between lam and alef
                    i = next;
                    continue;
                }

                bool joinedToNext = next < chars.Count && IsArabicLetter(chars[next]) && JoinsPrev(chars[next]) && JoinsNext(c);
                char[] f = Forms[c];
                char g;
                if (joinedFromPrev && joinedToNext) g = f[3] != '\0' ? f[3] : f[1];
                else if (joinedFromPrev) g = f[1] != '\0' ? f[1] : f[0];
                else if (joinedToNext) g = f[2] != '\0' ? f[2] : f[0];
                else g = f[0];
                sb.Append(g);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Shaped and reordered for a left-to-right renderer: the line is reversed, digit/Latin runs
        /// are put back left-to-right, and brackets are mirrored. Call once per displayed string.
        /// </summary>
        public static string ForLtrRenderer(string text)
        {
            string shaped = Shape(text);
            var chars = shaped.ToCharArray();
            System.Array.Reverse(chars);
            for (int i = 0; i < chars.Length; i++)
            {
                chars[i] = Mirror(chars[i]);
                if (!IsLtrRun(chars[i])) continue;
                int j = i; while (j < chars.Length && IsLtrRun(chars[j])) j++;
                System.Array.Reverse(chars, i, j - i);
                i = j;
            }
            return new string(chars);
        }

        static char Mirror(char c)
        {
            switch (c)
            {
                case '(': return ')';
                case ')': return '(';
                case '[': return ']';
                case ']': return '[';
                case '<': return '>';
                case '>': return '<';
                case '{': return '}';
                case '}': return '{';
                default: return c;
            }
        }
    }
}
