using Sheytoonak.Core.Duel;
using UnityEngine;

namespace Sheytoonak.Prototype
{
    /// <summary>
    /// The ban/pick screen before every duel (user, 2026-11-01). Cards for every hero in the pool with the
    /// poison and the antidote; the current step says what to do; the opponent's picks and bans are shown as they
    /// happen (user 2026-11-07: a team is built against what the other side took).
    /// </summary>
    public class DraftScreen : MonoBehaviour
    {
        public DuelManager duel;
        public TouchKickInput touch;
        /// <summary>The hero shown large on the right after one tap; a second tap on it (or the big button) acts. User (2026-11-05):
        /// «اول یه ضربه بزنیم و اون بازیکن ابعادش بزرگتر شه مثل پسرخوانده بعد اگر دوباره زدیم روش انتخاب بشه، قدرتش هم بزرگ بشه».</summary>
        public string Focused { get; private set; }

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

        /// <summary>The focused hero at full height with a big poison and antidote, and the act button under it.</summary>
        void FocusPanel(DraftRules d, HeroDef[] all, bool myTurn, Rect r, float u, GUIStyle label, GUIStyle big)
        {
            var panel = DuelHud.Kit("panel");
            if (panel != null) GUI.DrawTexture(r, panel, ScaleMode.StretchToFill);
            else { var o = GUI.color; GUI.color = new Color(0.1f, 0.1f, 0.14f, 0.9f); GUI.DrawTexture(r, Texture2D.whiteTexture); GUI.color = o; }
            int fi = -1;
            for (int i = 0; i < all.Length; i++) if (all[i].Id == Focused) fi = i;
            if (fi < 0)
            {
                GUI.Label(new Rect(r.x, r.y + r.height * 0.45f, r.width, 40 * u), Ar.T(Ar.DraftTitle), label);
                return;
            }
            var h = all[fi];
            var ribbon = DuelHud.Kit("ribbon");
            var rib = new Rect(r.x + r.width * 0.08f, r.y - 10 * u, r.width * 0.84f, r.width * 0.84f * 246f / 969f);
            if (ribbon != null) GUI.DrawTexture(rib, ribbon, ScaleMode.StretchToFill);
            var ink = new GUIStyle(label) { fontSize = Mathf.RoundToInt(24 * u) }; ink.normal.textColor = new Color(0.35f, 0.18f, 0.08f);
            GUI.Label(new Rect(rib.x, rib.y + rib.height * 0.05f, rib.width, rib.height * 0.6f), Ar.T(h.NameAr), ink);
            // full standing figure, as tall as the panel allows
            var idle = Load("Art/Heroes/" + h.Id + "_idle");
            float figH = r.height - rib.height - 120 * u, figW = idle != null ? figH * idle.width / idle.height : figH * 0.5f;
            figW = Mathf.Min(figW, r.width * 0.55f); if (idle != null) figH = figW * idle.height / idle.width;
            var fr = new Rect(r.xMax - r.width * 0.06f - figW, rib.yMax + 4 * u, figW, figH);
            if (idle != null) GUI.DrawTexture(fr, idle, ScaleMode.ScaleToFit);
            // big poison (red card) and antidote (green card) on the left of the figure
            float bs = Mathf.Min(r.width * 0.34f, 150 * u);
            var cardR = DuelHud.Kit("card_red"); var cardG = DuelHud.Kit("card_green");
            var pr = new Rect(r.x + r.width * 0.06f, fr.y + 10 * u, bs, bs * 1.3f);
            var ar = new Rect(pr.x, pr.yMax + 12 * u, bs, bs * 1.3f);
            BigTool(pr, cardR, h.Poison.Id, new Color(0.72f, 0.18f, 0.15f), u);
            BigTool(ar, cardG, h.Antidote.Id, new Color(0.16f, 0.5f, 0.32f), u);
            // the act button: ban (red) or pick (green) when it is my move and allowed
            bool can = myTurn && !d.Done && (d.Current.What == DraftRules.Kind.Ban ? d.CanBan(duel.Me, h.Id) : d.CanPick(duel.Me, h.Id));
            bool ban = !d.Done && d.Current.What == DraftRules.Kind.Ban;
            var br = new Rect(r.x + r.width * 0.2f, r.yMax - 96 * u, r.width * 0.6f, 76 * u);
            var tex = DuelHud.Kit(ban ? "btn_red" : "btn_green");
            var old = GUI.color; GUI.color = can ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.8f);
            if (tex != null) GUI.DrawTexture(br, tex, ScaleMode.StretchToFill);
            GUI.color = old;
            DuelHud.DrawIcon(new Rect(br.center.x - 22 * u, br.y + 16 * u, 44 * u, 44 * u), ban ? "ban" : "pick", Color.white);
            if (can && GUI.Button(br, GUIContent.none, GUIStyle.none)) { touch?.IgnoreFor(0.5f); duel.DraftAct(h.Id); }
        }

