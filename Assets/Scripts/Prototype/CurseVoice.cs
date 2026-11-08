using System.Collections;
using UnityEngine;

namespace Sheytoonak
{
    /// <summary>
    /// Every broken window gets an angry (clean) shout from inside the flat, in Levantine Arabic
    /// (user, 2026-10-27). Recorded clips go in Resources/Audio/Curses (any name, .ogg/.wav/.mp3);
    /// the lines to record are listed in docs/ART_SPEC.fa.md. Until they exist a synthesised
    /// grumble stands in, so the timing and volume can be judged on the phone.
    /// </summary>
    public class CurseVoice : MonoBehaviour
    {
        [Range(0f, 1f)] public float volume = 1f;
        public float delaySeconds = 0.45f;

        AudioClip[] clips = new AudioClip[0];
        AudioClip placeholder;
        AudioSource src;
        int last = -1;

        void Awake()
        {
            src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            clips = Resources.LoadAll<AudioClip>("Audio/Curses") ?? new AudioClip[0];
            if (clips.Length == 0) placeholder = Grumble();
        }

        // User (2026-11-08): «با شکستن تک تک پنجره‌ها فحش رو نداشته باشیم» -- the curse is the resident's line, so it plays
        // only when one leans out (every few windows, PrototypeBootstrap.OnWindowBroken), not on every break.
        void OnEnable() => GameEvents.ResidentShouts += OnBroken;
        void OnDisable() => GameEvents.ResidentShouts -= OnBroken;

        void OnBroken(int i, Vector3 point) => StartCoroutine(Shout(ResidentIsFemale));

        /// <summary>Set by whoever shows the resident just before ResidentShouts (PrototypeBootstrap). User (2026-11-08):
        /// «شخصیت‌هایی که فحش می‌دن باید صداهاشون مچ باشه با سیماشون؛ تصویر زن رو صدای مرد نشینه و بالعکس».
        /// Clips: curse_m_* (Ahmed) and curse_f_* (Tara).</summary>
        public static bool ResidentIsFemale;
        int lastM = -1, lastF = -1;

        IEnumerator Shout(bool female)
        {
            yield return new WaitForSeconds(delaySeconds);
            AudioClip c = null;
            var pool = new System.Collections.Generic.List<AudioClip>();
            foreach (var a in clips) if (a.name.StartsWith(female ? "curse_f_" : "curse_m_")) pool.Add(a);
            if (pool.Count == 0) foreach (var a in clips) pool.Add(a); // no clip of that voice: any line beats silence
            if (pool.Count > 0)
            {
                int lastK = female ? lastF : lastM;
                int k = pool.Count == 1 ? 0 : Random.Range(0, pool.Count - 1);
                if (k >= lastK && pool.Count > 1) k++; // never the same line twice in a row
                if (female) lastF = k; else lastM = k;
                c = pool[k];
            }
            else c = placeholder;
            if (c == null) yield break;
            // 2026-11-08: ten ElevenLabs v3 lines, two real speakers (Ahmed, Tara) with acted anger, so the wide pitch
            // spread that faked "several neighbours" out of one flat voice is gone; a hair of variation only.
            src.pitch = Random.Range(0.97f, 1.03f);
            src.PlayOneShot(c, volume);
        }

        /// <summary>Two gruff syllables: a muffled shout through a wall.</summary>
        static AudioClip Grumble()
        {
            const int rate = 22050; const float seconds = 0.7f;
            int n = (int)(rate * seconds);
            var d = new float[n];
            var rng = new System.Random(3);
            float lp = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)rate, k = t / seconds;
                float env = k < 0.45f ? Mathf.Sin(Mathf.PI * k / 0.45f) : Mathf.Sin(Mathf.PI * (k - 0.45f) / 0.55f) * 0.8f;
                float f = 130f + 40f * Mathf.Sin(k * 9f);
                float voice = Mathf.Sin(2f * Mathf.PI * f * t) + 0.5f * Mathf.Sin(2f * Mathf.PI * f * 2f * t) + 0.3f * Mathf.Sin(2f * Mathf.PI * f * 3f * t);
                float noise = (float)(rng.NextDouble() * 2.0 - 1.0);
                lp += (noise - lp) * 0.05f;
                d[i] = Mathf.Clamp((voice * 0.35f + lp * 0.6f) * Mathf.Max(0f, env), -1f, 1f);
            }
            var clip = AudioClip.Create("grumble", n, 1, rate, false);
            clip.SetData(d, 0);
            return clip;
        }
    }
}
