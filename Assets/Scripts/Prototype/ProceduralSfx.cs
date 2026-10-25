using System;
using UnityEngine;

namespace Sheytoonak
{
    /// <summary>
    /// Sound without sound files: short clips synthesised at start-up (sine thumps, noise bursts, a
    /// two-note chime) and played on game events. User, 2026-10-25: "بازی هنوز هم صدا نداره". Real
    /// recorded sounds can replace these later by assigning clips in the inspector; code only fills
    /// the ones left empty.
    /// </summary>
    public class ProceduralSfx : MonoBehaviour
    {
        public AudioClip kick, bounce, wall, glass, swallow, throwBack, lifeLost, medal, win, lose;
        [Range(0f, 1f)] public float volume = 0.9f;

        const int Rate = 22050;
        AudioSource src;

        void Awake()
        {
            src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.spatialBlend = 0f;
            kick = kick ? kick : Thump("kick", 0.12f, 140f, 60f, 1f);
            bounce = bounce ? bounce : Thump("bounce", 0.09f, 110f, 50f, 0.6f);
            wall = wall ? wall : Thump("wall", 0.14f, 90f, 40f, 0.8f);
            glass = glass ? glass : Shatter("glass", 0.5f);
            swallow = swallow ? swallow : Thump("swallow", 0.2f, 70f, 30f, 0.5f);
            throwBack = throwBack ? throwBack : Whoosh("throw", 0.35f);
            lifeLost = lifeLost ? lifeLost : Tone("life", 0.45f, new[] { 220f, 165f }, 0.22f);
            medal = medal ? medal : Tone("medal", 0.7f, new[] { 523f, 659f, 784f, 1047f }, 0.17f);
            win = win ? win : Tone("win", 1.2f, new[] { 523f, 659f, 784f, 1047f, 1319f }, 0.24f);
            lose = lose ? lose : Tone("lose", 0.9f, new[] { 330f, 262f, 196f }, 0.3f);
        }

        void OnEnable()
        {
            GameEvents.Kicked += OnKicked;
            GameEvents.Bounced += OnBounced;
            GameEvents.WallHit += OnWall;
            GameEvents.WindowBroken += OnGlass;
            GameEvents.BallInHouse += OnInHouse;
            GameEvents.OwnerThrow += OnThrow;
            GameEvents.LifeLost += OnLife;
            GameEvents.MedalEarned += OnMedal;
            GameEvents.GameOver += OnGameOver;
            GameEvents.Victory += OnVictory;
        }

        void OnDisable()
        {
            GameEvents.Kicked -= OnKicked;
            GameEvents.Bounced -= OnBounced;
            GameEvents.WallHit -= OnWall;
            GameEvents.WindowBroken -= OnGlass;
            GameEvents.BallInHouse -= OnInHouse;
            GameEvents.OwnerThrow -= OnThrow;
            GameEvents.LifeLost -= OnLife;
            GameEvents.MedalEarned -= OnMedal;
            GameEvents.GameOver -= OnGameOver;
            GameEvents.Victory -= OnVictory;
        }

        void Play(AudioClip c, float vol = 1f, float pitch = 1f)
        {
            if (c == null || src == null) return;
            src.pitch = pitch;
            src.PlayOneShot(c, volume * vol);
        }

        void OnKicked(Sheytoonak.Core.KickGesture g, float quality, Vector3 v) => Play(kick, 0.7f + 0.3f * g.Strength01, 0.95f + 0.15f * g.Strength01);
        void OnBounced(Vector3 p, int n, bool returning) => Play(bounce, 0.8f, UnityEngine.Random.Range(0.95f, 1.05f));
        void OnWall(Vector3 p) => Play(wall);
        void OnGlass(int i, Vector3 p) => Play(glass, 1f, UnityEngine.Random.Range(0.9f, 1.1f));
        void OnInHouse(int i, bool back) => Play(swallow);
        void OnThrow(int i) => Play(throwBack);
        void OnLife(int left, Sheytoonak.Core.LossReason r) => Play(lifeLost);
        void OnMedal(int m, HouseDefinition h) => Play(medal);
        void OnGameOver(int s, int b, int m) => Play(lose);
        void OnVictory(int s, int b, int m) => Play(win);

        // ------------------------------------------------------------------ synthesis

        static AudioClip Make(string name, float seconds, Func<float, float> f)
        {
            int n = Mathf.Max(1, (int)(Rate * seconds));
            var data = new float[n];
            for (int i = 0; i < n; i++) data[i] = Mathf.Clamp(f(i / (float)Rate), -1f, 1f);
            var clip = AudioClip.Create(name, n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>A pitch-dropping sine with a fast decay: a ball being struck or landing.</summary>
        static AudioClip Thump(string name, float seconds, float f0, float f1, float amp)
        {
            return Make(name, seconds, t =>
            {
                float k = t / seconds;
                float freq = Mathf.Lerp(f0, f1, k);
                float env = Mathf.Exp(-k * 9f) * amp;
                return Mathf.Sin(2f * Mathf.PI * freq * t) * env;
            });
        }

        /// <summary>Bright noise burst with a few ringing partials: breaking glass.</summary>
        static AudioClip Shatter(string name, float seconds)
        {
            var rng = new System.Random(7);
            float prev = 0f;
            return Make(name, seconds, t =>
            {
                float k = t / seconds;
                float white = (float)(rng.NextDouble() * 2.0 - 1.0);
                float hp = white - prev * 0.6f; prev = white; // crude high-pass: glass is bright
                float ring = 0.25f * (Mathf.Sin(2f * Mathf.PI * 3100f * t) + Mathf.Sin(2f * Mathf.PI * 4700f * t) * 0.6f);
                float env = Mathf.Exp(-k * 6f) * (0.3f + 0.7f * Mathf.Exp(-k * 25f));
                return (hp * 0.8f + ring) * env;
            });
        }

        /// <summary>Filtered noise swelling and fading: something thrown through the air.</summary>
        static AudioClip Whoosh(string name, float seconds)
        {
            var rng = new System.Random(11);
            float lp = 0f;
            return Make(name, seconds, t =>
            {
                float k = t / seconds;
                float white = (float)(rng.NextDouble() * 2.0 - 1.0);
                lp += (white - lp) * 0.08f;
                float env = Mathf.Sin(Mathf.PI * k);
                return lp * 2.2f * env;
            });
        }

        /// <summary>Notes played one after another with a soft envelope: jingles.</summary>
        static AudioClip Tone(string name, float seconds, float[] notes, float noteLen)
        {
            return Make(name, seconds, t =>
            {
                int i = Mathf.Min(notes.Length - 1, (int)(t / noteLen));
                float lt = t - i * noteLen;
                float env = Mathf.Clamp01(lt / 0.01f) * Mathf.Exp(-lt * 5f);
                float f = notes[i];
                return (Mathf.Sin(2f * Mathf.PI * f * t) + 0.3f * Mathf.Sin(2f * Mathf.PI * f * 2f * t)) * 0.4f * env;
            });
        }
    }
}
