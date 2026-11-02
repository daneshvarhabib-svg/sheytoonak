using Sheytoonak.Core.Duel;
using UnityEngine;

namespace Sheytoonak.Prototype
{
    /// <summary>IMGUI head-up display of the duel: scores, turn, timer, actions, hero tools, window values, end panel.</summary>
    public class DuelHud : MonoBehaviour
    {
        public DuelManager duel;
        public DuelStage stage;
        public BackdropWindowMapper mapper;
        public Camera cam;
        public TouchKickInput touch;
        public System.Action onPlayAgain;

        string toast = ""; float toastUntil;
        bool ended; bool won; float surrenderArmedUntil;

        void OnEnable() { duel.Toast += OnToast; duel.Finished += OnFinished; duel.DuelStarted += () => ended = false; }
        void OnDisable() { duel.Toast -= OnToast; duel.Finished -= OnFinished; }
        void OnToast(string t, float s) { toast = t; toastUntil = Time.time + s; }
        void OnFinished(Side winner, bool iWon) { ended = true; won = iWon; }

        void OnGUI()
        {
            var rules = duel.Rules;
            if (rules == null) return; // the draft screen is up
            float u = Mathf.Max(1f, Screen.height / 900f);
            var font = Ar.Font;
            var label = new GUIStyle(GUI.skin.label) { fontSize = Mathf.RoundToInt(24 * u), normal = { textColor = Color.white }, font = font, clipping = TextClipping.Overflow };
            var small = new GUIStyle(label) { fontSize = Mathf.RoundToInt(19 * u) };
            var big = new GUIStyle(label) { fontSize = Mathf.RoundToInt(40 * u), alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            var button = new GUIStyle(GUI.skin.button) { fontSize = Mathf.RoundToInt(24 * u), font = font };
            float pad = 14f * u, top = pad + (Screen.height - Screen.safeArea.yMax);

            // window value badges on the facade
            if (!ended)
                for (int w = 0; w < rules.WindowCount; w++)
                {
                    if (rules.IsBroken(w)) continue;
                    Rect r = mapper.WindowRects[w];
                    Vector3 sp = cam.WorldToScreenPoint(new Vector3(r.center.x, r.yMax + 0.1f, mapper.FacadeZ));
                    if (sp.z < 0) continue;
                    string v = Ar.N(rules.ShownValue(w));
                    var st = new GUIStyle(small) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold,
                        normal = { textColor = rules.IsBreakable(w) ? new Color(1f, 0.95f, 0.5f) : new Color(0.6f, 0.6f, 0.6f) } };
                    Shadowed(new Rect(sp.x - 30 * u, Screen.height - sp.y - 26 * u, 60 * u, 26 * u), v, st);
                }

            // hero names
            if (stage != null)
                for (int s = 0; s < 2; s++)
                    for (int i = 0; i < 3 && stage.Heroes[s] != null; i++)
                    {
                        var h = stage.Heroes[s][i]; if (h == null) continue;
                        Vector3 sp = cam.WorldToScreenPoint(h.position + Vector3.up * (stage.heroHeight - 0.05f));
                        if (sp.z < 0) continue;
                        Shadowed(new Rect(sp.x - 120 * u, Screen.height - sp.y - 24 * u, 240 * u, 26 * u), Ar.T(rules.Heroes[s][i].NameAr.Split(' ')[0]),
                            new GUIStyle(small) { alignment = TextAnchor.MiddleCenter });
                    }

            // scores
            Shadowed(new Rect(pad, top, Screen.width * 0.4f, 44 * u), Ar.T("شبابيكي: " + Ar.N(rules.Windows[0]) + (rules.RageReady[0] ? "  " + Ar.Rage + " ✦" : "")), new GUIStyle(big) { alignment = TextAnchor.UpperLeft, fontSize = Mathf.RoundToInt(32 * u) });
            Shadowed(new Rect(Screen.width * 0.6f - pad, top, Screen.width * 0.4f, 44 * u), Ar.T("الخصم: " + Ar.N(rules.Windows[1]) + (rules.RageReady[1] ? "  " + Ar.Rage + " ✦" : "")), new GUIStyle(big) { alignment = TextAnchor.UpperRight, fontSize = Mathf.RoundToInt(32 * u) });
            bool mine = rules.Current == duel.Me;
            string turnLine = ended ? "" : (mine ? Ar.YourTurn : Ar.TheirTurn) + "  ·  " + Ar.Actions + " " + Ar.N(rules.ActionsLeft) + "  ·  " + Ar.Timer(Mathf.CeilToInt(duel.TurnTimeLeft));
            // the turn line lives at the bottom (the facade's top row is right under the top edge)
            float lineY = Screen.height - 40 * u * 3 - 6 * u * 3; // first row of the bottom strip, between the button columns
            Shadowed(new Rect(0, lineY, Screen.width, 32 * u), Ar.T(turnLine), new GUIStyle(label) { alignment = TextAnchor.MiddleCenter, normal = { textColor = mine ? new Color(0.6f, 1f, 0.6f) : new Color(1f, 0.8f, 0.6f) } });
            Shadowed(new Rect(0, top + 40 * u, Screen.width * 0.4f, 30 * u), Ar.T(Ar.WindowsLine(rules.Windows[0], rules.Windows[1], rules.Majority)), new GUIStyle(small) { alignment = TextAnchor.UpperLeft });

            // active poisons: icon + turns left (∞ = until the antidote), no words
            float y = top + 70 * u, isz = 40 * u;
            float ix = Screen.width - pad - isz;
            foreach (var p in rules.Poisons)
            {
                var old = GUI.color; GUI.color = PoisonRed;
                GUI.DrawTexture(new Rect(ix, y, isz, isz), Texture2D.whiteTexture); GUI.color = Color.white;
                var tex = Icon(p.Tool.Id);
                if (tex != null) GUI.DrawTexture(new Rect(ix + 4 * u, y + 4 * u, isz - 8 * u, isz - 8 * u), tex, ScaleMode.ScaleToFit);
                GUI.color = old;
                string life = p.Tool.Life == Lifetime.UntilAntidote ? "∞" : Ar.N(p.TurnsLeft);
                Shadowed(new Rect(ix - 50 * u, y, 44 * u, isz), Ar.T(life), new GUIStyle(label) { alignment = TextAnchor.MiddleRight });
                ix -= isz + 60 * u;
                if (ix < Screen.width * 0.55f) { ix = Screen.width - pad - isz; y += isz + 6 * u; }
            }

            if (Time.time < toastUntil)
                Shadowed(new Rect(0, Screen.height * 0.40f, Screen.width, 60 * u), Ar.T(toast), big);

            // my actions: icons, not words. User (2026-11-02): «سم و زهر نباید نوشته بشه … باید تصویر و آیکون باشه،
            // کلا هم تو بازی نوشته زیاده». Poisons sit on red badges (top row), antidotes on green (bottom row),
            // one column per hero; rage / pass / surrender on dark badges on the right.
            if (!ended && mine && !rules.KickInFlight)
            {
                float sz = 62 * u, gap = 8 * u;
                float stripH = sz * 2 + gap * 3;
                var old = GUI.color; GUI.color = new Color(0f, 0f, 0f, 0.4f);
                GUI.DrawTexture(new Rect(0, Screen.height - stripH, Screen.width, stripH), Texture2D.whiteTexture); GUI.color = old;
                float x0 = pad, yb = Screen.height - stripH + gap;
                for (int i = 0; i < 3; i++)
                {
                    var h = rules.Heroes[0][i];
                    float x = x0 + i * (sz + gap);
                    bool poisonOk = rules.ActionsLeft > 0 && rules.CanPoison(duel.Me, h.Poison, duel.TargetsFor(h.Poison)) == ActionReject.None;
                    if (IconButton(new Rect(x, yb, sz, sz), h.Poison.Id, PoisonRed, poisonOk, h.Poison.NameAr)) { touch?.IgnoreFor(0.4f); duel.Poison(i); }
                    if (IconButton(new Rect(x, yb + sz + gap, sz, sz), h.Antidote.Id, AntidoteGreen, rules.ActionsLeft > 0, h.Antidote.NameAr)) { touch?.IgnoreFor(0.4f); duel.Antidote(i); }
                }
                float xr = Screen.width - pad - sz;
                bool armed = Time.time < surrenderArmedUntil;
                if (IconButton(new Rect(xr, yb + sz + gap, sz, sz), "surrender", armed ? new Color(0.95f, 0.75f, 0.2f) : ControlDark, true, null))
                {
                    touch?.IgnoreFor(0.4f);
                    if (armed) duel.Surrender(); else { surrenderArmedUntil = Time.time + 3f; OnToast(Ar.SurrenderSure, 1.5f); } // two taps, like every costly button
                }
                if (IconButton(new Rect(xr - sz - gap, yb + sz + gap, sz, sz), "pass", ControlDark, true, null)) { touch?.IgnoreFor(0.4f); duel.Pass(); }
                if (rules.RageReady[0] && IconButton(new Rect(xr, yb, sz, sz), "rage", new Color(0.85f, 0.4f, 0.1f), true, null)) { touch?.IgnoreFor(0.4f); duel.Rage(); }
            }

            if (ended)
            {
                var old = GUI.color; GUI.color = new Color(0f, 0f, 0f, 0.65f);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture); GUI.color = old;
                GUILayout.BeginArea(new Rect(pad * 2, Screen.height * 0.25f, Screen.width - pad * 4, Screen.height * 0.6f));
                GUILayout.Label(Ar.T(won ? Ar.Won : Ar.Lost), big);
                GUILayout.Label(Ar.T(Ar.WindowsLine(rules.Windows[0], rules.Windows[1], rules.Majority)), new GUIStyle(label) { alignment = TextAnchor.MiddleCenter });
                var (shards, delta) = rules.RewardFor(duel.Me);
                GUILayout.Label(Ar.T(Ar.Reward(shards, delta, duel.Curses[0])), new GUIStyle(label) { alignment = TextAnchor.MiddleCenter });
                GUILayout.Space(16 * u);
                if (GUILayout.Button(Ar.T(Ar.PlayAgain), button, GUILayout.Height(70 * u))) { touch?.IgnoreFor(0.5f); onPlayAgain?.Invoke(); }
                GUILayout.EndArea();
            }
        }

