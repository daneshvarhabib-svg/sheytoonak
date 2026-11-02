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
                    // smaller and a little ahead of the ball so the facade stays the centre of the picture (run 26 had them filling the frame)
                    float x = sign * (1.1f + i * 0.7f);
                    // run 38: the painted kids stood with their legs under the bottom button strip; a step towards the wall lifts them above it
                    var pos = new Vector3(x, 0f, foot.Z + 2.1f - i * 0.25f);
                    homes[side][i] = pos;
                    Heroes[side][i] = MakeHero(side == 0 ? leftMat : rightMat, pos, (side == 0 ? "L" : "R") + i).transform;
                }
            }
            duel.DuelStarted += OnDuelStarted;
            duel.KickLaunched += OnKick;
            duel.Rekicked += OnRekick;
        }

        void OnDestroy()
        {
            if (duel == null) return;
            duel.DuelStarted -= OnDuelStarted;
            duel.KickLaunched -= OnKick;
            duel.Rekicked -= OnRekick;
        }

        GameObject MakeHero(Material mat, Vector3 pos, string name)
        {
            var root = new GameObject("Hero " + name);
            root.transform.position = pos;
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            Object.Destroy(body.GetComponent<Collider>());
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            body.transform.localScale = new Vector3(0.34f, 0.55f, 0.34f);
            body.GetComponent<Renderer>().sharedMaterial = mat;
            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Object.Destroy(head.GetComponent<Collider>());
            head.transform.SetParent(root.transform, false);
            head.transform.localPosition = new Vector3(0f, 1.22f, 0f);
            head.transform.localScale = Vector3.one * 0.26f;
            head.GetComponent<Renderer>().sharedMaterial = mat;
            return root;
        }

        // ------------------------------------------------------------------ painted heroes

        /// <summary>Art/Heroes/&lt;id&gt;_idle.png and &lt;id&gt;_kick.png (docs/ART_SPEC.fa.md, heroes section).
        /// Drawn facing right, three-quarter back view with about a third of the face showing (user: «یک سوم از صورت
        /// قهرمان رو ببینیم، کامل از پشت نباشه»); the right side is mirrored so both teams face the middle.
        /// No file yet = the capsule stays.</summary>
        public float heroHeight = 1.6f; // a bit over real kid height so they read on a phone
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
                    foreach (Transform c in hero) c.gameObject.SetActive(idle == null); // the capsule placeholder
                    if (idle == null) continue;
                    var go = new GameObject("Painted " + id);
                    go.transform.SetParent(hero, false);
                    var r = go.AddComponent<SpriteRenderer>();
                    r.sprite = idle;
                    r.flipX = side == 1;
                    go.transform.localScale = Vector3.one * (heroHeight / Mathf.Max(0.01f, idle.bounds.size.y));
                    painted[side][i] = r;
                    if (i == 0) kickerScale[side] = go.transform.localScale;
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
            if (slot == 0) kickerScale[(int)side] = r.transform.localScale;
        }

        /// <summary>True when the hero has the 5-frame kick sheet (k1 run, k2 plant, k3 wind-up, k4 contact, k5 land).</summary>
        bool HasFrames(Side side) => painted[(int)side][0] != null && HeroSprite(lookIds[(int)side][0], "k1") != null;

        void OnDuelStarted()
        {
            ApplyLooks();
            foreach (var kv in props) foreach (var go in kv.Value) if (go != null) Destroy(go);
            props.Clear();
            duel.Rules.PoisonPlaced += OnPoison;
            duel.Rules.PoisonExpired += OnExpired;
            duel.Rules.AntidoteUsed += OnAntidote;
            duel.Rules.WindowTaken += (s, w, v) => Refresh();
        }

        void OnKick(Side side, int target)
        {
            if (stepping[(int)side] == null) stepping[(int)side] = StartCoroutine(StepForward(Heroes[(int)side][0], side));
            if (duel.Rules.ModifiersFor(side).Chance > 0) StartCoroutine(PigeonCrossing());
        }

        Coroutine[] stepping = new Coroutine[2];

        /// <summary>
        /// The kicker runs to the ball, winds up, strikes and stays with the ball while the rally lasts (the rebound
        /// can be hit again), then walks back. User (2026-11-02): «فرم زدن ضربه … فقط دو تصویره و اصن حرکت نداره
        /// توش، باید حرکت بیشتر شه و حالت انیمیشن بگیره» — until there are more painted frames the motion is
        /// procedural on top of the two poses: a bobbing run-up, a crouch, a lean-and-hop strike, a settle.
        /// </summary>
        IEnumerator StepForward(Transform hero, Side side)
        {
            Vector3 home = homes[(int)side][0];
            float sideOff = side == Side.Left ? -0.35f : 0.35f;
            Vector3 Spot() => (side == duel.Me ? foot.StrikeSpot : new Vector3(ballX, 0f, foot.StrikeSpot.z)) + new Vector3(sideOff, 0f, -0.3f);
            // run-up: a few bouncing steps
            bool frames = HasFrames(side);
            for (float k = 0f; k < 1f; k += Time.deltaTime * 3.2f)
            {
                hero.position = Vector3.Lerp(home, Spot(), k) + Vector3.up * Mathf.Abs(Mathf.Sin(k * Mathf.PI * 3f)) * 0.08f;
                if (frames) SetPose(side, 0, (int)(k * 6f) % 2 == 0 ? "k1" : "k2"); // two running frames, three steps
                yield return null;
            }
            striking[(int)side] = StartCoroutine(Strike(hero, side));
            while (striking[(int)side] != null) yield return null; // not `yield return` the coroutine: a re-kick stops and restarts it
            // stay with the ball while it can still be hit
            while (duel.RallyActive && duel.Kicker == side)
            {
                hero.position = Vector3.MoveTowards(hero.position, Spot(), 5f * Time.deltaTime);
                yield return null;
            }
            yield return new WaitForSeconds(0.4f);
            Vector3 from = hero.position;
            for (float k = 0f; k < 1f; k += Time.deltaTime * 2.2f)
            {
                hero.position = Vector3.Lerp(from, home, k) + Vector3.up * Mathf.Abs(Mathf.Sin(k * Mathf.PI * 4f)) * 0.05f;
                if (frames) { var pr = painted[(int)side][0]; SetPose(side, 0, (int)(k * 8f) % 2 == 0 ? "k1" : "k2"); if (pr != null) pr.flipX = side == Side.Left; } // walking back faces home
                yield return null;
            }
            hero.position = home;
            if (frames) { var pr = painted[(int)side][0]; if (pr != null) pr.flipX = side == Side.Right; SetPose(side, 0, "idle"); }
            stepping[(int)side] = null;
        }

        float ballX;
        void LateUpdate() { if (duel != null && duel.ball != null) ballX = Mathf.Lerp(ballX, duel.ball.transform.position.x, 4f * Time.deltaTime); }

        /// <summary>Crouch, swing (kick pose, lean into the shot, a little hop), settle back to idle.</summary>
        IEnumerator Strike(Transform hero, Side side)
        {
            if (HasFrames(side))
            {
                // painted sequence (user: «پنج تا تصویر رو به شکل کمیک از زدن یه توپ»): plant, wind-up, contact, land
                var pr = painted[(int)side][0];
                SetPose(side, 0, "k2"); yield return new WaitForSeconds(0.08f);
                SetPose(side, 0, "k3"); yield return new WaitForSeconds(0.14f);
                // contact frame; where the sheet's contact frame was rejected (user: the girls' «زاویه پاشون خوب نیست») the single kick pose stands in
                SetPose(side, 0, HeroSprite(lookIds[(int)side][0], "k4") != null ? "k4" : "kick");
                for (float k = 0f; k < 1f && pr != null; k += Time.deltaTime / 0.18f) { pr.transform.localPosition = new Vector3(0f, 0.06f * Mathf.Sin(k * Mathf.PI), 0f); yield return null; }
                if (pr != null) pr.transform.localPosition = Vector3.zero;
                SetPose(side, 0, "k5"); yield return new WaitForSeconds(0.22f);
                SetPose(side, 0, "idle");
                striking[(int)side] = null;
                yield break;
            }
            var r = painted[(int)side][0];
            Transform body = r != null ? r.transform : hero;
            Vector3 baseScale = r != null ? kickerScale[(int)side] : Vector3.one; // never read mid-animation (a quick re-kick would drift)
            float lean = side == Side.Left ? -1f : 1f;
            // anticipation
            for (float k = 0f; k < 1f; k += Time.deltaTime * 9f) { body.localScale = new Vector3(baseScale.x * (1f + 0.06f * k), baseScale.y * (1f - 0.08f * k), baseScale.z); yield return null; }
            SetPose(side, 0, "kick");
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
            SetPose(side, 0, "idle");
            for (float k = 0f; k < 1f; k += Time.deltaTime * 6f)
            {
                body.localScale = Vector3.Lerp(new Vector3(baseScale.x * 1.04f, baseScale.y * 0.95f, baseScale.z), baseScale, k);
                yield return null;
            }
            body.localScale = baseScale; body.localRotation = Quaternion.identity; body.localPosition = Vector3.zero;
            striking[(int)side] = null;
        }

        void OnRekick(Side side)
        {
            var hero = Heroes[(int)side][0];
            if (hero == null) return;
            if (striking[(int)side] != null) StopCoroutine(striking[(int)side]);
            striking[(int)side] = StartCoroutine(Strike(hero, side));
        }

        IEnumerator PigeonCrossing()
        {
            if (pigeon == null) pigeon = PoisonProps.Pigeon();
            pigeon.SetActive(true);
            float z = mapper.FacadeZ - 4f, y = 2.8f;
            for (float t = 0f; t < 1f; t += Time.deltaTime / 1.1f)
            {
                pigeon.transform.position = new Vector3(Mathf.Lerp(-7f, 7f, t), y + Mathf.Sin(t * 20f) * 0.15f, z);
                yield return null;
            }
            pigeon.SetActive(false);
        }

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
                        list.Add(PoisonProps.Bus(this, rects, mapper.FacadeZ));
                    }
                    break;
                case Effect.Close:
                    foreach (int w in p.Windows) list.Add(PoisonProps.OpenWindow(mapper.WindowRects[w], mapper.FacadeZ));
                    break;
                case Effect.Reach:
                    // a little towards the road: right at the ball it sat under the HUD turn line (run 30)
                    list.Add(PoisonProps.Puddle(foot.StrikeSpot + new Vector3(0f, 0f, 2.6f)));
                    break;
                case Effect.Accuracy:
                    list.Add(PoisonProps.Wind(this, mapper.FacadeZ));
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
                yield return PigeonCrossing();
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
            foreach (var go in list) if (go != null) Destroy(go);
            props.Remove(p);
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
