using System.Collections;
using System.Collections.Generic;
using Sheytoonak.Core.Duel;
using UnityEngine;

namespace Sheytoonak.Prototype
{
    /// <summary>
    /// The shared stage seen from behind (SCENARIO v1.3): my three heroes bottom-left, the opponent's
    /// bottom-right, the hero of the move steps forward to the ball and back. Placeholder bodies until the
    /// painted heroes arrive; poisons appear as simple props on the facade / the street.
    /// </summary>
    public class DuelStage : MonoBehaviour
    {
        public DuelManager duel;
        public BackdropWindowMapper mapper;
        public PlayerFoot foot;
        public Material leftMat, rightMat, propMat, shutterMat, puddleMat;

        public readonly Transform[][] Heroes = new Transform[2][];
        readonly Vector3[][] homes = new Vector3[2][];
        readonly Dictionary<ActivePoison, List<GameObject>> props = new Dictionary<ActivePoison, List<GameObject>>();
        GameObject shutter; // the sealed window, shown (review 2026-11-07 §2.2)
        GameObject pigeon;

        void Start()
        {
            for (int side = 0; side < 2; side++)
            {
                Heroes[side] = new Transform[3];
                homes[side] = new Vector3[3];
                float sign = side == 0 ? -1f : 1f;
                for (int i = 0; i < 3; i++)
                {
                    // User (2026-11-11), comparing with the reference game's recordings: «هم سایز بازیکنا … شبیه‌تر بشه». The kids
                    // are much taller now (heroHeight) and stand apart toward the two corners, so the middle of the street (the
                    // ball, the ground-floor windows) stays clear. ⚠️ Run 26 once had them filling the frame from the middle.
                    float x = sign * (2.3f + i * 1.35f);
                    // run 38: the painted kids stood with their legs under the bottom button strip; a step towards the wall lifts them above it
                    var pos = new Vector3(x, 0f, foot.Z + 2.1f - i * 0.25f);
                    homes[side][i] = pos;
                    Heroes[side][i] = MakeHero(side == 0 ? leftMat : rightMat, pos, (side == 0 ? "L" : "R") + i).transform;
                }
            }
            duel.DuelStarted += OnDuelStarted;
            duel.KickLaunched += OnKick;
            duel.Rekicked += OnRekick;
            duel.KickerChosen += OnKickerChosen;
            duel.PigeonHit += OnPigeonHit;
            duel.BallDead += OnBallDead;
            duel.KickerReleased += SendHome;
            duel.Finished += OnFinished;
        }

        void OnDestroy()
        {
            if (duel == null) return;
            duel.DuelStarted -= OnDuelStarted;
            duel.KickLaunched -= OnKick;
            duel.Rekicked -= OnRekick;
            duel.KickerChosen -= OnKickerChosen;
            duel.PigeonHit -= OnPigeonHit;
            duel.BallDead -= OnBallDead;
            duel.KickerReleased -= SendHome;
            duel.Finished -= OnFinished;
        }

        // User (2026-11-07): «این صحنه آدمک‌های شبیه مهره شطرنج رو بردار از بازی کلا هیچ جا نباشه». The capsule+sphere
        // placeholder is gone: a hero is an empty root until its painting (Art/Heroes/<id>_idle.png) is attached in
        // ApplyLooks; a hero with no painting stays invisible rather than turning into a chess piece.
        GameObject MakeHero(Material mat, Vector3 pos, string name)
        {
            var root = new GameObject("Hero " + name);
            root.transform.position = pos;
            return root;
        }

        // ------------------------------------------------------------------ painted heroes

        /// <summary>Art/Heroes/&lt;id&gt;_idle.png and &lt;id&gt;_kick.png (docs/ART_SPEC.fa.md, heroes section).
        /// Drawn facing right, three-quarter back view with about a third of the face showing (user: «یک سوم از صورت
        /// قهرمان رو ببینیم، کامل از پشت نباشه»); the right side is mirrored so both teams face the middle.
        /// No file yet = nothing is drawn for that hero (no placeholder).</summary>
        public float heroHeight = 2.5f; // 2026-11-11: 1.6 → 2.5 (a third of the screen, like the reference game's fighters)
        readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
        readonly SpriteRenderer[][] painted = { new SpriteRenderer[3], new SpriteRenderer[3] };
        readonly string[][] lookIds = { new string[3], new string[3] };
        readonly Vector3[] kickerScale = new Vector3[2];
        readonly Coroutine[] striking = new Coroutine[2];

