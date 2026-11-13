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

        void OnEnable() { duel.Toast += OnToast; duel.Pop += OnPop; duel.Finished += OnFinished; duel.TurnChanged += OnTurn; duel.DuelStarted += () => { ended = false; duel.Rules.WindowTaken += OnTaken; }; }
        void OnDisable() { duel.Toast -= OnToast; duel.Pop -= OnPop; duel.Finished -= OnFinished; duel.TurnChanged -= OnTurn; }

        // ------------------------------------------------------------------ the turn banner and the result board
        // User (2026-11-11), with two screen recordings of the reference game: «دیزاین بازی … بهتره شبیه‌تر بشه به این».
        // Every change of turn drops a big painted plank «دورك» / «دور الخصم» from the top (1.3 s), and the end of a duel
        // is a board that bounces in over a darkened street instead of a line of text.
        const float BannerSeconds = 1.3f;
        float bannerAt = -10f; bool bannerMine; float endedAt;
        void OnTurn(Side s) { if (ended) return; bannerAt = Time.time; bannerMine = s == duel.Me; }

        void TurnBanner(float u, GUIStyle big)
        {
            float t = Time.time - bannerAt;
            if (t < 0f || t > BannerSeconds) return;
            float inT = Mathf.Clamp01(t / 0.28f), outT = Mathf.Clamp01((BannerSeconds - t) / 0.25f);
            float drop = 1f - Mathf.Pow(1f - inT, 3f);
            float bw = Mathf.Min(560f * u, Screen.width * 0.42f), bh = bw * 0.254f;
            float y = Mathf.Lerp(-bh, Screen.height * 0.17f, drop) - (1f - outT) * bh * 0.4f;
            float sc = inT < 1f ? 1f : 1f + 0.06f * Mathf.Sin(Mathf.Clamp01((t - 0.28f) / 0.25f) * Mathf.PI);
            var r = new Rect(Screen.width * 0.5f - bw * sc * 0.5f, y, bw * sc, bh * sc);
            var old = GUI.color; GUI.color = new Color(1f, 1f, 1f, outT);
            var plank = Kit("ribbon");
            if (plank != null) GUI.DrawTexture(r, plank, ScaleMode.StretchToFill);
            else { GUI.color = new Color(0.35f, 0.2f, 0.1f, 0.9f * outT); GUI.DrawTexture(r, Texture2D.whiteTexture); }
            var st = new GUIStyle(big) { fontSize = Mathf.RoundToInt(bh * 0.36f) };
            st.normal.textColor = bannerMine ? new Color(0.25f, 0.55f, 0.2f, outT) : new Color(0.65f, 0.15f, 0.1f, outT);
            GUI.Label(new Rect(r.x, r.y - r.height * 0.06f, r.width, r.height), Ar.T(bannerMine ? Ar.YourTurn : Ar.TheirTurn), st);
            GUI.color = old;
        }

        /// <summary>The result board: rays, the painted plank with «ربحت! / خسرت» bouncing in, two pennants.</summary>
        float ResultBoard(float u, GUIStyle big)
        {
            float t = Time.time - endedAt;
            float pop = Mathf.Clamp01(t / 0.45f);
            float sc = pop < 1f ? Mathf.Lerp(0.3f, 1.12f, 1f - Mathf.Pow(1f - pop, 3f)) : 1f + 0.12f * Mathf.Exp(-(t - 0.45f) * 6f);
            float bw = Mathf.Min(620f * u, Screen.width * 0.46f), bh = bw * 0.254f, cx = Screen.width * 0.5f, cy = Screen.height * 0.22f;
            var old = GUI.color;
            // slow-turning rays behind the plank (gold for a win, red for a loss)
            var ray = won ? new Color(1f, 0.82f, 0.3f, 0.22f) : new Color(0.9f, 0.2f, 0.15f, 0.22f);
            var m = GUI.matrix;
            for (int k = 0; k < 10; k++)
            {
                GUIUtility.RotateAroundPivot(t * 12f + k * 36f, new Vector2(cx, cy));
                GUI.color = ray; GUI.DrawTexture(new Rect(cx - 9f * u, cy - bw * 0.62f, 18f * u, bw * 0.62f), Texture2D.whiteTexture);
                GUI.matrix = m;
            }
            // two pennants on poles (only around the old plank; the painted emblem has its own laurels)
            bool painted = Resources.Load<Texture2D>(won ? "Art/UI/win_board" : "Art/UI/lose_board") != null;
            for (int sgn = -1; sgn <= 1 && !painted; sgn += 2)
            {
                float px = cx + sgn * bw * 0.52f * sc, wave = Mathf.Sin(t * 5f + sgn) * 6f * u;
                GUI.color = new Color(0.35f, 0.22f, 0.1f); GUI.DrawTexture(new Rect(px - 3f * u, cy - bh * 1.3f, 6f * u, bh * 1.4f), Texture2D.whiteTexture);
                GUI.color = won ? new Color(0.85f, 0.15f, 0.12f) : new Color(0.3f, 0.3f, 0.35f);
                GUI.DrawTexture(new Rect(px + (sgn < 0 ? -46f * u : 3f * u), cy - bh * 1.3f + wave, 43f * u, 30f * u), Texture2D.whiteTexture);
            }
            // the painted emblem (user 2026-11-12, after «این گرافیک پیروزی … حس موفقیت نمیده و زشته»): a gold star with
            // the ball and laurels for a win, a dented shield with a deflated ball under a rain cloud for a loss; the
            // word is written on its empty ribbon. Without the picture, the old plank below.
            var board = Resources.Load<Texture2D>(won ? "Art/UI/win_board" : "Art/UI/lose_board");
            if (board != null)
            {
                float h = Screen.height * 0.40f * sc, w = h * board.width / board.height;
                var br = new Rect(cx - w * 0.5f, Screen.height * 0.035f + Screen.height * 0.40f * (1f - sc) * 0.5f, w, h);
                GUI.color = Color.white; GUI.DrawTexture(br, board, ScaleMode.ScaleToFit);
                var bs = new GUIStyle(big) { fontSize = Mathf.RoundToInt(h * 0.085f) };
                bs.normal.textColor = won ? new Color(1f, 0.93f, 0.55f) : new Color(0.92f, 0.95f, 1f);
                var ribbon = new Rect(br.x + w * 0.2f, br.y + h * 0.53f, w * 0.6f, h * 0.13f);
                var shadow = new GUIStyle(bs); shadow.normal.textColor = new Color(0.25f, 0.1f, 0.05f, 0.9f);
                GUI.Label(new Rect(ribbon.x + 2f * u, ribbon.y + 3f * u, ribbon.width, ribbon.height), Ar.T(won ? Ar.Won : Ar.Lost), shadow);
                GUI.Label(ribbon, Ar.T(won ? Ar.Won : Ar.Lost), bs);
                GUI.color = old;
                return Screen.height * 0.035f + Screen.height * 0.40f - 20f * u;
            }
            var r = new Rect(cx - bw * sc * 0.5f, cy - bh * sc * 0.5f, bw * sc, bh * sc);
            GUI.color = Color.white;
            var plank = Kit("ribbon");
            if (plank != null) GUI.DrawTexture(r, plank, ScaleMode.StretchToFill);
            var st = new GUIStyle(big) { fontSize = Mathf.RoundToInt(bh * 0.4f * sc) };
            st.normal.textColor = won ? new Color(0.2f, 0.5f, 0.15f) : new Color(0.6f, 0.12f, 0.08f);
            GUI.Label(new Rect(r.x, r.y - r.height * 0.06f, r.width, r.height), Ar.T(won ? Ar.Won : Ar.Lost), st);
            GUI.color = old;
            return cy + bh * 0.7f;
        }
        const float PopSeconds = 1.4f;
        string popIcon; Side popSide; DuelManager.PopKind popKind; float popUntil;
        int gainWindow = -1, gainValue; Side gainSide; float gainUntil;
        void OnTaken(Side side, int w, int v) { gainWindow = w; gainValue = v; gainSide = side; gainUntil = Time.time + 1.3f; }
        void OnPop(string icon, Side side, DuelManager.PopKind kind) { popIcon = icon; popSide = side; popKind = kind; popUntil = Time.time + PopSeconds; }
        void OnToast(string t, float s) { toast = t; toastUntil = Time.time + s; }
        void OnFinished(Side winner, bool iWon) { ended = true; won = iWon; endedAt = Time.time; bannerAt = -10f; }

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
                // the opponent is a bot and says so (docs/STORE_RISK.fa.md §2 #7: a bot that passes for a person invites
                // «deception» reports); a small grey tag under its bar
                if (side != (int)duel.Me)
                {
                    // beside the score (run 218: under the bar the poison badges covered it)
                    var tr = new Rect(left ? x + isz0 + 70 * u : x - 180 * u, top + 8 * u, 56 * u, 24 * u);
                    var o = GUI.color; GUI.color = new Color(0f, 0f, 0f, 0.55f); GUI.DrawTexture(tr, Texture2D.whiteTexture); GUI.color = o;
                    var bt = new GUIStyle(small) { alignment = TextAnchor.MiddleCenter, fontSize = Mathf.RoundToInt(16 * u) }; bt.normal.textColor = new Color(0.85f, 0.85f, 0.85f);
                    GUI.Label(tr, Ar.T("بوت"), bt);
                }
            }
            // the match clock (user 2026-11-13: «تایم کلی بازی ۴ دقیقه … تایمر رو هم ببینیم»): top centre, red in the last 30 s
            if (!ended)
            {
                int left = Mathf.Max(0, Mathf.CeilToInt(duel.MatchTimeLeft));
                string clock = Ar.N(left / 60) + ":" + Ar.N(left % 60 / 10) + Ar.N(left % 10);
                var cs = new GUIStyle(big) { fontSize = Mathf.RoundToInt(40 * u), alignment = TextAnchor.MiddleCenter };
                cs.normal.textColor = left <= 30 ? new Color(1f, 0.35f, 0.3f) : Color.white;
                var cr = new Rect(Screen.width * 0.5f - 80 * u, 6 * u, 160 * u, 54 * u);
                var oc = GUI.color; GUI.color = new Color(0f, 0f, 0f, 0.45f); GUI.DrawTexture(new Rect(cr.x + 14 * u, cr.y + 4 * u, cr.width - 28 * u, cr.height - 8 * u), Texture2D.whiteTexture); GUI.color = oc;
                Shadowed(cr, clock, cs);
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
                // User (2026-11-13): «هر دست … نهایت ۲۰ ثانیه … یه تایمر دیگه داشته باشه و اون رو هم ببینیم و پنج ثانیه مونده
                // … صدای تایمر بلند شه». The seconds beside the clock; the last five red, pulsing and ticking (my turn only).
                int secs = Mathf.Max(0, Mathf.CeilToInt(duel.TurnTimeLeft));
                bool last = secs <= 5 && secs > 0;
                var ts = new GUIStyle(big) { fontSize = Mathf.RoundToInt((last ? 30f + 6f * Mathf.Abs(Mathf.Sin(Time.time * 6f)) : 26f) * u), alignment = TextAnchor.MiddleCenter };
                ts.normal.textColor = last ? new Color(1f, 0.3f, 0.25f) : Color.white;
                Shadowed(new Rect(mine ? tx - 92 * u : tx + tw + 34 * u, ty - 22 * u, 58 * u, 44 * u), Ar.N(secs), ts);
                if (mine && last && secs != lastTick) { lastTick = secs; Tick(secs == 1); }
                if (!last) lastTick = -1;
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

            TurnBanner(u, big);

            if (ended)
            {
                var old = GUI.color; GUI.color = new Color(0f, 0f, 0f, 0.65f);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture); GUI.color = old;
                // the result in pictures: windows, shards won, curses gained or lost (or, in a challenge, the run so far)
                float cx = Screen.width * 0.5f, esz = 56 * u;
                float y0 = ResultBoard(u, big) - 20 * u;
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
            { "carpet", "open_window" }, // until the rug icon is painted
            { "rug_push", "pebble" },    // until its own icon is painted
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
            { "pigeon","bus","wind","open_window","carpet","rug_push","puddle","nails","vane","pebble","plank","fart","rage","pass","surrender","window","shards","curse","clock","replay",
              "truck","cart","rain","complaint","umbrella","flashlight" };

        int lastTick = -1;
        AudioSource tickSrc; AudioClip tickClip, tickLast;
        /// <summary>A short wooden tick (higher and longer on the last second), made in code: no file needed.</summary>
        void Tick(bool final)
        {
            if (tickSrc == null) { tickSrc = gameObject.AddComponent<AudioSource>(); tickSrc.playOnAwake = false; tickSrc.spatialBlend = 0f; }
            if (tickClip == null) { tickClip = Blip("tick", 1300f, 0.07f); tickLast = Blip("tick_last", 1750f, 0.16f); }
            tickSrc.PlayOneShot(final ? tickLast : tickClip, 0.8f);
        }
        static AudioClip Blip(string name, float hz, float seconds)
        {
            const int rate = 44100; int n = Mathf.RoundToInt(rate * seconds);
            var d = new float[n];
            for (int i = 0; i < n; i++) { float t = i / (float)rate; d[i] = Mathf.Sin(2f * Mathf.PI * hz * t) * Mathf.Exp(-t * 38f) * 0.9f; }
            var c = AudioClip.Create(name, n, 1, rate, false); c.SetData(d, 0); return c;
        }

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
