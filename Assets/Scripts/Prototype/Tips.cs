using Sheytoonak.Core;
using UnityEngine;

namespace Sheytoonak
{
    /// <summary>
    /// The subtitle of «how to play» lines (Resources/Text/tips.txt, see <see cref="TipDeck"/>). Unity shows it while the
    /// opponent picks in the draft; the web page shows the same file under its loading bar (WebGLTemplates/Telegram).
    /// A line stays <see cref="Seconds"/> so it can be read, then the next one comes.
    /// </summary>
    public static class Tips
    {
        public const float Seconds = 6f;
        static TipDeck deck;
        static string current = "";
        static float shownAt = -999f;

        public static string Current
        {
            get
            {
                if (deck == null)
                {
                    var file = Resources.Load<TextAsset>("Text/tips");
                    deck = TipDeck.FromText(file != null ? file.text : "");
                }
                if (Time.realtimeSinceStartup - shownAt >= Seconds) { current = deck.Next(); shownAt = Time.realtimeSinceStartup; }
                return current;
            }
        }

        /// <summary>One line in a dark band, inside <paramref name="r"/>.</summary>
        public static void Subtitle(Rect r, float u, Font font)
        {
            string tip = Current;
            if (string.IsNullOrEmpty(tip)) return;
            var o = GUI.color; GUI.color = new Color(0f, 0f, 0f, 0.55f); GUI.DrawTexture(r, Texture2D.whiteTexture); GUI.color = o;
            var st = new GUIStyle(GUI.skin.label) { font = font, fontSize = Mathf.RoundToInt(19 * u), alignment = TextAnchor.MiddleCenter, wordWrap = true };
            st.normal.textColor = new Color(1f, 0.92f, 0.75f);
            GUI.Label(new Rect(r.x + 10 * u, r.y, r.width - 20 * u, r.height), Ar.T(tip), st);
        }
    }
}
