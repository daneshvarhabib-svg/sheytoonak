using System.Collections.Generic;
using UnityEngine;
using Sheytoonak.Core.Duel;

namespace Sheytoonak
{
    /// <summary>
    /// A voice per hero and two shared tool sounds (user 2026-11-07: «هر بازیکن یه صدایی داشته باشه که موقع زدن
    /// ضربه یا سم اون صدا تولید شه … یه صدای خاص برای زدن ضربه، یه صدای عمومی موقع استفاده از سم و یه صدای
    /// عمومی موقع پادزهر»). Clips are picked up by name from Resources:
    ///   Audio/Heroes/&lt;heroId&gt;/kick*.ogg   — that hero's grunt or line when its kick launches (any count);
    ///   Audio/Fx/poison*.ogg                — any side places a poison;
    ///   Audio/Fx/antidote*.ogg              — any side uses an antidote.
    /// Several files per folder are chosen at random, never the same twice in a row; a missing folder is silence.
    /// </summary>
    public class HeroVoice : MonoBehaviour
    {
        [Range(0f, 1f)] public float volume = 1f;

        DuelManager duel;
        DuelRules hooked;
        AudioSource src;
        readonly Dictionary<string, AudioClip[]> kicks = new Dictionary<string, AudioClip[]>();
        AudioClip[] poison, antidote;
        readonly Dictionary<AudioClip[], int> last = new Dictionary<AudioClip[], int>();
        readonly int[] slot = { -1, -1 }; // the hero slot each side last sent to the ball

        void Awake()
        {
            src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false; src.spatialBlend = 0f;
            duel = GetComponent<DuelManager>();
            poison = Load("Audio/Fx", "poison");
            antidote = Load("Audio/Fx", "antidote");
        }

        static AudioClip[] Load(string folder, string prefix)
        {
            var all = Resources.LoadAll<AudioClip>(folder);
            var list = new List<AudioClip>();
            foreach (var c in all) if (c != null && c.name.StartsWith(prefix)) list.Add(c);
            return list.ToArray();
        }

        void OnEnable()
        {
            if (duel == null) return;
            duel.DuelStarted += OnDuel;
            duel.KickerChosen += OnKicker;
            duel.KickLaunched += OnKick;
        }

        void OnDisable()
        {
            if (duel == null) return;
            duel.DuelStarted -= OnDuel;
            duel.KickerChosen -= OnKicker;
            duel.KickLaunched -= OnKick;
            Unhook();
        }

        void OnDuel()
        {
            Unhook();
            hooked = duel.Rules;
            if (hooked == null) return;
            hooked.PoisonPlaced += OnPoison;
            hooked.AntidoteUsed += OnAntidote;
            slot[0] = slot[1] = -1;
            foreach (var side in hooked.Heroes) foreach (var h in side)
                if (h.Id != null && !kicks.ContainsKey(h.Id)) kicks[h.Id] = Load("Audio/Heroes/" + h.Id, "kick");
        }

        void Unhook()
        {
            if (hooked == null) return;
            hooked.PoisonPlaced -= OnPoison;
            hooked.AntidoteUsed -= OnAntidote;
            hooked = null;
        }

        void OnKicker(Side s, int hero) => slot[(int)s] = hero;

        void OnKick(Side s, int target)
        {
            var r = duel.Rules; if (r == null) return;
            int i = s == duel.Me && duel.Selected >= 0 ? duel.Selected : slot[(int)s];
            var heroes = r.Heroes[(int)s];
            if (i < 0 || i >= heroes.Length || heroes[i].Id == null) return;
            if (kicks.TryGetValue(heroes[i].Id, out var set)) Play(set, Random.Range(0.96f, 1.04f));
        }

        void OnPoison(Side s, ActivePoison p) => Play(poison, 1f);
        void OnAntidote(Side s, Tool t) => Play(antidote, 1f);

        void Play(AudioClip[] set, float pitch)
        {
            if (set == null || set.Length == 0 || src == null) return;
            int k = set.Length == 1 ? 0 : Random.Range(0, set.Length - 1);
            if (set.Length > 1 && last.TryGetValue(set, out var prev) && k >= prev) k++;
            last[set] = k;
            src.pitch = pitch;
            src.PlayOneShot(set[k], volume);
        }
    }
}
