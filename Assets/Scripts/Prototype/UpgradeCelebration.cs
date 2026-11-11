using UnityEngine;

namespace Sheytoonak.Prototype
{
    /// <summary>
    /// User (2026-11-11), on the heroes screen: «الان وقتی ارتقاء می‌دی اصن نشون نمیده که ارتقاء دادی. باید مثل پسرخوانده
    /// وقتی ارتقاء میدی یه چهارتا بادکنک و ربان از بالا بیاد و یه موزیکی که نشون بده خوشحالیم». Four balloons and
    /// streamers drift down from the top over a dimmed screen, the tool's icon pops in the middle with «مستوى N!», and a
    /// short happy jingle plays; a tap (or 3.2 s) closes it.
    /// Art when painted: Art/UI/balloon_0..3 and Art/UI/ribbon_0..1; sound Audio/Fx/levelup_01 (prompts in ART_SPEC).
    /// Until then balloons are drawn from a generated disc and the jingle is synthesised.
    /// </summary>
    public class UpgradeCelebration : MonoBehaviour
    {
        public static UpgradeCelebration Instance;
        public const float Seconds = 3.2f;

        string toolId, title;
        float start = -100f;
        AudioSource src;
        AudioClip jingle;
        Texture2D disc;
        readonly Texture2D[] balloons = new Texture2D[4];
        readonly Texture2D[] ribbons = new Texture2D[2];
        static readonly Color[] Hues = { new Color(0.95f, 0.25f, 0.25f), new Color(1f, 0.8f, 0.15f), new Color(0.25f, 0.65f, 1f), new Color(0.35f, 0.85f, 0.4f) };
        // where each balloon comes down (fraction of the width) and how late it starts
        static readonly float[] Xs = { 0.16f, 0.36f, 0.64f, 0.84f };
        static readonly float[] Lag = { 0f, 0.18f, 0.08f, 0.26f };

        public static bool Showing => Instance != null && Time.realtimeSinceStartup - Instance.start < Seconds;

        void Awake()
        {
            Instance = this;
            src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false; src.spatialBlend = 0f;
            jingle = Resources.Load<AudioClip>("Audio/Fx/levelup_01") ?? Jingle();
            for (int i = 0; i < 4; i++) balloons[i] = Resources.Load<Texture2D>("Art/UI/balloon_" + i);
            for (int i = 0; i < 2; i++) ribbons[i] = Resources.Load<Texture2D>("Art/UI/ribbon_" + i);
            disc = Disc(64);
        }

        public void Play(string id, string nameAr, int level)
        {
            toolId = id; title = nameAr + "  " + Ar.Lv(level) + "!";
            start = Time.realtimeSinceStartup;
            if (jingle != null) src.PlayOneShot(jingle, 0.9f);
        }

        void OnGUI()
        {
            float t = Time.realtimeSinceStartup - start;
            if (t >= Seconds || toolId == null) return;
            GUI.depth = -500;
            float sw = Screen.width, sh = Screen.height, u = sh / 900f;
            float fade = t > Seconds - 0.4f ? (Seconds - t) / 0.4f : 1f;
            var old = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.55f * fade * Mathf.Clamp01(t / 0.2f));
            GUI.DrawTexture(new Rect(0, 0, sw, sh), Texture2D.whiteTexture);

