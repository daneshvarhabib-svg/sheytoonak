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
        public System.Action onPlayAgain, onHome;

        string toast = ""; float toastUntil;
        bool ended; bool won; float surrenderArmedUntil;

        void OnEnable() { duel.Toast += OnToast; duel.Pop += OnPop; duel.Finished += OnFinished; duel.DuelStarted += () => { ended = false; duel.Rules.WindowTaken += OnTaken; }; }
        void OnDisable() { duel.Toast -= OnToast; duel.Pop -= OnPop; duel.Finished -= OnFinished; }
        const float PopSeconds = 1.4f;
        string popIcon; Side popSide; DuelManager.PopKind popKind; float popUntil;
        int gainWindow = -1, gainValue; Side gainSide; float gainUntil;
        void OnTaken(Side side, int w, int v) { gainWindow = w; gainValue = v; gainSide = side; gainUntil = Time.time + 1.3f; }
        void OnPop(string icon, Side side, DuelManager.PopKind kind) { popIcon = icon; popSide = side; popKind = kind; popUntil = Time.time + PopSeconds; }
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
                    if (rules.IsBroken(w) || w == rules.SealedWindow) continue; // the shutter says it all
                    Rect r = mapper.WindowRects[w];
                    Vector3 sp = cam.WorldToScreenPoint(new Vector3(r.center.x, r.yMax + 0.1f, mapper.FacadeZ));
                    if (sp.z < 0) continue;
                    string v = Ar.N(rules.ShownValue(w));
                    var st = new GUIStyle(small) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold,
                        normal = { textColor = rules.IsBreakable(w) ? new Color(1f, 0.95f, 0.5f) : new Color(0.6f, 0.6f, 0.6f) } };
                    Shadowed(new Rect(sp.x - 30 * u, Screen.height - sp.y - 26 * u, 60 * u, 26 * u), v, st);
                }

            // ⚠️ no hero names, no sentences (user 2026-11-02: «کلا تو بازی نوشته زیاده و بهتره تا جای ممکن از آیکون
            // و تصویر بهره ببریم»): the faces name the heroes; score = window icon + number, the majority as a bar.
            bool mine = rules.Current == duel.Me;
            var num = new GUIStyle(big) { fontSize = Mathf.RoundToInt(34 * u), alignment = TextAnchor.MiddleLeft };
            float isz0 = 40 * u;
            for (int side = 0; side < 2; side++)
            {
                bool left = side == 0;
                float x = left ? pad : Screen.width - pad - isz0;
                DrawIcon(new Rect(x, top, isz0, isz0), "window", Color.white);
                var st = new GUIStyle(num) { alignment = left ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight };
                Shadowed(new Rect(left ? x + isz0 + 8 * u : x - 108 * u, top, 100 * u, isz0), Ar.N(rules.Windows[side]), st);
                if (rules.RageReady[side]) DrawIcon(new Rect(left ? x + isz0 + 70 * u : x - 110 * u, top + 4 * u, 32 * u, 32 * u), "rage", new Color(1f, 0.6f, 0.2f));
                // progress toward the majority: a bar under the score
                float bw = Screen.width * 0.22f, bx = left ? pad : Screen.width - pad - bw, by = top + isz0 + 6 * u;
                Bar(new Rect(bx, by, bw, 8 * u), Mathf.Clamp01(rules.Windows[side] / (float)Mathf.Max(1, rules.Majority)), left, side == (int)duel.Me ? new Color(0.45f, 0.85f, 0.45f) : new Color(1f, 0.6f, 0.35f));
            }
            // whose turn: the timer bar sits on the active side's half of the bottom, actions left as dots above it
            if (!ended)
            {
                float tw = Screen.width * 0.2f, th = 10 * u;
                float tx = Screen.width * 0.5f + (mine ? -tw - 20 * u : 20 * u);
                float ty = Screen.height - 22 * u; // inside the bottom strip, between the tool buttons (run 52: over the heroes' legs it was clutter)
                Color tc = mine ? new Color(0.45f, 0.85f, 0.45f) : new Color(1f, 0.6f, 0.35f);
                Bar(new Rect(tx, ty, tw, th), Mathf.Clamp01(duel.TurnTimeLeft / DuelRules.TurnSeconds), mine, duel.TurnTimeLeft < 10f ? new Color(1f, 0.3f, 0.25f) : tc);
                DrawIcon(new Rect(mine ? tx - 30 * u : tx + tw + 6 * u, ty - 10 * u, 26 * u, 26 * u), "clock", tc);
                for (int a = 0; a < rules.ActionsLeft; a++)
                    DrawDot(new Rect(tx + (mine ? tw - (a + 1) * 22 * u : a * 22 * u), ty - 24 * u, 16 * u, 16 * u), tc);
            }

            // "+2" rising off the window that just broke, in the colour of the side that broke it
            if (Time.time < gainUntil && gainWindow >= 0)
            {
                float k = 1f - (gainUntil - Time.time) / 1.3f;
                Rect wr = mapper.WindowRects[gainWindow];
                Vector3 sp = cam.WorldToScreenPoint(new Vector3(wr.center.x, wr.center.y, mapper.FacadeZ));
                var st = new GUIStyle(big) { fontSize = Mathf.RoundToInt(54 * u), normal = { textColor = gainSide == duel.Me ? new Color(0.55f, 1f, 0.55f, 1f - k * k) : new Color(1f, 0.65f, 0.4f, 1f - k * k) } };
                Shadowed(new Rect(sp.x - 100 * u, Screen.height - sp.y - 40 * u - k * 60 * u, 200 * u, 80 * u), "+" + Ar.N(gainValue), st);
            }

            // pictures for what just happened: the tool's icon pops up over the side that played it
            if (Time.time < popUntil && popIcon != null)
            {
                float k = 1f - (popUntil - Time.time) / PopSeconds; // 0..1
                float sz = Mathf.Lerp(70f, 110f, Mathf.Min(1f, k * 4f)) * u;
                float px = popSide == duel.Me ? Screen.width * 0.3f : Screen.width * 0.7f;
                var c = popKind == DuelManager.PopKind.Poison ? PoisonRed : popKind == DuelManager.PopKind.Antidote ? AntidoteGreen : new Color(0.85f, 0.45f, 0.1f);
                c.a = k > 0.75f ? (1f - k) * 4f : 1f;
                var old = GUI.color; GUI.color = c;
                var r = new Rect(px - sz * 0.5f, Screen.height * 0.36f - sz * 0.5f, sz, sz);
                GUI.DrawTexture(r, Texture2D.whiteTexture);
                GUI.color = new Color(1f, 1f, 1f, c.a);
                var tex = Icon(popIcon);
                if (tex != null) GUI.DrawTexture(new Rect(r.x + sz * 0.14f, r.y + sz * 0.14f, sz * 0.72f, sz * 0.72f), tex, ScaleMode.ScaleToFit);
                GUI.color = old;
            }

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
            if (touch != null) touch.DeadZoneHeight = 0f;
            if (!ended && mine && !rules.KickInFlight)
            {
                // User (2026-11-05): «کارت انتخاب قهرمان توی بازی و انتخاب قدرت‌هاشون بزرگتر بشه». 62 -> 84 px at 900p.
                float sz = 84 * u, gap = 10 * u;
                float stripH = sz * 2 + gap * 3;
                if (touch != null)
                {
                    touch.DeadZoneHeight = stripH;
                    touch.DeadZoneLeft = pad + 3 * (sz + gap) + gap;   // faces / poison / antidote / kick columns
                    touch.DeadZoneRight = pad + 2 * (sz + gap) + gap;  // pass, surrender, rage
                }
                var old = GUI.color; GUI.color = new Color(0f, 0f, 0f, 0.4f);
                GUI.DrawTexture(new Rect(0, Screen.height - stripH, Screen.width, stripH), Texture2D.whiteTexture); GUI.color = old;
                float x0 = pad, yb = Screen.height - stripH + gap;
                // ⚠️ user (2026-11-04): «باید اول قهرمان انتخاب شه بعد قدرتش یا زدن ضربه. اگر خواست ضربه بزنه تازه
                // بیاد پشت توپ». Bottom row: my three heroes' faces; top row: the chosen hero's poison, antidote and kick.
                for (int i = 0; i < 3; i++)
                {
                    var h = rules.Heroes[(int)duel.Me][i];
                    var fr = new Rect(x0 + i * (sz + gap), yb + sz + gap, sz, sz);
                    bool sel = duel.Selected == i;
                    var oc = GUI.color;
                    GUI.color = sel ? new Color(1f, 0.85f, 0.25f) : new Color(0f, 0f, 0f, 0.5f);
                    GUI.DrawTexture(new Rect(fr.x - 3 * u, fr.y - 3 * u, fr.width + 6 * u, fr.height + 6 * u), Texture2D.whiteTexture);
                    GUI.color = duel.Selected < 0 ? new Color(1f, 1f, 1f, 0.75f + 0.25f * Mathf.Sin(Time.time * 5f)) : sel ? Color.white : new Color(0.7f, 0.7f, 0.7f);
                    Face(fr, h.Id);
                    GUI.color = Color.white;
                    var pf = Kit("portrait");
                    if (pf != null) GUI.DrawTexture(new Rect(fr.x - fr.width * 0.12f, fr.y - fr.height * 0.12f, fr.width * 1.24f, fr.height * 1.24f), pf, ScaleMode.StretchToFill);
                    GUI.color = oc;
                    if (GUI.Button(fr, GUIContent.none, GUIStyle.none)) { touch?.IgnoreFor(0.4f); duel.Select(i); }
                }
                if (duel.Selected >= 0)
                {
                    int i = duel.Selected;
                    var h = rules.Heroes[(int)duel.Me][i];
                    bool poisonOk = rules.ActionsLeft > 0 && rules.CanPoison(duel.Me, h.Poison, duel.TargetsFor(h.Poison)) == ActionReject.None;
                    float x = x0;
                    if (IconButton(new Rect(x, yb, sz, sz), h.Poison.Id, PoisonRed, poisonOk, h.Poison.NameAr)) { touch?.IgnoreFor(0.4f); duel.Poison(i); }
                    if (IconButton(new Rect(x + sz + gap, yb, sz, sz), h.Antidote.Id, AntidoteGreen, rules.ActionsLeft > 0, h.Antidote.NameAr)) { touch?.IgnoreFor(0.4f); duel.Antidote(i); }
                    var kr = new Rect(x + 2 * (sz + gap), yb, sz, sz);
                    if (IconButton(kr, "kick", duel.KickArmed ? new Color(1f, 0.85f, 0.25f) : new Color(0.2f, 0.45f, 0.75f), rules.ActionsLeft > 0, null)) { touch?.IgnoreFor(0.4f); duel.ArmKick(); }
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
                // the result in pictures: windows, shards won, curses gained or lost (or, in a challenge, the run so far)
                float cx = Screen.width * 0.5f, y0 = Screen.height * 0.2f, esz = 56 * u;
                Shadowed(new Rect(0, y0, Screen.width, 60 * u), Ar.T(won ? Ar.Won : Ar.Lost), big);
                var enumSt = new GUIStyle(big) { fontSize = Mathf.RoundToInt(34 * u), alignment = TextAnchor.MiddleLeft };
                float ey = y0 + 90 * u;
                if (duel.InChallenge && duel.Challenge != null)
                {
                    ChallengeStrip(new Rect(cx - Screen.width * 0.35f, ey, Screen.width * 0.7f, esz), duel.Challenge, u);
                }
                else
                {
                    var (shards, delta) = rules.RewardFor(duel.Me);
                    string[] ids = { "window", "shards", "curse" };
                    string[] vals = { Ar.N(rules.Windows[0]) + " - " + Ar.N(rules.Windows[1]), "+" + Ar.N(shards), (delta >= 0 ? "+" : "-") + Ar.N(Mathf.Abs(delta)) };
                    for (int k = 0; k < 3; k++)
                    {
                        float ex = cx - Screen.width * 0.33f + k * Screen.width * 0.23f;
                        DrawIcon(new Rect(ex, ey, esz, esz), ids[k], k == 2 && delta < 0 ? new Color(1f, 0.5f, 0.4f) : Color.white);
                        Shadowed(new Rect(ex + esz + 10 * u, ey, 200 * u, esz), vals[k], enumSt);
                    }
                }
                bool more = !duel.InChallenge || duel.ChallengeActive;
                var br = new Rect(cx + 10 * u, y0 + 190 * u, 140 * u, 90 * u);
                if (more && IconButton(br, "replay", AntidoteGreen, true, Ar.PlayAgain)) { touch?.IgnoreFor(0.5f); onPlayAgain?.Invoke(); }
                var hr = new Rect(cx - 150 * u, y0 + 190 * u, 140 * u, 90 * u);
                if (IconButton(more ? hr : new Rect(cx - 70 * u, hr.y, hr.width, hr.height), "home", ControlDark, true, null)) { touch?.IgnoreFor(0.5f); onHome?.Invoke(); }
            }
        }

        internal static readonly Color PoisonRed = new Color(0.72f, 0.18f, 0.15f), AntidoteGreen = new Color(0.16f, 0.5f, 0.32f), ControlDark = new Color(0.12f, 0.12f, 0.14f);
        static readonly System.Collections.Generic.Dictionary<string, Texture2D> icons = new System.Collections.Generic.Dictionary<string, Texture2D>();

        static readonly System.Collections.Generic.Dictionary<string, string> IconStandIn = new System.Collections.Generic.Dictionary<string, string>
        {
            // painted since 2026-11-04; kept for any later hero whose icon is still missing
        };

        internal static Texture2D Icon(string id)
        {
            if (!icons.TryGetValue(id, out var t))
            {
                t = Resources.Load<Texture2D>("Art/Icons/" + id);
                // tools of the new heroes until their icons are painted: the icon of a tool with the same effect
                if (t == null && IconStandIn.TryGetValue(id, out var alt)) t = Resources.Load<Texture2D>("Art/Icons/" + alt);
                icons[id] = t;
            }
            return t;
        }

        /// <summary>A rounded-looking badge with a white glyph; disabled = dimmed. Falls back to the Arabic name if the icon is missing.</summary>
        static readonly System.Collections.Generic.Dictionary<string, Texture2D> kit = new System.Collections.Generic.Dictionary<string, Texture2D>();
        internal static Texture2D Kit(string name)
        {
            if (!kit.TryGetValue(name, out var t)) { t = Resources.Load<Texture2D>("Art/UI/" + name); kit[name] = t; }
            return t;
        }

        /// <summary>Painted button for a badge colour: red, green, orange or blue by hue; greys and very dark colours get none.</summary>
        static Texture2D ButtonFor(Color c)
        {
            Color.RGBToHSV(c, out float h, out float sat, out float v);
            if (sat < 0.3f || v < 0.25f) return null;
            string n = h < 0.05f || h > 0.92f ? "btn_red" : h < 0.14f ? "btn_orange" : h < 0.45f ? "btn_green" : "btn_blue";
            return Kit(n);
        }

        internal static bool IconButton(Rect r, string iconId, Color bg, bool enabled, string fallbackAr)
        {
            var old = GUI.color;
            // painted buttons of the UI kit (2026-11-05, user: «کیفیت بصری بازی خیلی نزدیک بشه» to the reference game):
            // the colour of the flat badge picks the painted button of the nearest hue; dark controls stay flat
            var painted = ButtonFor(bg);
            if (painted != null)
            {
                GUI.color = enabled ? Color.white : new Color(0.45f, 0.45f, 0.45f, 0.85f);
                GUI.DrawTexture(new Rect(r.x - r.width * 0.06f, r.y - r.height * 0.04f, r.width * 1.12f, r.height * 1.1f), painted, ScaleMode.StretchToFill);
            }
            else
            {
                GUI.color = enabled ? bg : new Color(bg.r * 0.45f, bg.g * 0.45f, bg.b * 0.45f, 0.8f);
                GUI.DrawTexture(r, Texture2D.whiteTexture);
                GUI.color = new Color(1f, 1f, 1f, 0.25f);
                GUI.DrawTexture(new Rect(r.x, r.y, r.width, 3f), Texture2D.whiteTexture); // a thin highlight on top
            }
            GUI.color = enabled ? Color.white : new Color(1f, 1f, 1f, 0.35f);
            var tex = Icon(iconId);
            float m = r.width * 0.14f;
            if (tex != null) GUI.DrawTexture(new Rect(r.x + m, r.y + m, r.width - 2 * m, r.height - 2 * m), tex, ScaleMode.ScaleToFit);
            else if (fallbackAr != null) GUI.Label(r, Ar.T(fallbackAr), new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, wordWrap = true, font = Ar.Font, fontSize = Mathf.RoundToInt(r.height * 0.22f) });
            GUI.color = old;
            return enabled && GUI.Button(r, GUIContent.none, GUIStyle.none);
        }

        /// <summary>The run at a glance: 12 win slots, 3 hearts, the prize now and after the next win.</summary>
        static readonly System.Collections.Generic.Dictionary<string, Texture2D> faces = new System.Collections.Generic.Dictionary<string, Texture2D>();
        /// <summary>The hero's painted portrait (or the top of the standing figure) cropped into the rect.</summary>
        static void Face(Rect r, string heroId)
        {
            if (!faces.TryGetValue(heroId, out var t)) { t = Resources.Load<Texture2D>("Art/Heroes/" + heroId + "_face"); faces[heroId] = t; }
            if (t != null) { GUI.DrawTexture(r, t, ScaleMode.ScaleAndCrop); return; }
            var idle = Resources.Load<Texture2D>("Art/Heroes/" + heroId + "_idle");
            if (idle == null) return;
            float vh = 0.3f, vw = Mathf.Min(1f, vh * (r.width / r.height) * idle.height / idle.width);
            GUI.DrawTextureWithTexCoords(r, idle, new Rect(0.5f - vw * 0.5f, 1f - vh - 0.01f, vw, vh));
        }

        internal static void ChallengeStrip(Rect r, Sheytoonak.Core.Challenge.ChallengeRun run, float u)
        {
            float d = Mathf.Min(r.height * 0.42f, r.width / 22f);
            for (int k = 0; k < Sheytoonak.Core.Challenge.ChallengeRun.MaxWins; k++)
                DrawDot(new Rect(r.x + k * (d + 4 * u), r.y, d, d), k < run.Wins ? new Color(0.45f, 0.9f, 0.45f) : new Color(1f, 1f, 1f, 0.25f));
            for (int k = 0; k < Sheytoonak.Core.Challenge.ChallengeRun.MaxLosses; k++)
                DrawIcon(new Rect(r.xMax - (k + 1) * (d * 1.4f + 4 * u), r.y - d * 0.2f, d * 1.4f, d * 1.4f), "heart",
                    k < Sheytoonak.Core.Challenge.ChallengeRun.MaxLosses - run.Losses ? new Color(0.95f, 0.3f, 0.3f) : new Color(1f, 1f, 1f, 0.2f));
            float py = r.y + d + 10 * u, ps = 34 * u;
            var st = new GUIStyle(GUI.skin.label) { font = Ar.Font, fontSize = Mathf.RoundToInt(28 * u), alignment = TextAnchor.MiddleLeft, normal = { textColor = Color.white }, clipping = TextClipping.Overflow };
            DrawIcon(new Rect(r.x, py, ps, ps), "shards", Color.white);
            string prize = Ar.N(run.PrizeNow) + (run.Wins < Sheytoonak.Core.Challenge.ChallengeRun.MaxWins ? "  →  " + Ar.N(run.PrizeNext) : "");
            GUI.Label(new Rect(r.x + ps + 8 * u, py, r.width * 0.6f, ps), prize, st);
        }

        /// <summary>Icons that are painted (full colour, from the ChatGPT sheet) are never tinted, only faded.</summary>
        static readonly System.Collections.Generic.HashSet<string> Painted = new System.Collections.Generic.HashSet<string>
            { "pigeon","bus","wind","open_window","puddle","nails","vane","pebble","plank","fart","rage","pass","surrender","window","shards","curse","clock","replay",
              "truck","cart","rain","complaint","umbrella","flashlight" };

        internal static void DrawIcon(Rect r, string id, Color tint)
        {
            if (Painted.Contains(id) || (IconStandIn.TryGetValue(id, out var alt) && Painted.Contains(alt))) tint = new Color(1f, 1f, 1f, tint.a);
            var tex = Icon(id); if (tex == null) return;
            var old = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.5f); GUI.DrawTexture(new Rect(r.x + 2, r.y + 2, r.width, r.height), tex, ScaleMode.ScaleToFit);
            GUI.color = tint; GUI.DrawTexture(r, tex, ScaleMode.ScaleToFit);
            GUI.color = old;
        }

        internal static void Bar(Rect r, float fill, bool fromLeft, Color c)
        {
            var old = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.45f); GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = c;
            float w = r.width * fill;
            GUI.DrawTexture(new Rect(fromLeft ? r.x : r.xMax - w, r.y, w, r.height), Texture2D.whiteTexture);
            GUI.color = old;
        }

        static Texture2D dot;
        internal static void DrawDot(Rect r, Color c)
        {
            if (dot == null)
            {
                const int n = 32; dot = new Texture2D(n, n, TextureFormat.RGBA32, false);
                for (int y = 0; y < n; y++) for (int x = 0; x < n; x++)
                { float d = Mathf.Sqrt((x - 15.5f) * (x - 15.5f) + (y - 15.5f) * (y - 15.5f)); dot.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(15f - d))); }
                dot.Apply();
            }
            var old = GUI.color; GUI.color = c; GUI.DrawTexture(r, dot); GUI.color = old;
        }

        static void Shadowed(Rect r, string text, GUIStyle s)
        {
            var shadow = new GUIStyle(s) { normal = { textColor = new Color(0f, 0f, 0f, 0.75f) } };
            GUI.Label(new Rect(r.x + 2, r.y + 2, r.width, r.height), text, shadow);
            GUI.Label(r, text, s);
        }
    }
}
