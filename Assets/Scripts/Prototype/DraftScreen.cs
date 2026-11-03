using Sheytoonak.Core.Duel;
using UnityEngine;

namespace Sheytoonak.Prototype
{
    /// <summary>
    /// The ban/pick screen before every duel (user, 2026-11-01). Cards for every hero in the pool with the
    /// poison and the antidote; the current step says what to do; the opponent's picks stay hidden ("؟")
    /// until the duel starts, its bans are public because they remove the hero for both.
    /// </summary>
    public class DraftScreen : MonoBehaviour
    {
        public DuelManager duel;
        public TouchKickInput touch;

        static readonly System.Collections.Generic.Dictionary<string, Texture2D> tex = new System.Collections.Generic.Dictionary<string, Texture2D>();
        static Texture2D Load(string path)
        {
            if (!tex.TryGetValue(path, out var t)) { t = Resources.Load<Texture2D>(path); tex[path] = t; }
            return t;
        }

        /// <summary>Head and shoulders out of the painted standing pose (no separate portrait art yet).</summary>
        static void Portrait(Rect r, string heroId)
        {
            var face = Load("Art/Heroes/" + heroId + "_face"); // painted head-and-shoulders portrait (2026-11-03)
            if (face != null) { GUI.DrawTexture(r, face, ScaleMode.ScaleAndCrop); return; }
            var t = Load("Art/Heroes/" + heroId + "_idle");
            if (t == null) return;
            // the top ~42% of the figure, centred, cropped to the rect's aspect
            float vh = 0.42f, vw = vh * (r.width / r.height) * t.height / t.width;
            vw = Mathf.Min(1f, vw);
            GUI.DrawTextureWithTexCoords(r, t, new Rect(0.5f - vw * 0.5f, 1f - vh - 0.01f, vw, vh));
        }

        static void Badge(Rect r, string icon, Color bg)
        {
            var o = GUI.color; GUI.color = bg; GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = Color.white; var t = Load("Art/Icons/" + icon);
            if (t != null) GUI.DrawTexture(new Rect(r.x + r.width * 0.14f, r.y + r.height * 0.14f, r.width * 0.72f, r.height * 0.72f), t, ScaleMode.ScaleToFit);
            GUI.color = o;
        }

