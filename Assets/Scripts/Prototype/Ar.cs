using System;
using Sheytoonak.Core;
using UnityEngine;

namespace Sheytoonak
{
    /// <summary>
    /// The game's UI language is spoken Levantine Arabic (user, 2026-10-27: "زبان بازی باید عربی محاوره باشه").
    /// ⚠️ Since 2026-11-19 it is simple modern standard Arabic instead (user 2026-11-17, after the game turned medieval: «زبان جملات ...
    /// عربی فصیح بشه. اینطوری چالش لهجه هم حل میشه. ولی باید جملاتی از فصحی برداریم که الآن هم شبیهش استفاده می‌شه و خیلی نامفهوم
    /// نباشه»): everyday words every Arab reader knows, no dialect, nothing bookish.
    /// IMGUI cannot shape Arabic, so every string goes through <see cref="T"/> (contextual forms +
    /// visual order) and is drawn with the bundled Vazirmatn font (OFL), which has the presentation glyphs.
    /// </summary>
    public static class Ar
    {
        // ---- duel (docs/SCENARIO.fa.md)
        public const string DuelIntro = "مبارزة النوافذ: لكلٍّ دوره، وفي الدور حركتان. نافذة سفلى = ١، عليا = ٢، وهناك نافذة مخفيّة = ٣. من يكسر أكثر من النصف يفوز.";
        public const string YourTurn = "دورك";
        public const string TheirTurn = "دور الخصم";
        public const string Actions = "حركات";
        public const string Rage = "غضب";
        public const string RageReady = "أنت غاضب: حركة إضافيّة!";
        public const string RageUsed = "غضب! حركة إضافيّة";
        public const string PassTurn = "انتهى دوري";
        public const string Surrender = "أستسلم";
        public const string DraftTitle = "اختر فريقك";
        public const string DraftBanYou = "استبعد بطلاً (يُستبعد عن الجميع)";
        public const string DraftPickYou = "اختر بطلاً";
        public const string DraftWait = "الخصم يختار...";
        public const string DraftYourPick = "اختيارك";
        public const string DraftTheirPick = "اختيار الخصم";
        public const string DraftYouBanned = "استبعدتَ";
        public const string DraftTheyBanned = "الخصم استبعد";
        public const string YouBan = "استبعدتَ:";
        public const string Sealed = "لا تنكسر!"; // the sealed window took the ball (user: one window per round never breaks)
        public const string HitsLeft = "ضربات باقية"; // hits left on this ball (user: three per ball)
        public const string TheyBan = "الخصم استبعد:";
        public const string Banned = "مستبعَد";
        public const string Mine = "لك";
        public const string TheirPicks = "فريق الخصم";
        public const string SurrenderSure = "متأكّد؟ اضغط مرّة أخرى";
        public const string NoActions = "لم تبقَ حركات";
        public const string TimeUp = "انتهى الوقت";
        public const string HiddenThree = "هذه النافذة المخفيّة: ٣!";
        public const string Intercepted = "اصطدمت بشيء في الهواء!";
        public const string Covered = "مغطّاة!";
        public const string Shut = "مغلقة!";
        public const string AlreadyBroken = "مكسورة من قبل";
        public const string Missed = "أخطأت";
        public const string YouPoison = "رميتَ:";
        public const string TheyPoison = "الخصم رمى:";
        public const string YouAntidote = "ترياقك:";
        public const string TheyAntidote = "ترياق الخصم:";
        public const string PoisonWord = "سمّ";
        public const string AntidoteWord = "ترياق";
        public const string Won = "فزت!";
        public const string Lost = "خسرت";
        public const string Draw = "تعادل";
        public static string WindowsLine(int mine, int theirs, int majority) => "نوافذ " + N(mine) + " – " + N(theirs) + "  (الفوز من " + N(majority) + ")";
        public static string Reward(int shards, int curseDelta, int curses) => "زجاج: +" + N(shards) + "   شتائم: " + (curseDelta >= 0 ? "+" : "−") + N(Math.Abs(curseDelta)) + " (" + N(curses) + ")";
        public static string Timer(int seconds) => N(seconds) + " ث";
        public const string Title = "مدينة الفاجعة";
        public const string Intro = "اركل الكرة نحو النوافذ! ضع إصبعك قرب الكرة واسحب نحو النافذة ثم ارفعه: كلّما سحبت أطول ذهبت أقوى وأعلى. حين تعود الكرة من الجدار يمكنك أن تضربها ثلاث مرّات أخرى، قبل أن ترتدّ عن الأرض مرّتين.";
        public const string Play = "هيّا نلعب";
        public const string PlayAgain = "العب ثانية";
        public const string ServeHint = "اسحب من الكرة نحو النافذة"; // 2026-11-09: the kick is a swipe, not a tap
        public const string VibrateOn = "الاهتزاز: نعم";
        public const string NothingToClear = "لا شيء لتزيله";
        /// <summary>The antidote is two levels or more below the poison (SCENARIO §8).</summary>
        public const string TooWeak = "الترياق أضعف من السمّ";
        // user 2026-11-19: «تیله خوبه»: the second currency is glass marbles (whole glass, against the shards of broken glass), not gold coins.
        // ⚠️ the code still says Lira everywhere (saves, ShopCatalog): only the word and the picture changed.
        public const string Lira = "بِلّي";
        public const string NotEnoughLira = "ليس معك بِلّي كافٍ";
        public const string PaySoon = "الدفع قريباً";
        public const string CodeWrong = "الرمز خطأ أو مستعمل";
        public const string Upgrade = "قوِّ";
        public const string Heroes = "الأبطال";
        public const string MaxLevel = "الأقوى";
        public static string Lv(int l) => "مستوى " + N(l);
        public const string VibrateOff = "الاهتزاز: لا";
        public const string NotifyOn = "التنبيه: نعم";
        public const string NotifyOff = "التنبيه: لا";
        public const string Smashed = "+١  كسرت النافذة!";
        public const string Invite = "ادعُ صديقك";
        public const string InviteText = "تعال نكسر النوافذ معاً في مدينة الفاجعة! افتح الرابط وخذ ٥٠٠ زجاج هديّة.";
        public static string InviteGot(int n) => "هدية من صاحبك: +" + N(n) + " زجاج";
        public const string Story = "انشرها في القصّة";
        public const string StoryText = "كسرت نوافذ الحارة كلّها! تعال جرّب.";
        public const string StoryOld = "حدّث تلغرام لتنشر في القصّة";
        public const string MapTitle = "خريطة المدينة";
        public const string MapButton = "الخريطة";
        public const string MapQuarters = "حارات مفتوحة: ";
        public const string MapPrizeReady = "جائزتك جاهزة! اضغط لتأخذها";
        public const string MapHouses = "بيوت مهدومة: ";
        public const string MapHow = "كلّ فوز يضيء بيتاً. الخسارة ترجعك";
        public const string MapWhole = "المدينة كلّها لك!";
        public const string Searching = "ندقّ الطبل... نبحث عن خصم";
        public const string Found = "وُجد خصم!";
        public const string ChallengeTitle = "التحدّي";
        public const string ChallengeHow = "١٢ فوزاً تملأ الحلقات. ٣ خسارات تنهيه. كلّ فوز يكبّر الجائزة";
        public const string ChallengeStart = "ابدأ";
        public const string ChallengeGo = "تابع";
        public const string ChallengeFreeOnce = "الأولى مجّاناً";
        /// <summary>What a window antidote works against, said when it is pressed with nothing of its kind on the wall.</summary>
        public static string AntidoteIsFor(Sheytoonak.Core.Duel.Effect e) =>
            e == Sheytoonak.Core.Duel.Effect.Close ? "هذه تُسقط السجّادة فقط" :
            e == Sheytoonak.Core.Duel.Effect.Cover ? "هذه تُبعد العربات والقوافل فقط" : NothingToClear;
        public const string GatePassed = "عبرت الباب! صار لك: ";
        public const string MapNext = "التالي: ";
        public const string JarFree = "مجّاناً";
        public const string Settings = "الإعدادات";
        public const string Close = "إغلاق";
        /// <summary>The ten districts of the city, five quarters each (research p50). Invented names of trades and places; no
        /// real city, no religious name. ⚠️ Proposed 2026-11-19, the owner still has to approve the list.</summary>
        static readonly string[] Districts = { "سوق العطّارين", "حارة الدبّاغين", "باب النهر", "درب النحّاسين", "حارة الخبّازين", "سوق الصاغة", "درب القوافل", "حارة البساتين", "ساحة البئر", "القلعة العالية" };
        public static string District(int i) => Districts[System.Math.Max(0, System.Math.Min(Districts.Length - 1, i))];
        public const string JarTitle = "الحصّالة";
        public const string JarNotFull = "الحصّالة لم تمتلئ بعد";
        public const string JarHow = "كلّ فوز يزيدها. إذا امتلأت خُذها بنصف السعر";
        public const string JarHalf = "خصم ٥٠٪";
        public const string MissionsTitle = "مهامّ اليوم";
        public const string MissionDone = "أنجزت مهمّة!";
        public const string MissionTake = "خُذ";
        public const string MissionTaken = "أخذتها";
        public const string MissionAll = "الثلاثة كلّها";
        public const string MissionSwap = "بدّل";
        public const string MissionSwapFree = "بدّل مجّاناً";
        /// <summary>Text of one job of the day; {0} is its goal.</summary>
        public static string MissionText(Sheytoonak.Core.Missions.MissionKind k, int goal)
        {
            string n = goal.ToString();
            switch (k)
            {
                case Sheytoonak.Core.Missions.MissionKind.PlayDuels: return "العب " + n + " مبارزات";
                case Sheytoonak.Core.Missions.MissionKind.WinDuels: return "اربح " + n + " مبارزات";
                case Sheytoonak.Core.Missions.MissionKind.BreakWindows: return "اكسر " + n + " نوافذ";
                case Sheytoonak.Core.Missions.MissionKind.UsePoisons: return "ارمِ " + n + " سموم";
                case Sheytoonak.Core.Missions.MissionKind.UseAntidotes: return "استعمل " + n + " ترياق";
                default: return "افتح " + n + " كيس";
            }
        }
        public const string ComboTitle = "تركيبة اليوم";
        public const string ComboDone = "أخذتها اليوم";
        public const string ComboWon = "تركيبة اليوم! +";
        public const string DoubleWindow = "+٢  نافذة الشتائم!"; // a window worth 2: the resident leans out and curses
        public const string BounceFirst = "دعها ترتدّ مرّة أوّلاً";
        public const string TooFar = "بعيدة عنك";
        public const string Now = "الآن!";
        public const string IntoFlatBack = "دخلت البيت... صاحب البيت أعادها!";
        public const string IntoFlat = "دخلت البيت!";
        public const string GameOver = "انتهت اللعبة";
        public const string Victory = "كسرت زجاج الشارع كلّه!";
        public const string WindCalm = "هادئة";
        public const string LifeLost = "(−١ روح)";
        public const string LostTwice = "ارتدّت مرّتين";
        public const string LostShort = "قصيرة، لم تصل";
        public const string LostStreet = "ذهبت إلى الشارع";
        public const string LostKept = "صاحب البيت احتفظ بها";
        public const string LostPast = "فاتتك";