        Sprite HeroSprite(string id, string pose)
        {
            string key = id + "_" + pose;
            if (sprites.TryGetValue(key, out var sp)) return sp;
            var tex = Resources.Load<Texture2D>("Art/Heroes/" + key);
            sp = tex != null ? Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0f), 100f) : null;
            sprites[key] = sp;
            return sp;
        }

        void ApplyLooks()
        {
            for (int side = 0; side < 2; side++)
                for (int i = 0; i < 3; i++)
                {
                    var hero = Heroes[side][i];
                    string id = duel.Rules.Heroes[side][i].Id;
                    lookIds[side][i] = id;
                    var idle = HeroSprite(id, "idle");
                    if (painted[side][i] != null) { Destroy(painted[side][i].gameObject); painted[side][i] = null; }
                    if (idle == null) continue;
                    var go = new GameObject("Painted " + id);
                    go.transform.SetParent(hero, false);
                    var r = go.AddComponent<SpriteRenderer>();
                    r.sprite = idle;
                    r.flipX = side == 1;
                    go.transform.localScale = Vector3.one * (heroHeight / Mathf.Max(0.01f, idle.bounds.size.y));
                    baseY[r] = go.transform.localScale.y; breathPhase[r] = Random.value * Mathf.PI * 2f;
                    painted[side][i] = r;
                    HeroRig.Build(r, id); // step 4 (2026-11-11): a jointed puppet where the hero has a rig sheet
                    if (i == kickSlot[side]) kickerScale[side] = go.transform.localScale;
                }
        }

        void SetPose(Side side, int slot, string pose)
        {
            var r = painted[(int)side][slot];
            if (r == null) return;
            var sp = HeroSprite(lookIds[(int)side][slot], pose) ?? HeroSprite(lookIds[(int)side][slot], "idle");
            if (sp == null) return;
            r.sprite = sp;
            // frames come from different sheets (a 5-frame sheet is ~1/3 the pixels of a single pose): scale per sprite
            r.transform.localScale = Vector3.one * (heroHeight / Mathf.Max(0.01f, sp.bounds.size.y));
            baseY[r] = r.transform.localScale.y;
            if (slot == kickSlot[(int)side]) kickerScale[(int)side] = r.transform.localScale;
        }

        // User (2026-11-08): «بازیکنان نفس بکشن و یه حرکت ریزی به بالا داشته باشن … در حد یه میلیمتر بالا پایین شن». The
        // sprite's pivot is at the feet, so a ~1 % slow stretch in y lifts the head and chest a touch and settles again.
        readonly Dictionary<SpriteRenderer, float> baseY = new Dictionary<SpriteRenderer, float>();
        readonly Dictionary<SpriteRenderer, float> breathPhase = new Dictionary<SpriteRenderer, float>();
        void Breathe()
        {
            for (int side = 0; side < 2; side++)
                for (int i = 0; i < 3; i++)
                {
                    var r = painted[side][i];
                    if (r == null || !baseY.TryGetValue(r, out float by)) continue;
                    if (striking[side] != null && i == kickSlot[side]) continue; // the kick animation owns the scale
                    breathPhase.TryGetValue(r, out float ph);
                    float b = 1f + 0.028f * (0.5f + 0.5f * Mathf.Sin(Time.time * 2f * Mathf.PI / 3.4f + ph)); // user (2026-11-08): 1.2 % was «خیلی ریز»
                    var ls = r.transform.localScale;
                    r.transform.localScale = new Vector3(ls.x, by * b, ls.z);
                }
        }

        /// <summary>The 6-frame run cycle (r1..r6, 2026-11-04: «حرکت بازیکن‌ها هنوز طبیعی نیست») when painted; otherwise the
        /// two first kick frames as before. <paramref name="cycles"/> counts strides: one stride = the six frames.</summary>
        // Toni has 4 run frames: user (2026-11-05) «طونی عکس اول و سومی از چپ درست نیست، باقی درسته» -- those two were dropped.
        readonly System.Collections.Generic.Dictionary<string, int> runFrames = new System.Collections.Generic.Dictionary<string, int>();
        int RunFrames(string id)
        {
            if (runFrames.TryGetValue(id, out int n)) return n;
            n = 0;
            while (n < 6 && HeroSprite(id, "r" + (n + 1)) != null) n++;
            return runFrames[id] = Mathf.Max(1, n);
        }
        bool HasRun(Side side, int slot) => painted[(int)side][slot] != null && HeroSprite(lookIds[(int)side][slot], "r1") != null;
        void RunPose(Side side, int slot, float cycles, bool kickFrames)
        {
            if (HasRun(side, slot))
            {
                int n = RunFrames(lookIds[(int)side][slot]);
                SetPose(side, slot, "r" + (1 + (int)(cycles * n) % n));
                return;
            }
            if (kickFrames) SetPose(side, slot, (int)(cycles * 4f) % 2 == 0 ? "k1" : "k2");
        }

        /// <summary>True when the hero has the 5-frame kick sheet (k1 run, k2 plant, k3 wind-up, k4 contact, k5 land).</summary>
        bool HasFrames(Side side) => painted[(int)side][K(side)] != null && HeroSprite(lookIds[(int)side][K(side)], "k1") != null;
        int K(Side side) => kickSlot[(int)side];

        void OnDuelStarted()
        {
            for (int sd = 0; sd < 2; sd++)
            {
                if (stepping[sd] != null) { StopCoroutine(stepping[sd]); stepping[sd] = null; }
                if (striking[sd] != null) { StopCoroutine(striking[sd]); striking[sd] = null; }
                kickSlot[sd] = 0; turns[sd] = 0;
                for (int i = 0; i < 3; i++) if (Heroes[sd][i] != null) Heroes[sd][i].position = homes[sd][i];
            }
            ApplyLooks();
            foreach (var kv in props) foreach (var go in kv.Value) if (go != null) Destroy(go);
            props.Clear();
            if (shutter != null) Destroy(shutter);
            shutter = duel.Rules.SealedWindow >= 0 ? PoisonProps.Shutter(mapper.WindowRects[duel.Rules.SealedWindow], mapper.FacadeZ) : null;
            duel.Rules.PoisonPlaced += OnPoison;
            duel.Rules.PoisonExpired += OnExpired;
            duel.Rules.AntidoteUsed += OnAntidote;
            duel.Rules.WindowTaken += (s, w, v) => Refresh();
        }

        // ⚠️ until 2026-11-04 the hero ran to the ball only after the ball had already been launched, always the same hero
        // (slot 0, the one nearest the ball), and walked home while the rebound was still coming. User: «فقط بازیکن نزدیک
        // به توپ میره پشت توپ … تا توپ به شیشه نخورده یا اوت نشده وسط بمونه که توپ برگشتی رو بزنه ولی الان برمیگرده سر جایش».
        // Now: at the start of a side's turn the next hero of that side (they take turns) walks up and stands behind the
        // ball, strikes the moment the ball leaves, stays in the middle for the whole turn (rebounds too) and only walks
        // home when the turn passes to the other side.
        readonly int[] kickSlot = new int[2], turns = new int[2];
        readonly Coroutine[] stepping = new Coroutine[2];

        /// <summary>A hero was chosen to kick: the one already at the ball (if another) walks home, the chosen one walks up.</summary>
        void OnKickerChosen(Side side, int slot)
        {
            if (stepping[(int)side] != null && kickSlot[(int)side] == slot) { duel.KickerArrived(side); return; }
            SendHome(side);
            kickSlot[(int)side] = slot;
            var r = painted[(int)side][slot];
            if (r != null) kickerScale[(int)side] = r.transform.localScale;
            stepping[(int)side] = StartCoroutine(StandAtBall(side));
        }

        void OnFinished(Side winner, bool iWon) { SendHome(Side.Left); SendHome(Side.Right); }

        void SendHome(Side side)
        {
            if (stepping[(int)side] == null) return;
            StopCoroutine(stepping[(int)side]);
            stepping[(int)side] = null;
            StartCoroutine(WalkHome(side, kickSlot[(int)side])); // not tracked: a newly chosen hero may walk up at the same time
        }

        /// <summary>Behind and to the side of the ball (right-footers stand left of it), so the ball itself stays visible.</summary>
        Vector3 Spot(Side side)
        {
            float off = side == Side.Left ? -0.75f : 0.75f; // 0.45 → 0.75 with the taller kids (2026-11-11), so the ball stays in sight
            Vector3 b = foot.StrikeSpot; // both kickers hold their line; ballX is only for the shadow/ring marks
            // a puppet stands so its toe, at the contact angle, touches the back of the ball (HeroRig.FootOffset), on the
            // ball's own depth (user 2026-11-11: «توپ … باید بخوره حتماً به پای بازیکن … چند سانت اون طرف‌تره»)
            var pr = painted[(int)side][K(side)];
            var rig = pr != null ? pr.GetComponent<HeroRig>() : null;
            if (rig != null)
            {
                Vector3 fo = rig.FootOffset(HeroRig.ContactDeg);
                float r = duel.ball != null ? duel.ball.Radius : 0.11f;
                return new Vector3(b.x - fo.x - Mathf.Sign(fo.x) * r * 0.7f, 0f, b.z - 0.04f);
            }
            return new Vector3(b.x + off, 0f, b.z - 0.35f);
        }

        IEnumerator StandAtBall(Side side)
        {
            int slot = K(side);
            var hero = Heroes[(int)side][slot];
            Vector3 home = homes[(int)side][slot];
            bool frames = HasFrames(side);
            for (float k = 0f; k < 1f; k += Time.deltaTime * 2.4f)
            {
                hero.position = Vector3.Lerp(home, Spot(side), Mathf.SmoothStep(0f, 1f, k)) + Vector3.up * Mathf.Abs(Mathf.Sin(k * Mathf.PI * 3f)) * 0.06f;
                RunPose(side, slot, k * 1.6f, frames);
                yield return null;
            }
            if (frames || HasRun(side, slot)) SetPose(side, slot, "idle");
            var readyRig = painted[(int)side][slot] != null ? painted[(int)side][slot].GetComponent<HeroRig>() : null;
            if (readyRig != null) readyRig.Ready = true; // wound up behind the ball until the kick
            duel.KickerArrived(side);
            // waits behind the ball, follows it (my finger moves the ball spot; the rebound moves the ball)
            while (true)
            {
                // User (2026-11-07): «بازیکنان وقتی پشت توپن بعد از ضربه الکی به راست و چپ حرکت می‌کنن». The kicker no
                // longer chases the ball's x during the rally (the rebound now lands on the kicker's own line, see
                // DuelManager.SendBack); he only drifts to his spot, slowly.
                // ⚠️ not while kicking: a rebound kick moves him to the ball, and drifting back mid-kick slid the foot off it
                var kr = painted[(int)side][slot] != null ? painted[(int)side][slot].GetComponent<HeroRig>() : null;
                if (kr == null || !kr.Kicking)
                    hero.position = Vector3.MoveTowards(hero.position, Spot(side), 1.5f * Time.deltaTime);
                yield return null;
            }
        }

        IEnumerator WalkHome(Side side, int slot)
        {
            var hero = Heroes[(int)side][slot];
            if (striking[(int)side] != null) { StopCoroutine(striking[(int)side]); striking[(int)side] = null; }
            var pr = painted[(int)side][slot];
            if (pr != null) { pr.transform.localPosition = Vector3.zero; pr.transform.localRotation = Quaternion.identity; }
            var homeRig = pr != null ? pr.GetComponent<HeroRig>() : null;
            if (homeRig != null) homeRig.Ready = false;
            bool frames = HasFrames(side);
            Vector3 from = hero.position, home = homes[(int)side][slot];
            for (float k = 0f; k < 1f; k += Time.deltaTime * 2.6f)
            {
                hero.position = Vector3.Lerp(from, home, k) + Vector3.up * Mathf.Abs(Mathf.Sin(k * Mathf.PI * 4f)) * 0.05f;
                RunPose(side, slot, k * 2f, frames);
                if (pr != null && (frames || HasRun(side, slot))) pr.flipX = side == Side.Left; // walking back faces home
                yield return null;
            }
            hero.position = home;
            if (pr != null) { pr.flipX = side == Side.Right; SetPose(side, slot, "idle"); }
            yield break;
        }

        void OnKick(Side side, int target)
        {
            // the chosen hero is already at the ball (a kick needs it); a kick without one (tests) uses the current slot
            if (stepping[(int)side] == null) OnKickerChosen(side, K(side));
            var hero = Heroes[(int)side][K(side)];
            ToeToBall(side, hero); // the ball may not be exactly where the kicker was walking to
            if (striking[(int)side] != null) StopCoroutine(striking[(int)side]);
            striking[(int)side] = StartCoroutine(Strike(hero, side));
            if (duel.Rules.ModifiersFor(side).Chance > 0) StartCoroutine(PigeonCrossing());
        }

        float ballX;
        SpriteRenderer ballShadow, ballRing;

        void LateUpdate()
        {
            Breathe();
            if (duel == null || duel.ball == null) return;
            ballX = Mathf.Lerp(ballX, duel.ball.transform.position.x, 4f * Time.deltaTime);
            BallMarks();
        }

        /// <summary>
        /// User (2026-11-04): «توپ رو اولین ضربه که میخوای بزنی بهش نمی‌بینی کجاست». A shadow under the ball always,
        /// and on my turn a pulsing ring round the resting ball, so the eye finds it among the kids' legs.
        /// </summary>
        void BallMarks()
        {
            if (ballShadow == null) { ballShadow = GroundSprite("Ball shadow", Disc(64, false), new Color(0f, 0f, 0f, 0.45f)); ballRing = GroundSprite("Ball ring", Disc(96, true), new Color(1f, 0.95f, 0.5f, 0.9f)); }
            var b = duel.ball;
            bool on = b.gameObject.activeSelf && duel.Rules != null && !duel.Rules.Finished;
            ballShadow.enabled = on;
            Vector3 p = b.transform.position;
            float r = b.Radius > 0f ? b.Radius : 0.11f;
            if (on)
            {
                float lift = Mathf.Clamp01(p.y / 3f);
                ballShadow.transform.position = new Vector3(p.x, 0.01f, p.z);
                ballShadow.transform.localScale = Vector3.one * r * 2.4f * (1f + lift) / 0.64f;
                ballShadow.color = new Color(0f, 0f, 0f, 0.45f * (1f - 0.7f * lift));
            }
            bool resting = on && duel.Rules.Current == duel.Me && duel.KickArmed && !duel.RallyActive && b.Body != null && b.Body.isKinematic;
            ballRing.enabled = resting;
            if (resting)
            {
                float pulse = 1f + 0.15f * Mathf.Sin(Time.time * 5f);
                ballRing.transform.position = new Vector3(p.x, 0.015f, p.z);
                ballRing.transform.localScale = Vector3.one * r * 4.2f * pulse / 0.96f;
            }
        }

        SpriteRenderer GroundSprite(string name, Sprite sp, Color c)
        {
            var go = new GameObject(name);
            go.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sp; sr.color = c;
            return sr;
        }

        static Sprite Disc(int n, bool ring)
        {
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            var px = new Color32[n * n];
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float d = new Vector2(x - n * 0.5f + 0.5f, y - n * 0.5f + 0.5f).magnitude / (n * 0.5f);
                    float a = ring ? Mathf.Clamp01(1f - Mathf.Abs(d - 0.85f) / 0.1f) : Mathf.Clamp01(1f - d * d);
                    px[y * n + x] = new Color32(255, 255, 255, (byte)(a * 255));
                }
            tex.SetPixels32(px); tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), 100f);
        }

        /// <summary>Crouch, swing (kick pose, lean into the shot, a little hop), settle back to idle.</summary>
        readonly float[] kickFrom = { HeroRig.ContactDeg, HeroRig.ContactDeg };

        IEnumerator Strike(Transform hero, Side side)
        {
            var rig = painted[(int)side][K(side)] != null ? painted[(int)side][K(side)].GetComponent<HeroRig>() : null;
            if (rig != null)
            {
                // the puppet kicks with its own joints (HeroRig); no frame swapping, no squash of the whole picture
                rig.Kick(kickFrom[(int)side]);
                kickFrom[(int)side] = HeroRig.ContactDeg;
                while (rig != null && rig.Kicking) yield return null;
                striking[(int)side] = null;
                yield break;
            }
            if (HasFrames(side))
            {
                // painted sequence (user: «پنج تا تصویر رو به شکل کمیک از زدن یه توپ»): plant, wind-up, contact, land
                int slot = K(side);
                var pr = painted[(int)side][slot];
                // the ball has already left the foot: a very short wind-up, then contact (a long wind-up looked like
                // the hero kicking air after the ball)
                SetPose(side, slot, "k3"); yield return new WaitForSeconds(0.06f);
                // contact frame; where the sheet's contact frame was rejected (user: the girls' «زاویه پاشون خوب نیست») the single kick pose stands in
                SetPose(side, slot, HeroSprite(lookIds[(int)side][slot], "k4") != null ? "k4" : "kick");
                for (float k = 0f; k < 1f && pr != null; k += Time.deltaTime / 0.18f) { pr.transform.localPosition = new Vector3(0f, 0.06f * Mathf.Sin(k * Mathf.PI), 0f); yield return null; }
                if (pr != null) pr.transform.localPosition = Vector3.zero;
                SetPose(side, slot, "k5"); yield return new WaitForSeconds(0.22f);
                SetPose(side, slot, "idle");
                striking[(int)side] = null;
                yield break;
            }
            int s0 = K(side);
            var r = painted[(int)side][s0];
            Transform body = r != null ? r.transform : hero;
            Vector3 baseScale = r != null ? kickerScale[(int)side] : Vector3.one; // never read mid-animation (a quick re-kick would drift)
            float lean = side == Side.Left ? -1f : 1f;
            // anticipation
            for (float k = 0f; k < 1f; k += Time.deltaTime * 9f) { body.localScale = new Vector3(baseScale.x * (1f + 0.06f * k), baseScale.y * (1f - 0.08f * k), baseScale.z); yield return null; }
            SetPose(side, s0, "kick");
            // the strike: stretch up, lean forward toward the wall, hop
            for (float k = 0f; k < 1f; k += Time.deltaTime * 5f)
            {
                float e = Mathf.Sin(k * Mathf.PI);
                body.localScale = new Vector3(baseScale.x * (1f - 0.04f * e), baseScale.y * (1f + 0.06f * e), baseScale.z);
                body.localRotation = Quaternion.Euler(0f, 0f, lean * 8f * e);
                body.localPosition = new Vector3(0f, 0.12f * e, 0f);
                yield return null;
            }
            yield return new WaitForSeconds(0.15f);
            SetPose(side, s0, "idle");
            for (float k = 0f; k < 1f; k += Time.deltaTime * 6f)
            {
                body.localScale = Vector3.Lerp(new Vector3(baseScale.x * 1.04f, baseScale.y * 0.95f, baseScale.z), baseScale, k);
                yield return null;
            }
            body.localScale = baseScale; body.localRotation = Quaternion.identity; body.localPosition = Vector3.zero;
            striking[(int)side] = null;
        }

        /// <summary>
        /// Stands the kicking puppet so its toe meets the ball where the ball is now, at the ball's own height, and picks
        /// the leg angle the kick starts from. User (2026-11-12): «ضربه دوم و سوم و چهارم رو هم درست کن بخوره به پاش
        /// حتماً» -- a rebound left from wherever the ball was while the hero kicked from his old spot.
        /// </summary>
        void ToeToBall(Side side, Transform hero)
        {
            var pr = painted[(int)side][K(side)];
            var rig = pr != null ? pr.GetComponent<HeroRig>() : null;
            if (rig == null || duel.ball == null) return;
            Vector3 b = duel.ball.transform.position;
            float r = duel.ball.Radius;
            float deg = rig.ContactAngleFor(b);
            Vector3 fo = rig.FootOffset(deg);
            hero.position = new Vector3(b.x - fo.x - Mathf.Sign(fo.x) * r * 0.7f, hero.position.y, b.z - 0.04f);
            kickFrom[(int)side] = deg;
        }

        void OnRekick(Side side)
        {
            var hero = Heroes[(int)side][K(side)];
            if (hero == null) return;
            ToeToBall(side, hero); // the rebound is struck where it is (the kid runs to it)
            if (striking[(int)side] != null) StopCoroutine(striking[(int)side]);
            striking[(int)side] = StartCoroutine(Strike(hero, side));
        }

        bool pigeonDown;
        readonly List<GameObject> flock = new List<GameObject>();
        /// <param name="birds">the poison's level (user 2026-11-09: «پرنده وقتی بیشتر شه تعداد پرنده بیشتر می‌شه»): the
        /// first one is the bird the ball can hit, the others fly with it, a little behind and above or below.</param>
        IEnumerator PigeonCrossing(int birds = 1)
        {
            if (pigeon == null) pigeon = PoisonProps.Pigeon();
            pigeon.SetActive(true);
            pigeonDown = false;
            while (flock.Count < birds - 1) flock.Add(PoisonProps.Pigeon());
            for (int i = 0; i < flock.Count; i++) flock[i].SetActive(i < birds - 1);
            float z = mapper.FacadeZ - 4f, y = 2.8f;
            for (float t = 0f; t < 1f && !pigeonDown; t += Time.deltaTime / 1.1f)
            {
                pigeon.transform.position = new Vector3(Mathf.Lerp(-7f, 7f, t), y + Mathf.Sin(t * 20f) * 0.15f, z);
                for (int i = 0; i < birds - 1 && i < flock.Count; i++)
                {
                    float lag = 0.55f * (i / 2 + 1), dy = (i % 2 == 0 ? 0.55f : -0.45f) * (i / 2 + 1);
                    flock[i].transform.position = new Vector3(Mathf.Lerp(-7f, 7f, t) - lag, y + dy + Mathf.Sin(t * 20f + i * 1.7f) * 0.15f, z + 0.3f * (i + 1));
                }
                yield return null;
            }
            // the rest of the flock flies on and away
            if (birds > 1) StartCoroutine(FlockAway(birds - 1, y));
            // User (2026-11-05): «توپ وقتی می‌خوره به کبوتر، کبوتر نمی‌خوره زمین و به راهش ادامه می‌ده … باید بخوره زمین».
            // Hit: it tumbles to the road where the ball met it, lies there a moment, then walks off.
            if (pigeonDown)
            {
                Vector3 from = pigeon.transform.position; float ground = 0.12f;
                for (float t = 0f; t < 1f; t += Time.deltaTime / 0.55f)
                {
                    pigeon.transform.position = new Vector3(from.x + t * 0.6f, Mathf.Lerp(from.y, ground, t * t), from.z);
                    pigeon.transform.rotation = Quaternion.Euler(0f, 0f, t * 540f);
                    yield return null;
                }
                pigeon.transform.rotation = Quaternion.Euler(0f, 0f, 90f); // on its side
                // User (2026-11-09): «وقتی توپ می‌خوره به پرنده باید یه صدایی داشته باشه موقع سقوط و خونش ریخته بشه و بعدش
                // ماشین تمیزکن شهرداری بیاد و جارو کنه و جنازه پرنده رو برداره». The bird no longer gets up: a copy stays
                // dead on the road with a stain, the city sweeper comes for it; the live pigeon object is free again at once.
                var corpse = Instantiate(pigeon);
                StartCoroutine(PigeonCorpse(corpse.transform));
            }
            pigeon.transform.rotation = Quaternion.identity;
            pigeon.SetActive(false);
            pigeonDown = false;
        }

        IEnumerator FlockAway(int n, float y)
        {
            for (float t = 0f; t < 1.2f; t += Time.deltaTime)
            {
                for (int i = 0; i < n && i < flock.Count; i++)
                    if (flock[i].activeSelf) flock[i].transform.position += new Vector3(12f, 1.5f, 0f) * Time.deltaTime;
                yield return null;
            }
            for (int i = 0; i < n && i < flock.Count; i++) flock[i].SetActive(false);
        }

        IEnumerator PigeonCorpse(Transform corpse)
        {
            var sr = corpse.GetComponentInChildren<SpriteRenderer>();
            var blood = new GameObject("Pigeon blood").AddComponent<SpriteRenderer>();
            blood.sprite = StreetLife.BloodSprite();
            blood.transform.SetParent(corpse, true);
            blood.transform.position = corpse.position + new Vector3(0.05f, -0.06f, 0.01f);
            blood.transform.rotation = Quaternion.identity;
            if (sr != null) blood.sortingOrder = sr.sortingOrder;
            blood.enabled = StreetLife.ShowBlood;
            for (float t = 0f; t < 0.6f; t += Time.deltaTime) { blood.transform.localScale = Vector3.one * Mathf.Lerp(0.3f, 1.4f, t / 0.6f) / Mathf.Max(0.01f, corpse.lossyScale.x); yield return null; }
            yield return new WaitForSeconds(1.2f);
            var truckT = Resources.Load<Texture2D>("Art/Props/sweeper");
            if (truckT == null)
            {
                // no sweeper picture yet: the bird just fades from the road
                for (float t = 0f; t < 1f; t += Time.deltaTime) { foreach (var r in corpse.GetComponentsInChildren<SpriteRenderer>()) r.color = new Color(1f, 1f, 1f, 1f - t); yield return null; }
                Destroy(corpse.gameObject);
                yield break;
            }
            // painted facing left like the ambulance; it comes from the far side, stops with its brush over the bird, sweeps
            bool fromLeft = corpse.position.x > 0f;
            var sp = Sprite.Create(truckT, new Rect(0, 0, truckT.width, truckT.height), new Vector2(0.5f, 0f), 100f);
            var truck = new GameObject("Sweeper").AddComponent<SpriteRenderer>();
            truck.sprite = sp;
            float k = 2.4f / Mathf.Max(0.01f, sp.bounds.size.y);
            truck.transform.localScale = new Vector3(fromLeft ? -k : k, k, k);
            float z = corpse.position.z - 0.6f;
            // the painted brush of sweeper.png sits at 11.9 % of the width from the front and 13.4 % up from the wheels'
            // line, 23.2 % of the width wide (cartoon repaint 2026-11-12, measured on the picture; was 11.8/9/22.5); the
            // separate sweeper_brush.png is laid exactly over it
            var brushLocal = new Vector3(sp.bounds.size.x * (0.119f - 0.5f), sp.bounds.size.y * 0.134f, -0.01f);
            float stopX = corpse.position.x - brushLocal.x * truck.transform.localScale.x; // brush right over the bird
            float x0 = fromLeft ? -26f : 26f;
            truck.transform.position = new Vector3(x0, 0f, z);
            var brushT = Resources.Load<Texture2D>("Art/Props/sweeper_brush");
            SpriteRenderer brushR = null;
            if (brushT != null)
            {
                brushR = new GameObject("Brush").AddComponent<SpriteRenderer>();
                brushR.sprite = Sprite.Create(brushT, new Rect(0, 0, brushT.width, brushT.height), new Vector2(0.5f, 0.5f), 100f);
                brushR.sortingOrder = truck.sortingOrder + 1;
                brushR.transform.SetParent(truck.transform, false);
                brushR.transform.localPosition = brushLocal;
                brushR.transform.localScale = Vector3.one * (sp.bounds.size.x * 0.232f / Mathf.Max(0.01f, brushR.sprite.bounds.size.x));
            }
            // ⚠️ the brush is painted from the side (a cone of bristles), so turning the sprite would tip it over; it
            // "spins" by flipping its bristles and a slight wobble instead
            System.Action<float> spin = speed =>
            {
                if (brushR == null) return;
                brushR.flipX = ((int)(Time.time * speed)) % 2 == 0;
                var l = brushR.transform.localScale; brushR.transform.localScale = new Vector3(Mathf.Abs(l.y) * (1f + 0.04f * Mathf.Sin(Time.time * speed * 3f)), l.y, l.z);
            };
            for (float t = 0f; t < 1f; t += Time.deltaTime / 2.2f)
            {
                truck.transform.position = new Vector3(Mathf.Lerp(x0, stopX, 1f - (1f - t) * (1f - t)), 0f, z);
                spin(14f);
                yield return null;
            }
            for (float t = 0f; t < 1.2f; t += Time.deltaTime)
            {
                spin(20f);
                if (corpse != null && t > 0.5f) { Destroy(corpse.gameObject); corpse = null; }
                yield return null;
            }
            float xe = fromLeft ? 26f : -26f;
            for (float t = 0f; t < 1f; t += Time.deltaTime / 2.4f)
            {
                truck.transform.position = new Vector3(Mathf.Lerp(stopX, xe, t * t), 0f, z);
                spin(14f);
                yield return null;
            }
            Destroy(truck.gameObject);
        }

        void OnPigeonHit(Vector3 at)
        {
            if (pigeon == null || !pigeon.activeSelf) return;
            // the bird's cry as it falls: Audio/Fx/pigeon_* (any number, random)
            var cries = new List<AudioClip>();
            foreach (var c in Resources.LoadAll<AudioClip>("Audio/Fx")) if (c.name.StartsWith("pigeon")) cries.Add(c);
            if (cries.Count > 0 && Camera.main != null) AudioSource.PlayClipAtPoint(cries[Random.Range(0, cries.Count)], Camera.main.transform.position, 0.9f);
            pigeon.transform.position = new Vector3(at.x, Mathf.Max(0.5f, at.y), pigeon.transform.position.z);
            pigeonDown = true;
        }

        /// <summary>
        /// The action is over: the ball goes limp where it lies (two soft bounces, a little squashed, as if it lost some
        /// air), then rolls back to the kicker's foot; only then can the next kick start.
        /// </summary>
        void OnBallDead(Vector3 at)
        {
            if (settling != null) StopCoroutine(settling);
            settling = StartCoroutine(SettleBall(at, duel.DeadBallRolls ? 0 : 2));
        }
        Coroutine settling;
        /// <param name="hops">0 when the ball already bounced itself (user 2026-11-08: no fake bounces after the real ones)</param>
        IEnumerator SettleBall(Vector3 at, int hops)
        {
            var ball = duel.ball;
            var t = ball.transform;
            // ⚠️ run 89: the ball is a sphere scaled to its diameter, so "localScale = one" blew it up to a metre wide
            // after every action; the squash is a factor on the real scale, not an absolute scale.
            Vector3 baseScale = Vector3.one * (ball.Radius * 2f);
            ball.PlaceAtRest(new Vector3(at.x, ball.Radius, at.z));
            Vector3 home = foot.StrikeSpot + Vector3.up * ball.Radius;
            float hop = 0.35f, dur = 0.35f;
            Vector3 p = t.position;
            for (int bounce = 0; bounce < hops; bounce++)
            {
                Vector3 to = Vector3.Lerp(p, home, 0.25f);
                for (float k = 0f; k < 1f; k += Time.deltaTime / dur)
                {
                    float h = hop * 4f * k * (1f - k);
                    t.position = new Vector3(Mathf.Lerp(p.x, to.x, k), ball.Radius + h, Mathf.Lerp(p.z, to.z, k));
                    t.localScale = baseScale; yield return null;
                }
                // squash on landing: the "air going out" look
                for (float k = 0f; k < 1f; k += Time.deltaTime / 0.12f)
                {
                    float sq = 1f - 0.22f * Mathf.Sin(k * Mathf.PI);
                    t.localScale = Vector3.Scale(baseScale, new Vector3(1f + (1f - sq) * 0.6f, sq, 1f + (1f - sq) * 0.6f));
                    t.position = new Vector3(to.x, ball.Radius * sq, to.z); yield return null;
                }
                t.localScale = baseScale;
                p = to; hop *= 0.45f; dur *= 0.8f;
            }
            // roll the rest of the way, slowing down
            Vector3 start = p; float dist = Vector3.Distance(start, home);
            // the roll home: ~1.4 s cap when the ball was dropped (hops), but a ball that bounced itself and is rolling in
            // from far away takes its time (user 2026-11-08: the third kick's return «چرت و پرت نشون می‌ده»)
            float rollDur = hops > 0 ? Mathf.Clamp(dist * 0.25f, 0.5f, 1.4f) : Mathf.Clamp(dist * 0.45f, 0.4f, 3f);
            for (float k = 0f; k < 1f; k += Time.deltaTime / rollDur)
            {
                float e = 1f - (1f - k) * (1f - k);
                t.position = Vector3.Lerp(start, home, e);
                t.Rotate(Vector3.right, dist * e * 40f * Time.deltaTime, Space.World);
                yield return null;
            }
            t.rotation = Quaternion.identity;
            settling = null;
            duel.BallReturned();
        }

        /// <summary>CI screenshots only: hangs a level-3 rug as the poison would, without touching the rules.</summary>
        GameObject shotCarpet;
        public void CarpetForShot()
        {
            var p = duel.CarpetForShot();
            if (p.Windows.Length > 0) shotCarpet = PoisonProps.Carpet(this, duel.CarpetSpan(p), mapper.FacadeZ);
        }
        /// <summary>CI screenshots only: the antidote pushes that rug off the roof.</summary>
        public void PushCarpetForShot() { if (shotCarpet != null) StartCoroutine(RugFall(shotCarpet)); }

        void OnPoison(Side by, ActivePoison p)
        {
            var list = new List<GameObject>();
            switch (p.Tool.Effect)
            {
                case Effect.Cover:
                    if (p.Windows.Length > 0)
                    {
                        var rects = new Rect[p.Windows.Length];
                        for (int i = 0; i < rects.Length; i++) rects[i] = mapper.WindowRects[p.Windows[i]];
                        list.Add(PoisonProps.Bus(this, rects, mapper.FacadeZ, p.Tool.Id, p.Tool.Power));
                    }
                    break;
                case Effect.Close:
                    if (p.Tool.Id == "carpet") { if (p.Windows.Length > 0) list.Add(PoisonProps.Carpet(this, duel.CarpetSpan(p), mapper.FacadeZ)); }
                    else foreach (int w in p.Windows) list.Add(PoisonProps.OpenWindow(mapper.WindowRects[w], mapper.FacadeZ));
                    break;
                case Effect.Reach:
                    // a little towards the road: right at the ball it sat under the HUD turn line (run 30)
                    list.Add(PoisonProps.Puddle(foot.StrikeSpot + new Vector3(0f, 0f, 2.6f), p.Tool.Power));
                    if (p.Tool.Id == "rain") { var cloud = PoisonProps.Rain(this, new Vector3(0f, 4.2f, mapper.FacadeZ - 3f), p.Tool.Power); if (cloud != null) list.Add(cloud); }
                    break;
                case Effect.Accuracy:
                    list.Add(PoisonProps.Wind(this, mapper.FacadeZ, p.Tool.Power));
                    break;
                case Effect.Chance:
                    StartCoroutine(PigeonsWhile(p));
                    break;
            }
            props[p] = list;
        }

        /// <summary>The pigeon poison: birds keep crossing the stage while it lasts.</summary>
        IEnumerator PigeonsWhile(ActivePoison p)
        {
            while (duel.Rules != null && duel.Rules.Poisons.Contains(p))
            {
                yield return PigeonCrossing(Mathf.Clamp(p.Tool.Power, 1, 5));
                yield return new WaitForSeconds(0.6f);
            }
        }

        void OnAntidote(Side s, Tool a)
        {
            var hero = Heroes[(int)s] != null ? Heroes[(int)s][0] : null;
            var go = PoisonProps.Antidote(a, hero != null ? hero.position : foot.StrikeSpot, mapper.FacadeZ);
            Destroy(go, 2.5f);
        }

        void OnExpired(ActivePoison p)
        {
            if (!props.TryGetValue(p, out var list)) return;
            // User (2026-11-05): «اتوبوس و گاری … باید از چپ بیان از راست خارج شن؛ الآن غیب می‌شن». A vehicle drives off to
            // the right before it is destroyed; everything else still vanishes.
            foreach (var go in list)
                if (go != null)
                {
                    if (go.name == "Poison bus") StartCoroutine(DriveOff(go)); // bus, lorry and cart all build under this name
                    // turns still left = an antidote lifted it: the rug is pushed off the roof; run out = rolled back up
                    else if (go.name == "Poison carpet") StartCoroutine(p.TurnsLeft > 0 ? RugFall(go) : RugUp(go));
                    else Destroy(go);
                }
            props.Remove(p);
        }

        /// <summary>
        /// The antidote of the rug (user 2026-11-10): «این هل بده فرش رو از پشت بوم و بندازه رو زمین». Someone appears on the
        /// roof over the rug (Art/Poison/rug_pusher_0/1 when painted: leaning, then shoving), the rug lets go, drops with
        /// gravity, tips a little, slumps on the pavement and fades.
        /// </summary>
        IEnumerator RugFall(GameObject go)
        {
            var t = go.transform; Vector3 top = t.position;
            var f0 = Resources.Load<Texture2D>("Art/Poison/rug_pusher_0"); var f1 = Resources.Load<Texture2D>("Art/Poison/rug_pusher_1");
            SpriteRenderer pusher = null;
            if (f0 != null)
            {
                pusher = new GameObject("Rug pusher").AddComponent<SpriteRenderer>();
                pusher.sprite = Sprite.Create(f0, new Rect(0, 0, f0.width, f0.height), new Vector2(0.5f, 0f), 100f);
                pusher.transform.position = top + new Vector3(0f, -0.55f, -0.02f); // her hands (and the rug's edge in them) at the parapet
                pusher.transform.localScale = Vector3.one * (1.5f / pusher.sprite.bounds.size.y);
                pusher.sortingOrder = -1;
                yield return new WaitForSeconds(0.35f);
                if (f1 != null && pusher != null) pusher.sprite = Sprite.Create(f1, new Rect(0, 0, f1.width, f1.height), new Vector2(0.5f, 0f), 100f);
            }
            yield return new WaitForSeconds(0.15f);
            float v = 0f, tilt = Random.Range(-8f, 8f);
            // its length: from its top (the pivot, at the roof) to the lowest painted pixel
            float len = 0f;
            foreach (var r in go.GetComponentsInChildren<Renderer>()) len = Mathf.Max(len, top.y - r.bounds.min.y);
            len = Mathf.Max(0.5f, len);
            while (t != null && t.position.y - len > 0.02f) // falls until its bottom edge meets the street
            {
                v += 9.8f * Time.deltaTime;
                t.position += Vector3.down * v * Time.deltaTime + Vector3.back * 0.6f * Time.deltaTime;
                t.rotation = Quaternion.Euler(0f, 0f, Mathf.MoveTowards(t.eulerAngles.z > 180f ? t.eulerAngles.z - 360f : t.eulerAngles.z, tilt, 20f * Time.deltaTime));
                yield return null;
            }
            // slumps: it was hanging from its top, now it lies in a heap
            for (float k = 0f; k < 1f && t != null; k += Time.deltaTime / 0.35f)
            {
                float sy = 1f - 0.8f * k;                         // the top comes down onto the bottom, which stays on the street
                t.localScale = new Vector3(1f + 0.15f * k, sy, 1f);
                t.position = new Vector3(t.position.x, len * sy + 0.02f, t.position.z);
                yield return null;
            }
            if (pusher != null) Destroy(pusher.gameObject);
            yield return new WaitForSeconds(1.2f);
            for (float k = 0f; k < 1f && t != null; k += Time.deltaTime / 0.6f)
            {
                foreach (var r in t.GetComponentsInChildren<SpriteRenderer>()) r.color = new Color(1f, 1f, 1f, 1f - k);
                yield return null;
            }
            if (go != null) Destroy(go);
        }

        /// <summary>The rug's time ran out: it is pulled back up onto the roof (its hanging part shrinks upward).</summary>
        IEnumerator RugUp(GameObject go)
        {
            var t = go.transform;
            for (float k = 0f; k < 1f && t != null; k += Time.deltaTime / 0.9f) { t.localScale = new Vector3(1f, Mathf.Max(0.02f, 1f - k * k), 1f); yield return null; }
            if (go != null) Destroy(go);
        }

        IEnumerator DriveOff(GameObject go)
        {
            var t = go.transform; Vector3 from = t.position, to = from + Vector3.right * 34f;
            for (float k = 0f; k < 1f && t != null; k += Time.deltaTime / 1.4f)
            {
                float e = k * k;
                t.position = Vector3.Lerp(from, to, e);
                yield return null;
            }
            if (go != null) Destroy(go);
        }

        /// <summary>A covered window that broke anyway (never happens) or expired props: nothing to do yet.</summary>
        void Refresh() { }

        static GameObject Box(string name, Vector3 pos, Vector3 size, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(go.GetComponent<Collider>());
            go.name = name;
            go.transform.position = pos;
            go.transform.localScale = size;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            return go;
        }
    }
}