        static void BigTool(Rect r, Texture2D card, string icon, Color flat, float u)
        {
            if (card != null) GUI.DrawTexture(r, card, ScaleMode.StretchToFill);
            else { var o = GUI.color; GUI.color = flat; GUI.DrawTexture(r, Texture2D.whiteTexture); GUI.color = o; }
            var t = Load("Art/Icons/" + icon);
            if (t != null) GUI.DrawTexture(new Rect(r.x + r.width * 0.15f, r.y + r.height * 0.2f, r.width * 0.7f, r.width * 0.7f), t, ScaleMode.ScaleToFit);
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
            // the grid takes the left two thirds (two rows of four when there are more than five), the right third is the
            // focused hero drawn large with its poison and antidote, like the reference game's draft
            float panelW = Screen.width * 0.34f;
            int cols = n > 5 ? Mathf.CeilToInt(n / 2f) : n, rows = Mathf.CeilToInt(n / (float)cols);
            float gridW = Screen.width - panelW - pad * 2f;
            float cw = Mathf.Min(170 * u, (gridW - pad * (cols - 1)) / cols), ch = cw * 1.3f;
            float total = cols * cw + (cols - 1) * pad, x0 = pad + gridW - (gridW - total) * 0.5f - cw, y0 = pad + si + 14 * u;
            if (Focused != null && (d.Banned.Contains(Focused) || d.Done)) Focused = null;
            // CI screenshots: focus the first card so the shot shows the big panel (no taps happen under xvfb)
            if (Focused == null && !string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("SHEYTOONAK_SHOTS"))) Focused = all[0].Id;
            for (int i = 0; i < n; i++)
            {
                var h = all[i];
                var r = new Rect(x0 - (i % cols) * (cw + pad), y0 + (i / cols) * (ch + pad), cw, ch);
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
                if (Focused == h.Id) { var oo = GUI.color; GUI.color = new Color(1f, 0.85f, 0.25f); GUI.DrawTexture(new Rect(r.x - 4 * u, r.y - 4 * u, r.width + 8 * u, 4 * u), Texture2D.whiteTexture); GUI.DrawTexture(new Rect(r.x - 4 * u, r.yMax, r.width + 8 * u, 4 * u), Texture2D.whiteTexture); GUI.DrawTexture(new Rect(r.x - 4 * u, r.y, 4 * u, r.height), Texture2D.whiteTexture); GUI.DrawTexture(new Rect(r.xMax, r.y, 4 * u, r.height), Texture2D.whiteTexture); GUI.color = oo; }
                if (!banned && GUI.Button(r, GUIContent.none, GUIStyle.none))
                {
                    touch?.IgnoreFor(0.5f);
                    // first tap shows the hero large; the second tap on the same card (when it is my move and allowed) acts
                    if (Focused == h.Id && can) duel.DraftAct(h.Id);
                    else Focused = h.Id;
                }
            }
            FocusPanel(d, all, myTurn, new Rect(Screen.width - pad - panelW, y0, panelW, Screen.height - y0 - 110 * u), u, label, big);

            // the two teams so far: mine as portraits on the right, the opponent's as portraits on the left.
            // User (2026-11-07): «موقع انتخاب بازیکن من باید ببینم رقیبم چی انتخاب کرده که بر اساس اون تیمم رو بچینم» —
            // the hidden «؟» cards are gone; every pick is shown as soon as it is made.
            float ty = Screen.height - 90 * u, ps = 70 * u;
            var mineList = d.Picked[(int)duel.Me];
            for (int k = 0; k < mineList.Count; k++)
            {
                var pr = new Rect(Screen.width - pad - (k + 1) * (ps + 8 * u), ty, ps, ps);
                var o = GUI.color; GUI.color = new Color(0.3f, 0.55f, 1f); GUI.DrawTexture(pr, Texture2D.whiteTexture); GUI.color = o;
                Portrait(new Rect(pr.x + 3 * u, pr.y + 3 * u, pr.width - 6 * u, pr.height - 6 * u), mineList[k]);
            }
            var theirs = d.Picked[1 - (int)duel.Me];
            for (int k = 0; k < theirs.Count; k++)
            {
                var pr = new Rect(pad + k * (ps + 8 * u), ty, ps, ps);
                var o = GUI.color; GUI.color = new Color(1f, 0.6f, 0.35f); GUI.DrawTexture(pr, Texture2D.whiteTexture); GUI.color = o;
                Portrait(new Rect(pr.x + 3 * u, pr.y + 3 * u, pr.width - 6 * u, pr.height - 6 * u), theirs[k]);
            }
        }
    }
}
