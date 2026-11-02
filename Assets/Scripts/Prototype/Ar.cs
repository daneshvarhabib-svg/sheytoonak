using System;
using Sheytoonak.Core;
using UnityEngine;

namespace Sheytoonak
{
    /// <summary>
    /// The game's UI language is spoken Levantine Arabic (user, 2026-10-27: "زبان بازی باید عربی محاوره باشه").
    /// IMGUI cannot shape Arabic, so every string goes through <see cref="T"/> (contextual forms +
    /// visual order) and is drawn with the bundled Vazirmatn font (OFL), which has the presentation glyphs.
    /// </summary>
    public static class Ar
    {
        // ---- duel (docs/SCENARIO.fa.md)
        public const string DuelIntro = "مبارزة شبابيك: كل واحد بدوره، حركتين بالدور. شباك تحتاني = ١، فوقاني = ٢، وفي شباك مخبّى = ٣. اللي بياخد أكتر من النص بيربح.";
        public const string YourTurn = "دورك";
        public const string TheirTurn = "دور الخصم";
        public const string Actions = "حركات";
        public const string Rage = "غيظ";
        public const string RageReady = "عندك غيظ: حركة زيادة!";
        public const string RageUsed = "غيظ! حركة زيادة";
        public const string PassTurn = "خلص دوري";
        public const string Surrender = "بسلّم";
        public const string DraftTitle = "اختار فريقك";
        public const string DraftBanYou = "شيل بطل (بينشال من الكل)";
        public const string DraftPickYou = "اختار بطل";
        public const string DraftWait = "الخصم عم يختار...";
        public const string YouBan = "شلت:";
        public const string TheyBan = "الخصم شال:";
        public const string Banned = "مشطوب";
        public const string Mine = "إلك";
        public const string TheirPicks = "فريق الخصم";
        public const string SurrenderSure = "أكيد؟ دقّ كمان مرّة";
        public const string NoActions = "ما بقي حركات";
        public const string TimeUp = "خلص الوقت";
        public const string HiddenThree = "هيدا الشباك المخبّى: ٣!";
        public const string Intercepted = "خبطت بشي بالهوا!";
        public const string Covered = "مغطّى!";
        public const string Shut = "مسكّر!";
        public const string AlreadyBroken = "مكسور من قبل";
        public const string Missed = "طابت";
        public const string YouPoison = "رميت:";
        public const string TheyPoison = "الخصم رمى:";
        public const string YouAntidote = "دواك:";
        public const string TheyAntidote = "دوا الخصم:";
        public const string PoisonWord = "سمّ";
        public const string AntidoteWord = "دوا";
        public const string Won = "ربحت!";
        public const string Lost = "خسرت";
        public static string WindowsLine(int mine, int theirs, int majority) => "شبابيك " + N(mine) + " – " + N(theirs) + "  (الربح من " + N(majority) + ")";
        public static string Reward(int shards, int curseDelta, int curses) => "زجاج: +" + N(shards) + "   فحش: " + (curseDelta >= 0 ? "+" : "−") + N(Math.Abs(curseDelta)) + " (" + N(curses) + ")";
        public static string Timer(int seconds) => N(seconds) + " ث";
        public const string Title = "مدينة فاجعة";
        public const string Intro = "كبّ الطابة عالشبابيك! دقّ عالشاشة وين ما بدك تخبط الطابة: تحت الشاشة = الشبابيك التحتانية، فوق = الشبابيك الفوقانية. لما ترجع الطابة من الحيط لازم تنطّ عالأرض مرّة وحدة بس قبل ما تضربها.";
        public const string Play = "يلا نلعب";
        public const string PlayAgain = "عيد اللعب";
        public const string ServeHint = "دقّ وين بدك الطابة تخبط";
        public const string Smashed = "+١  كسرت الشباك!";
        public const string BounceFirst = "خلّيها تنطّ مرّة أول";
        public const string TooFar = "بعيدة عنك";
        public const string Now = "هلّق!";
        public const string IntoFlatBack = "راحت جوّا البيت... صاحب البيت رجّعها!";
        public const string IntoFlat = "راحت جوّا البيت!";
        public const string GameOver = "خلصت اللعبة";
        public const string Victory = "كسّرت زجاج الشارع كلّو!";
        public const string WindCalm = "هادي";
        public const string LifeLost = "(−١ روح)";
        public const string LostTwice = "نطّت مرتين";
        public const string LostShort = "قصيرة، ما وصلت";
        public const string LostStreet = "راحت عالشارع";
        public const string LostKept = "صاحب البيت خلّاها";
        public const string LostPast = "فاتت من عندك";

        static Font font;
        public static Font Font
        {
            get
            {
                if (font == null) font = Resources.Load<Font>("Fonts/Vazirmatn-Regular");
                return font;
            }
        }

        /// <summary>Shaped, reordered, Eastern digits: ready for GUI.Label.</summary>
        public static string T(string s) => ArabicShaper.ForLtrRenderer(ArabicShaper.EasternDigits(s));
        public static string N(int n) => ArabicShaper.EasternDigits(n.ToString());
        public static string House(int oneBased) => "البيت " + N(oneBased);
        public static string Medal(string house) => "ميدالية! ما بقي زجاج بـ" + house;
        public static string Walking(string house) => "عم نمشي عـ" + house + "...";
        public static string Hud1(int glass) => "زجاج: " + N(glass);
        public static string Hud2(int lives, int maxLives, int house, int houses, int broken, int windows)
            => "أرواح: " + new string('♥', Mathf.Max(0, lives)) + new string('♡', Mathf.Max(0, maxLives - lives))
               + "   بيت " + N(house) + "/" + N(houses) + "   شبابيك " + N(broken) + "/" + N(windows);
        public static string Hud3(float wind, int medals)
            => "هوا: " + (Mathf.Abs(wind) < 0.05f ? WindCalm : (wind > 0f ? "← " : "→ ") + ArabicShaper.EasternDigits(Mathf.Abs(wind).ToString("0.0")))
               + "   ميداليات: " + N(medals);
        public static string End(int score, int best, int medals)
            => "زجاج مكسور: " + N(score) + "   أحسن نتيجة: " + N(best) + "   ميداليات: " + N(medals);
    }
}