        static readonly Color PoisonRed = new Color(0.72f, 0.18f, 0.15f), AntidoteGreen = new Color(0.16f, 0.5f, 0.32f), ControlDark = new Color(0.12f, 0.12f, 0.14f);
        static readonly System.Collections.Generic.Dictionary<string, Texture2D> icons = new System.Collections.Generic.Dictionary<string, Texture2D>();

        static Texture2D Icon(string id)
        {
            if (!icons.TryGetValue(id, out var t)) { t = Resources.Load<Texture2D>("Art/Icons/" + id); icons[id] = t; }
            return t;
        }

        /// <summary>A rounded-looking badge with a white glyph; disabled = dimmed. Falls back to the Arabic name if the icon is missing.</summary>
        static bool IconButton(Rect r, string iconId, Color bg, bool enabled, string fallbackAr)
        {
            var old = GUI.color;
            GUI.color = enabled ? bg : new Color(bg.r * 0.45f, bg.g * 0.45f, bg.b * 0.45f, 0.8f);
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = new Color(1f, 1f, 1f, 0.25f);
            GUI.DrawTexture(new Rect(r.x, r.y, r.width, 3f), Texture2D.whiteTexture); // a thin highlight on top
            GUI.color = enabled ? Color.white : new Color(1f, 1f, 1f, 0.35f);
            var tex = Icon(iconId);
            float m = r.width * 0.14f;
            if (tex != null) GUI.DrawTexture(new Rect(r.x + m, r.y + m, r.width - 2 * m, r.height - 2 * m), tex, ScaleMode.ScaleToFit);
            else if (fallbackAr != null) GUI.Label(r, Ar.T(fallbackAr), new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, wordWrap = true, font = Ar.Font, fontSize = Mathf.RoundToInt(r.height * 0.22f) });
            GUI.color = old;
            return enabled && GUI.Button(r, GUIContent.none, GUIStyle.none);
        }

        static void Shadowed(Rect r, string text, GUIStyle s)
        {
            var shadow = new GUIStyle(s) { normal = { textColor = new Color(0f, 0f, 0f, 0.75f) } };
            GUI.Label(new Rect(r.x + 2, r.y + 2, r.width, r.height), text, shadow);
            GUI.Label(r, text, s);
        }
    }
}