        static Font font;
        public static Font Font
        {
            get
            {
                if (font == null)
                {
                    font = Resources.Load<Font>("Fonts/Vazirmatn-Regular");
                    if (font != null) { UnityEngine.Font.textureRebuilt += OnRebuilt; Warm(); }
                }
                return font;
            }
        }

        // ⚠️ run 31/32: final ر/ى/و/ص came out as a stray mark in the poison list. The dynamic font atlas rebuilds
        // mid-frame when it meets a new glyph/size, and text drawn earlier that frame keeps the old UVs. Put every
        // glyph we can emit into the atlas at the sizes the HUD uses, once, and again after any rebuild.
        static readonly int[] warmSizes = { 16, 19, 22, 24, 28, 32, 36, 40, 48 };
        static bool warming;
        static void Warm()
        {
            if (font == null || warming) return;
            warming = true;
            string all = ArabicShaper.AllGlyphs();
            float u = Ar.UiScale(Screen.height);
            foreach (int s in warmSizes)
            {
                int px = Mathf.RoundToInt(s * u);
                font.RequestCharactersInTexture(all, px, FontStyle.Normal);
                font.RequestCharactersInTexture(all, px, FontStyle.Bold);
                if (px != s) font.RequestCharactersInTexture(all, s, FontStyle.Normal);
            }
            warming = false;
        }
        static void OnRebuilt(UnityEngine.Font f) { if (f == font) Warm(); }