        void OnGUI()
        {
            var d = duel.Draft;
            if (d == null || duel.Rules != null) return;
            float u = Mathf.Max(1f, Screen.height / 900f);
            var font = Ar.Font;
            var label = new GUIStyle(GUI.skin.label) { fontSize = Mathf.RoundToInt(22 * u), normal = { textColor = Color.white }, font = font, alignment = TextAnchor.MiddleCenter };
            var small = new GUIStyle(label) { fontSize = Mathf.RoundToInt(17 * u) };
            var big = new GUIStyle(label) { fontSize = Mathf.RoundToInt(38 * u), fontStyle = FontStyle.Bold };
            var card = new GUIStyle(GUI.skin.button) { font = font, fontSize = Mathf.RoundToInt(20 * u), wordWrap = true };

            var old = GUI.color; GUI.color = new Color(0f, 0f, 0f, 0.72f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture); GUI.color = old;

            // ⚠️ pictures, not words (user 2026-11-02: «کلا تو بازی نوشته زیاده»): each card is the hero's painted
            // portrait with the poison (red) and antidote (green) icons; the step is an icon in the active side's colour.
            float pad = 16 * u;
            bool myTurn = !d.Done && d.Current.Who == duel.Me;
            Color sideC = myTurn ? new Color(0.45f, 0.85f, 0.45f) : new Color(1f, 0.6f, 0.35f);
            string stepIcon = d.Done ? "pick" : !myTurn ? "clock" : d.Current.What == DraftRules.Kind.Ban ? "ban" : "pick";
            float si = 64 * u;
            Badge(new Rect((Screen.width - si) * 0.5f, pad, si, si), stepIcon, myTurn && d.Current.What == DraftRules.Kind.Ban ? new Color(0.72f, 0.18f, 0.15f) : sideC);

            var all = StarterHeroes.All;
            int n = all.Length;
            float cw = Mathf.Min(230 * u, (Screen.width - pad * (n + 1)) / n), ch = cw * 1.45f;
            float total = n * cw + (n - 1) * pad, x0 = (Screen.width + total) * 0.5f - cw, y0 = pad + si + 18 * u;
            for (int i = 0; i < n; i++)
            {
                var h = all[i];
                var r = new Rect(x0 - i * (cw + pad), y0, cw, ch);
                bool banned = d.Banned.Contains(h.Id);
                bool mine = d.Picked[(int)duel.Me].Contains(h.Id);
                bool can = myTurn && (d.Current.What == DraftRules.Kind.Ban ? d.CanBan(duel.Me, h.Id) : d.CanPick(duel.Me, h.Id));
                var o = GUI.color;
                GUI.color = mine ? new Color(0.3f, 0.55f, 1f) : can ? sideC : new Color(0.25f, 0.25f, 0.28f);
                GUI.DrawTexture(r, Texture2D.whiteTexture);
                GUI.color = new Color(0.1f, 0.1f, 0.12f);
                var inner = new Rect(r.x + 4 * u, r.y + 4 * u, r.width - 8 * u, r.height - 8 * u);
                GUI.DrawTexture(inner, Texture2D.whiteTexture);
                GUI.color = banned ? new Color(0.45f, 0.45f, 0.45f, 0.6f) : Color.white;
                Portrait(new Rect(inner.x, inner.y, inner.width, inner.height - cw * 0.42f), h.Id);
                GUI.color = o;
                float bs = cw * 0.36f, by = inner.yMax - bs - 4 * u;
                Badge(new Rect(inner.center.x + 4 * u, by, bs, bs), h.Poison.Id, new Color(0.72f, 0.18f, 0.15f));
                Badge(new Rect(inner.center.x - bs - 4 * u, by, bs, bs), h.Antidote.Id, new Color(0.16f, 0.5f, 0.32f));
                if (banned) Badge(new Rect(r.center.x - cw * 0.3f, r.y + ch * 0.18f, cw * 0.6f, cw * 0.6f), "ban", new Color(0.72f, 0.18f, 0.15f, 0.85f));
                else if (mine) Badge(new Rect(r.xMax - 40 * u, r.y + 6 * u, 34 * u, 34 * u), "pick", new Color(0.3f, 0.55f, 1f));
                if (can && GUI.Button(r, GUIContent.none, GUIStyle.none)) { touch?.IgnoreFor(0.5f); duel.DraftAct(h.Id); }
            }

            // the two teams so far: mine as portraits on the right, the opponent's as hidden "؟" cards on the left
            float ty = y0 + ch + 24 * u, ps = 70 * u;
            var mineList = d.Picked[(int)duel.Me];
            for (int k = 0; k < mineList.Count; k++)
            {
                var pr = new Rect(Screen.width - pad - (k + 1) * (ps + 8 * u), ty, ps, ps);
                var o = GUI.color; GUI.color = new Color(0.3f, 0.55f, 1f); GUI.DrawTexture(pr, Texture2D.whiteTexture); GUI.color = o;
                Portrait(new Rect(pr.x + 3 * u, pr.y + 3 * u, pr.width - 6 * u, pr.height - 6 * u), mineList[k]);
            }
            for (int k = 0; k < d.Picked[1 - (int)duel.Me].Count; k++)
            {
                var pr = new Rect(pad + k * (ps + 8 * u), ty, ps, ps);
                var o = GUI.color; GUI.color = new Color(1f, 0.6f, 0.35f); GUI.DrawTexture(pr, Texture2D.whiteTexture);
                GUI.color = new Color(0.12f, 0.12f, 0.14f); GUI.DrawTexture(new Rect(pr.x + 3 * u, pr.y + 3 * u, pr.width - 6 * u, pr.height - 6 * u), Texture2D.whiteTexture); GUI.color = o;
                GUI.Label(pr, "؟", new GUIStyle(big) { alignment = TextAnchor.MiddleCenter });
            }
        }
    }
}