            // streamers first, balloons over them
            for (int i = 0; i < 10; i++)
            {
                float lt = Mathf.Max(0f, t - i * 0.07f);
                float x = sw * (0.05f + 0.09f * i) + Mathf.Sin(lt * 3f + i) * 30f * u;
                float y = -120f * u + lt * sh * 0.42f;
                var col = Hues[i % 4];
                var rib = ribbons[i % 2];
                // painted streamers (gold, pink) keep their own colour; the stand-in strips take the balloon colours
                GUI.color = rib != null ? new Color(1f, 1f, 1f, fade) : new Color(col.r, col.g, col.b, fade);
                if (rib != null) { float rh = 190f * u; GUI.DrawTexture(new Rect(x, y, rh * rib.width / rib.height, rh), rib, ScaleMode.StretchToFill); }
                else for (int k = 0; k < 9; k++) // a wavy strip of small squares
                        GUI.DrawTexture(new Rect(x + Mathf.Sin(lt * 6f + k * 0.8f) * 10f * u, y + k * 11f * u, 9 * u, 11 * u), Texture2D.whiteTexture);
            }
            for (int i = 0; i < 4; i++)
            {
                float lt = Mathf.Max(0f, t - Lag[i]);
                float e = 1f - Mathf.Pow(1f - Mathf.Clamp01(lt / 1.6f), 3f); // come down and settle
                float bw = 120f * u, bh = bw * 1.25f;
                float x = sw * Xs[i] - bw * 0.5f + Mathf.Sin(t * 1.8f + i * 1.7f) * 14f * u;
                float y = Mathf.Lerp(-bh * 1.4f, sh * (i % 2 == 0 ? 0.12f : 0.2f), e) + Mathf.Sin(t * 2.4f + i) * 6f * u;
                var tex = balloons[i];
                // the painted balloons (user, 2026-11-11) carry their own curly string: drawn at their own aspect
                if (tex != null) { GUI.color = new Color(1f, 1f, 1f, fade); GUI.DrawTexture(new Rect(x, y, bw, bw * tex.height / tex.width), tex, ScaleMode.StretchToFill); }
                else
                {
                    GUI.color = new Color(1f, 1f, 1f, 0.8f * fade);
                    for (int k = 0; k < 10; k++) GUI.DrawTexture(new Rect(x + bw * 0.5f + Mathf.Sin(t * 3f + k * 0.6f + i) * 4f * u, y + bh + k * 9f * u, 2f * u, 9f * u), Texture2D.whiteTexture);
                    var c = Hues[i];
                    GUI.color = new Color(c.r, c.g, c.b, fade); GUI.DrawTexture(new Rect(x, y, bw, bh), disc);
                    GUI.color = new Color(1f, 1f, 1f, 0.45f * fade); GUI.DrawTexture(new Rect(x + bw * 0.22f, y + bh * 0.14f, bw * 0.22f, bh * 0.2f), disc);
                    GUI.color = new Color(c.r * 0.8f, c.g * 0.8f, c.b * 0.8f, fade); GUI.DrawTexture(new Rect(x + bw * 0.44f, y + bh - 4 * u, bw * 0.12f, 10 * u), disc);
                }
            }

            // the tool's icon pops in the middle with its new level
            float pop = Mathf.Clamp01((t - 0.15f) / 0.35f);
            float s = 170f * u * (pop < 1f ? 0.6f + 0.55f * Mathf.Sin(pop * Mathf.PI * 0.75f) : 1f);
            GUI.color = new Color(1f, 1f, 1f, fade);
            DuelHud.DrawIcon(new Rect((sw - s) * 0.5f, sh * 0.42f - s * 0.5f, s, s), toolId, new Color(1f, 1f, 1f, fade));
            var st = new GUIStyle(GUI.skin.label) { font = Ar.Font, fontSize = Mathf.RoundToInt(46 * u), alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            st.normal.textColor = new Color(0f, 0f, 0f, 0.7f * fade);
            var lr = new Rect(0, sh * 0.42f + 100 * u, sw, 70 * u);
            GUI.Label(new Rect(lr.x + 3 * u, lr.y + 3 * u, lr.width, lr.height), Ar.T(title), st);
            st.normal.textColor = new Color(1f, 0.85f, 0.25f, fade);
            GUI.Label(lr, Ar.T(title), st);
            GUI.color = old;

            // a tap anywhere closes it early
            if (t > 0.6f && Event.current.type == EventType.MouseDown) { start = -100f; Event.current.Use(); }
        }

        static Texture2D Disc(int n)
        {
            var tx = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            float r = n * 0.5f;
            for (int y = 0; y < n; y++) for (int x = 0; x < n; x++)
                {
                    float d = Mathf.Sqrt((x + 0.5f - r) * (x + 0.5f - r) + (y + 0.5f - r) * (y + 0.5f - r));
                    tx.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(r - d)));
                }
            tx.Apply();
            return tx;
        }

        /// <summary>A bright little fanfare (C major arpeggio and a held chord) until a recorded one is dropped in.</summary>
        static AudioClip Jingle()
        {
            const int rate = 22050; float len = 1.6f;
            int n = (int)(rate * len);
            var data = new float[n];
            float[] notes = { 523.25f, 659.25f, 783.99f, 1046.5f };
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)rate, v = 0f;
                for (int k = 0; k < notes.Length; k++)
                {
                    float on = k * 0.11f;
                    if (t < on) continue;
                    float lt = t - on;
                    float env = Mathf.Clamp01(lt / 0.01f) * Mathf.Exp(-lt * (k == notes.Length - 1 ? 1.6f : 3.5f));
                    v += (Mathf.Sin(2f * Mathf.PI * notes[k] * t) + 0.25f * Mathf.Sin(4f * Mathf.PI * notes[k] * t)) * env;
                }
                data[i] = v * 0.22f;
            }
            var c = AudioClip.Create("levelup", n, 1, rate, false);
            c.SetData(data, 0);
            return c;
        }
    }
}