        /// <summary>
        /// The HUD is laid out for a 900 px tall screen and never shrank below that (Max(1, h/900)). ⚠️ In Telegram (web)
        /// the game gets only a few hundred pixels under Telegram's bar and the lobby's bottom row (the play buttons) fell
        /// off the screen (user 2026-11-15: «دکمه‌ها درست نشون داده نمیشه»), and the same day: «اندرویدم ها» -- the phone
        /// app too (a screen under 900 px, or rendered smaller than the panel). Now it shrinks with every screen.
        /// </summary>
        public static float UiScale(float screenHeight) => Mathf.Max(0.35f, screenHeight / 900f);

        /// <summary>Shaped, reordered, Eastern digits: ready for GUI.Label.</summary>
        public static string T(string s) => ArabicShaper.ForLtrRenderer(ArabicShaper.EasternDigits(s));
        public static string N(int n) => ArabicShaper.EasternDigits(n.ToString());
        public static string House(int oneBased) => "البيت " + N(oneBased);
        public static string Medal(string house) => "وسام! لم يبقَ زجاج في " + house;
        public static string Walking(string house) => "نمشي إلى " + house + "...";
        public static string Hud1(int glass) => "زجاج: " + N(glass);
        public static string Hud2(int lives, int maxLives, int house, int houses, int broken, int windows)
            => "أرواح: " + new string('♥', Mathf.Max(0, lives)) + new string('♡', Mathf.Max(0, maxLives - lives))
               + "   بيت " + N(house) + "/" + N(houses) + "   نوافذ " + N(broken) + "/" + N(windows);
        public static string Hud3(float wind, int medals)
            => "الريح: " + (Mathf.Abs(wind) < 0.05f ? WindCalm : (wind > 0f ? "← " : "→ ") + ArabicShaper.EasternDigits(Mathf.Abs(wind).ToString("0.0")))
               + "   أوسمة: " + N(medals);
        public static string End(int score, int best, int medals)
            => "زجاج مكسور: " + N(score) + "   أفضل نتيجة: " + N(best) + "   أوسمة: " + N(medals);
    }
}
