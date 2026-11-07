using System;

namespace Sheytoonak.Core.Duel
{
    /// <summary>The four things a poison can do (docs/SCENARIO.fa.md §5): 50 poisons are 50 costumes on these.</summary>
    public enum Effect
    {
        /// <summary>Scatters the kick: extra degrees of error (wind, bees, grandmother shouting).</summary>
        Accuracy,
        /// <summary>The ball may not come back / the kick is weak (puddle, dog, wet ball).</summary>
        Reach,
        /// <summary>Some windows are hidden behind something for a while (bus, scaffold, banner).</summary>
        Cover,
        /// <summary>One window is shut and cannot break (neighbour opens it, pot on the sill).</summary>
        Close,
        /// <summary>Something crosses the path and may intercept the ball (pigeon, kite string).</summary>
        Chance
    }

    /// <summary>How long a poison stays on the shared stage. Part of its strength.</summary>
    public enum Lifetime { OneTurn = 1, TwoTurns = 2, UntilAntidote = 99 }

    /// <summary>One poison or antidote, as a hero carries it. Power 1..3.</summary>
    [Serializable]
    public struct Tool
    {
        public string Id;       // ascii key for art/sound lookup, e.g. "pigeon"
        public string NameAr;   // shown in the HUD
        public Effect Effect;
        public int Power;
        public Lifetime Life;   // poisons only
        public Tool(string id, string nameAr, Effect effect, int power, Lifetime life = Lifetime.OneTurn)
        { Id = id; NameAr = nameAr; Effect = effect; Power = power; Life = life; }
    }

    [Serializable]
    public struct HeroDef
    {
        public string Id;
        public string NameAr;
        public Tool Poison;
        public Tool Antidote;
        public HeroDef(string id, string nameAr, Tool poison, Tool antidote) { Id = id; NameAr = nameAr; Poison = poison; Antidote = antidote; }
    }

    /// <summary>
    /// The five heroes every player starts with (docs/HEROES.fa.md ★). A closed ring: each one's antidote
    /// answers the next one's poison, so the first draft already has meaningful picks.
    /// </summary>
    public static class StarterHeroes
    {
        public static readonly HeroDef[] All =
        {
            new HeroDef("abu_shahata", "أبو شحّاطة البيروتي",
                new Tool("pigeon", "حمامة", Effect.Chance, 1, Lifetime.OneTurn),
                new Tool("nails", "مسامير عالطريق", Effect.Cover, 1)),
            new HeroDef("zaatar", "زعتر الشامي",
                new Tool("bus", "باص", Effect.Cover, 1, Lifetime.OneTurn) /* two turns until the human test (review 2026-11-07 §2.4) */,
                new Tool("vane", "دوّارة هوا", Effect.Accuracy, 1)),
            new HeroDef("fulla", "فُلّة الإسكندرانية",
                new Tool("wind", "هوا قوي", Effect.Accuracy, 1, Lifetime.OneTurn),
                new Tool("pebble", "حصوة عالشباك", Effect.Close, 1)),
            new HeroDef("rima", "ريما الشرشوحة الصيداوية",
                // ⚠️ until 2026-11-04 the neighbour stayed until an antidote (pebble). User: «اون سه تا شیشه‌ای که بازه چیه؟ … نباید بعد از
                // یکی دو نوبت خودش بسته شه؟» -- with every other window broken the duel could not end. Now she shuts it after two turns.
                new Tool("open_window", "الجار فتح الشباك", Effect.Close, 1, Lifetime.OneTurn) /* two turns until the human test (review 2026-11-07 §2.4) */,
                new Tool("plank", "لوح عالجورة", Effect.Reach, 1)),
            new HeroDef("toni", "طوني الزعران البيروتي",
                new Tool("puddle", "جورة مي", Effect.Reach, 1, Lifetime.OneTurn),
                new Tool("fart", "ضرطة", Effect.Chance, 1)),
            // ⚠️ heroes 4, 5 and 7 of docs/HEROES.fa.md (2026-11-04): grown-ups after the five kids (user: «باقی قهرمانان
            // همه بچه نباشن … تا پیرمرد و پیرزن»). Their poisons reuse the stage props of the same effect until their
            // own art arrives (painted the same day: lorry, pickle cart, rain cloud, complaint, umbrella, flashlight). Um al-Arabiyat kicks with her left foot.
            new HeroDef("um_arabiyat", "أم العربيات البغدادية",
                new Tool("truck", "شاحنة عالية", Effect.Cover, 1, Lifetime.OneTurn) /* two turns until the human test (review 2026-11-07 §2.4) */,
                new Tool("complaint", "شكوى للبلدية", Effect.Cover, 1)),
            new HeroDef("hamada", "حمادة الطرشجي المصراوي",
                new Tool("cart", "عربية البياع", Effect.Cover, 1, Lifetime.OneTurn),
                new Tool("umbrella", "شمسية", Effect.Reach, 1)),
            new HeroDef("bubakr", "بوبكر الصيّاد التونسي",
                new Tool("rain", "مطر", Effect.Reach, 1, Lifetime.OneTurn) /* two turns until the human test (review 2026-11-07 §2.4) */,
                new Tool("flashlight", "ضو بطارية", Effect.Accuracy, 1)),
        };

        public static HeroDef Get(string id)
        {
            foreach (var h in All) if (h.Id == id) return h;
            // a blind ban can name a hero this build does not know; it is never fielded, so a stub is enough
            return new HeroDef(id, id, new Tool(id, id, Effect.Chance, 1), new Tool(id, id, Effect.Chance, 1));
        }
    }
}
