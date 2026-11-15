using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Sheytoonak.Core;

namespace Sheytoonak.Tests
{
    /// <summary>
    /// User (2026-11-15): «۱۰۰ تا دستورالعمل استفاده از بازی در قالب ۱۰۰ جمله کوتاه». Resources/Text/tips.txt is shown as a
    /// subtitle in the draft and under the web loading bar.
    /// </summary>
    public class TipsTests
    {
        static string TipsFile()
        {
            var d = new DirectoryInfo(Directory.GetCurrentDirectory());
            for (int i = 0; i < 6 && d != null; i++, d = d.Parent)
            {
                var p = Path.Combine(d.FullName, "Assets", "Resources", "Text", "tips.txt");
                if (File.Exists(p)) return p;
            }
            return null;
        }

        static List<string> Lines()
        {
            var f = TipsFile();
            Assert.IsNotNull(f, "Assets/Resources/Text/tips.txt is missing");
            var list = new List<string>();
            foreach (var l in File.ReadAllText(f).Replace("\r", "").Split('\n')) if (l.Trim().Length > 0) list.Add(l.Trim());
            return list;
        }

        [Test]
        public void There_are_one_hundred_short_different_tips()
        {
            var lines = Lines();
            Assert.AreEqual(100, lines.Count);
            Assert.AreEqual(100, new HashSet<string>(lines).Count, "a tip is repeated");
            foreach (var l in lines) Assert.LessOrEqual(l.Length, 80, "too long for one subtitle line: " + l);
        }

        [Test]
        public void Every_tip_uses_only_letters_the_shaper_and_font_carry()
        {
            // ⚠️ IMGUI cannot shape Arabic; ArabicShaper does, and the font atlas is warmed with exactly these glyphs (Ar.Warm).
            var ok = new HashSet<char>(ArabicShaper.AllGlyphs());
            foreach (var l in Lines())
                foreach (char c in l)
                {
                    bool tashkeel = c >= 'ً' && c <= 'ٟ';
                    Assert.IsTrue(ok.Contains(c) || tashkeel || c == ' ', "unsupported character U+" + ((int)c).ToString("X4") + " in: " + l);
                }
        }

        [Test]
        public void The_deck_shows_every_tip_once_before_repeating()
        {
            var lines = Lines();
            var deck = new TipDeck(lines, seed: 7);
            var seen = new HashSet<string>();
            for (int i = 0; i < lines.Count; i++) Assert.IsTrue(seen.Add(deck.Next()), "repeated inside one round");
            Assert.AreEqual(lines.Count, seen.Count);
        }

        [Test]
        public void A_new_round_never_repeats_the_last_tip()
        {
            for (int seed = 1; seed < 60; seed++)
            {
                var deck = new TipDeck(new[] { "a", "b", "c" }, seed);
                string prev = null;
                for (int i = 0; i < 30; i++) { var t = deck.Next(); Assert.AreNotEqual(prev, t, "seed " + seed); prev = t; }
            }
        }

        [Test]
        public void An_empty_file_gives_no_tip_instead_of_failing()
        {
            Assert.AreEqual("", TipDeck.FromText("\n\n").Next());
            Assert.AreEqual("x", TipDeck.FromText("x\r\n").Next());
        }
    }
}
