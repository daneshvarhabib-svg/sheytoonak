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
                        Vector3 sp = cam.WorldToScreenPoint(h.position + Vector3.up * (stage.heroHeight + 0.1f));
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

            // active poisons
            float y = top + 70 * u;
            foreach (var p in rules.Poisons)
            {
                // ⚠️ no "(n)": brackets and digits came out mirrored in RTL (run 30)
                string life = p.Tool.Life == Lifetime.UntilAntidote ? "حتى الدوا" : Ar.N(p.TurnsLeft) + " دور";
                // ⚠️ run 31-33: the row was shorter than the line, so the tails of ر و ى ص were clipped into a stray mark
                // (the shaped code points were right — checked). Taller rect and no clipping.
                Shadowed(new Rect(Screen.width * 0.6f - pad, y, Screen.width * 0.4f, 40 * u), Ar.T(Ar.PoisonWord + " " + p.Tool.NameAr + " - " + life), new GUIStyle(label) { alignment = TextAnchor.UpperRight, clipping = TextClipping.Overflow, wordWrap = false, normal = { textColor = new Color(1f, 0.7f, 0.7f) } });
                y += 34 * u;
            }

            if (Time.time < toastUntil)
                Shadowed(new Rect(0, Screen.height * 0.40f, Screen.width, 60 * u), Ar.T(toast), big);

            // my actions: one dark strip along the bottom, six tool buttons left, three controls right
            if (!ended && mine && !rules.KickInFlight)
            {
                float bh = 40 * u, gap = 6 * u;
                float stripH = bh * 3 + gap * 4;
                var old = GUI.color; GUI.color = new Color(0f, 0f, 0f, 0.45f);
                GUI.DrawTexture(new Rect(0, Screen.height - stripH, Screen.width, stripH), Texture2D.whiteTexture); GUI.color = old;
                float bw = Mathf.Min(250 * u, Screen.width * 0.24f);
                float x0 = pad, yb = Screen.height - stripH + gap;
                for (int i = 0; i < 3; i++)
                {
                    var h = rules.Heroes[0][i];
                    bool poisonOk = rules.CanPoison(duel.Me, h.Poison, duel.TargetsFor(h.Poison)) == ActionReject.None;
                    GUI.enabled = rules.ActionsLeft > 0 && poisonOk;
                    if (GUI.Button(new Rect(x0, yb + i * (bh + gap), bw, bh), Ar.T(Ar.PoisonWord + ": " + h.Poison.NameAr), button)) { touch?.IgnoreFor(0.4f); duel.Poison(i); }
                    GUI.enabled = rules.ActionsLeft > 0;
                    if (GUI.Button(new Rect(x0 + bw + gap, yb + i * (bh + gap), bw, bh), Ar.T(Ar.AntidoteWord + ": " + h.Antidote.NameAr), button)) { touch?.IgnoreFor(0.4f); duel.Antidote(i); }
                }
                GUI.enabled = true;
                float cw = Mathf.Min(200 * u, Screen.width * 0.18f), xr = Screen.width - pad - cw;
                if (rules.RageReady[0] && GUI.Button(new Rect(xr, yb, cw, bh), Ar.T(Ar.Rage + " ✦"), button)) { touch?.IgnoreFor(0.4f); duel.Rage(); }
                if (GUI.Button(new Rect(xr, yb + bh + gap, cw, bh), Ar.T(Ar.PassTurn), button)) { touch?.IgnoreFor(0.4f); duel.Pass(); }
                bool armed = Time.time < surrenderArmedUntil;
                if (GUI.Button(new Rect(xr, yb + (bh + gap) * 2, cw, bh), Ar.T(armed ? Ar.SurrenderSure : Ar.Surrender), button))
                {
                    touch?.IgnoreFor(0.4f);
                    if (armed) duel.Surrender(); else surrenderArmedUntil = Time.time + 3f; // two taps, like every costly button
                }
                if (rules.ActionsLeft > 0)
                    Shadowed(new Rect(0, Screen.height - stripH + gap + bh + gap, Screen.width, bh), Ar.T(Ar.ServeHint), new GUIStyle(small) { alignment = TextAnchor.MiddleCenter });
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

        static void Shadowed(Rect r, string text, GUIStyle s)
        {
            var shadow = new GUIStyle(s) { normal = { textColor = new Color(0f, 0f, 0f, 0.75f) } };
            GUI.Label(new Rect(r.x + 2, r.y + 2, r.width, r.height), text, shadow);
            GUI.Label(r, text, s);
        }
    }
}
