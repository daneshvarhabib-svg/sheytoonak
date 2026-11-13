using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sheytoonak
{
    /// <summary>
    /// Things that move so the street is not dead (user, 2026-10-27): trees swaying in the wind,
    /// passers-by on the far sidewalk, cars on the road. All are sprites from Resources/Art/Props:
    /// tree_*.png, ped_*.png, car_*.png (transparent PNGs, see docs/ART_SPEC.fa.md). Nothing is
    /// shown for a kind that has no sprite yet; no placeholder shapes.
    /// </summary>
    public class StreetLife : MonoBehaviour
    {
        public float farSidewalkZ = 12.2f, roadZ = 7.5f, nearSidewalkZ = 2.0f;
        public float treeHeight = 4.5f, pedHeight = 1.75f, carLength = 4.2f;
        public Vector2 pedEvery = new Vector2(5f, 12f), carEvery = new Vector2(9f, 20f);
        public float pedSpeed = 1.1f, carSpeed = 7f;

        // ⚠️ ped_01's third frame was dropped (user 2026-11-04: «یکی از عکسا تسبیح تو یه دستشه، تو دو تای دیگه اون یکی
        // دستش»): its prayer beads were in the far hand. Stride (a) and passing (b) alternate; a and c were the same stride.
        // ped_02 (user 2026-11-05, the shopper): frames 2 and 3 of her sheet had three hands («عکس دوم و سوم سه تا دست دارن
        // اون رو در نظر نگیر»), so her walk is sheet frames 1, 5, 4, 6.
        // ped_03 (user 2026-11-05, the old man with a cane): sheet frame 3 had no cane («عکس سومی از چپ بی عصاست»), dropped.
        // A "set" is one character: ped_01.png alone, or ped_01_a.png / ped_01_b.png / ped_01_c.png as walk frames
        // (legs crossing, apart, crossing the other way). User 2026-10-28: a single frame sliding along "does not walk".
        readonly List<Sprite> trees = new List<Sprite>();
        readonly List<List<Sprite>> peds = new List<List<Sprite>>(), cars = new List<List<Sprite>>();
        readonly List<Transform> treeTs = new List<Transform>();
        readonly List<Mover> movers = new List<Mover>();
        float nextPed, nextCar, wind;
        [Tooltip("Cars from ChatGPT came out facing left; pedestrians face right. Flip rules per kind.")]
        public bool carsFaceRight = false, pedsFaceRight = true;
        public float walkFps = 7f;

        class Mover
        {
            public Transform t; public SpriteRenderer sr; public List<Sprite> frames; public float speed, endX, frame;
            public Transform[] wheels; public float wheelRadius; // painted cars: wheels spin with the distance driven
            public bool cyclist; // a rider on two wheels: wheels roll, the walk frames pedal, no bob
        }

        void Awake()
        {
            var sets = new SortedDictionary<string, List<Sprite>>();
            foreach (var tex in Resources.LoadAll<Texture2D>("Art/Props"))
            {
                var sp = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0f), 100f);
                sp.name = tex.name; // Sprite.Create leaves the name empty: the frame order (a, b, c…) and the per-set pace read it
                if (tex.name.StartsWith("tree")) { trees.Add(sp); continue; }
                string key = SetKey(tex.name);
                if (!sets.TryGetValue(key, out var list)) sets[key] = list = new List<Sprite>();
                list.Add(sp);
            }
            foreach (var kv in sets)
            {
                kv.Value.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
                if (kv.Key.StartsWith("ped")) peds.Add(kv.Value);
                else if (kv.Key.StartsWith("car")) cars.Add(kv.Value);
            }
            LoadCars();
            // the sprite trees are gone (user 2026-11-05): the painting fills the frame and its own trees sway
            nextPed = Time.time + Random.Range(1f, 3f);
            nextCar = Time.time + Random.Range(2f, 6f);
            GameEvents.WindowBroken += OnWindowBroken;
            GameEvents.RoundStarted += OnRoundStarted;
        }

        void OnDestroy() { GameEvents.WindowBroken -= OnWindowBroken; GameEvents.RoundStarted -= OnRoundStarted; }

        // User (2026-11-08): «کشته شدن عابر پیاده هم دو سه بار نهایتاً در هر دست رخ بده»
        int victimsThisRound, victimsAllowed = 3;
        void OnRoundStarted() { victimsThisRound = 0; victimsAllowed = Random.Range(2, 4); }

        // User (2026-11-08): «وقتی شیشه می‌شکنه عابر پیاده زیر پنجره است یه لحظه شوکه شه و سرش خونی بشه و بعد آمبولانس بیاد
        // ببردش». A walker right under the window freezes, a red stain spreads on the head, he keels over on the pavement;
        // then the ambulance picture (Art/Props/ambulance, the user's: a body under a white sheet being loaded) drives in
        // on the road, stops at him, he is gone, it drives off. Without the picture he just lies there and fades.
        void OnWindowBroken(int window, Vector3 point)
        {
            if (victimsThisRound >= victimsAllowed) return;
            for (int i = movers.Count - 1; i >= 0; i--)
            {
                var m = movers[i];
                if (m.cyclist || m.wheels != null || Mathf.Abs(m.speed) >= 3f) continue; // walkers only
                if (Mathf.Abs(m.t.position.z - farSidewalkZ) > 1f || Mathf.Abs(m.t.position.x - point.x) > 1.1f) continue;
                movers.RemoveAt(i);
                victimsThisRound++;
                StartCoroutine(Victim(m));
                break;
            }
        }

        /// <summary>
        /// ⚠️ Off since 2026-11-11 (docs/STORE_RISK.fa.md §2 #6, applied at the user's word «اونا رو هم اعمال کنیم با اولویت
        /// بالا»): blood lifts the store age rating. The victim still reels, falls and is carried off; the pigeon still
        /// falls and is swept up -- only the stains are not drawn. The code stays so the rating decision can be undone.
        /// </summary>
        public static bool ShowBlood = false;
        static Texture2D bloodTex;
        /// <summary>The soft red stain (also used under the pigeon the ball brings down).</summary>
        public static Sprite BloodSprite()
        {
            if (bloodTex == null)
            {
                const int n = 32; bloodTex = new Texture2D(n, n, TextureFormat.RGBA32, false);
                var px = new Color32[n * n];
                for (int y = 0; y < n; y++) for (int x = 0; x < n; x++)
                { float d = Vector2.Distance(new Vector2(x, y), new Vector2(15.5f, 15.5f)) / 15.5f; float a = Mathf.Clamp01(1.2f - d * d * 1.4f); px[y * n + x] = new Color32(150, 10, 10, (byte)(a * 255)); }
                bloodTex.SetPixels32(px); bloodTex.Apply();
            }
            return Sprite.Create(bloodTex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 100f);
        }
        AudioSource sfx;
        /// <summary>Audio/Fx/victim_NN (the hit walker's cry) and Audio/Fx/siren (the van): random, never the same twice running.</summary>
        void PlayFx(string prefix, float volume, ref int last)
        {
            var all = new List<AudioClip>();
            foreach (var c in Resources.LoadAll<AudioClip>("Audio/Fx")) if (c.name.StartsWith(prefix)) all.Add(c);
            if (all.Count == 0) return;
            if (sfx == null) { sfx = gameObject.AddComponent<AudioSource>(); sfx.playOnAwake = false; sfx.spatialBlend = 0f; }
            int k = all.Count == 1 ? 0 : Random.Range(0, all.Count - 1);
            if (all.Count > 1 && k >= last) k++;
            last = k;
            sfx.pitch = Random.Range(0.97f, 1.03f);
            sfx.PlayOneShot(all[k], volume);
        }
        int lastVictimClip = -1, lastSiren = -1;

        IEnumerator Victim(Mover m)
        {
            var t = m.t; var sr = m.sr;
            if (t == null) yield break;
            t.name = "Victim";
            PlayFx("victim", 0.9f, ref lastVictimClip);
            // shocked: frozen mid-stride, a small jolt upward
            Vector3 p0 = t.position;
            for (float k = 0f; k < 0.35f; k += Time.deltaTime) { if (t == null) yield break; t.position = p0 + Vector3.up * 0.06f * Mathf.Sin(k / 0.35f * Mathf.PI); yield return null; }
            // the stain on the head (top of the sprite), growing
            if (bloodTex == null)
            {
                const int n = 32; bloodTex = new Texture2D(n, n, TextureFormat.RGBA32, false);
                var px = new Color32[n * n];
                for (int y = 0; y < n; y++) for (int x = 0; x < n; x++)
                { float d = Vector2.Distance(new Vector2(x, y), new Vector2(15.5f, 15.5f)) / 15.5f; float a = Mathf.Clamp01(1.2f - d * d * 1.4f); px[y * n + x] = new Color32(150, 10, 10, (byte)(a * 255)); }
                bloodTex.SetPixels32(px); bloodTex.Apply();
            }
            var blood = new GameObject("Blood").AddComponent<SpriteRenderer>();
            blood.sprite = Sprite.Create(bloodTex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 100f);
            blood.transform.SetParent(t, false);
            float hTop = sr.sprite.bounds.size.y; // sprite units: pivot at the feet
            blood.transform.localPosition = new Vector3(0f, hTop * 0.93f, -0.002f);
            // ⚠️ was sortingOrder + 1: sprite order beats depth, so the stain was painted over the ambulance parked in front
            // of him (user 2026-11-09: «خون کسی که ریخته شده میاد روی آمبولانس»). Same order as the body, a hair in front.
            blood.sortingOrder = sr.sortingOrder;
            blood.enabled = ShowBlood;
            for (float k = 0f; k < 0.5f; k += Time.deltaTime)
            {
                if (t == null) yield break;
                blood.transform.localScale = Vector3.one * Mathf.Lerp(0.2f, 1.3f, k / 0.5f) * (hTop * 0.22f / 0.32f);
                yield return null;
            }
            // keels over: falls away from the house onto the pavement (rotates about the feet)
            float dir = m.speed >= 0f ? -1f : 1f;
            for (float k = 0f; k < 0.6f; k += Time.deltaTime)
            {
                if (t == null) yield break;
                float e = k / 0.6f; e = e * e;
                t.localRotation = Quaternion.Euler(0f, 0f, dir * 88f * e);
                yield return null;
            }
            yield return new WaitForSeconds(1.6f);
            if (t == null) yield break;
            // ⚠️ the pictures come out facing LEFT (the user's ambulance has its cab on the left; the medics push to the
            // left), so a vehicle driving right is the mirrored one.
            Texture2D closed = Resources.Load<Texture2D>("Art/Props/ambulance_closed"), open = Resources.Load<Texture2D>("Art/Props/ambulance_open");
            // any number of walk frames per set: medics_empty_a, _b, _c … (the user's sheet has three), medics_loaded_*
            var emptyT = new List<Texture2D>(); var loadedT = new List<Texture2D>();
            foreach (var tx in Resources.LoadAll<Texture2D>("Art/Props"))
                if (tx.name.StartsWith("medics_empty_")) emptyT.Add(tx); else if (tx.name.StartsWith("medics_loaded_")) loadedT.Add(tx);
            emptyT.Sort((p, q) => string.CompareOrdinal(p.name, q.name)); loadedT.Sort((p, q) => string.CompareOrdinal(p.name, q.name));
            var amb = Resources.Load<Texture2D>("Art/Props/ambulance");
            if (closed != null && open != null && emptyT.Count > 0 && loadedT.Count > 0)
            {
                // User (2026-11-08): «نباید تعداد بیشتری تصویر از آمبولانس و پرسنلش داشته باشی؟ اینطوری که نمی‌تونی طبیعی در
                // بیاری حرکتشون رو». Staged: the van drives in and parks beside him, doors open, two medics wheel an empty
                // stretcher out to him, come back with him under the sheet, doors close, it drives off.
                bool drivesRight = t.position.x > 0f; // comes from the far side so it passes in front of him before stopping
                float side = drivesRight ? -1f : 1f;   // the van parks a little short of him on its own approach side
                var sp = Sprite.Create(closed, new Rect(0, 0, closed.width, closed.height), new Vector2(0.5f, 0f), 100f);
                float vEdge = OffScreenX(roadZ + 1.4f, 2.5f * closed.width / closed.height * 0.5f + 2.2f, 24f); // the open doors stick out
                var van = Make("Ambulance", sp, 2.5f, new Vector3(drivesRight ? -vEdge : vEdge, 0f, roadZ + 1.4f));
                PlayFx("siren", 0.6f, ref lastSiren);
                var vr = van.GetComponent<SpriteRenderer>();
                if (drivesRight) van.localScale = new Vector3(-van.localScale.x, van.localScale.y, van.localScale.z);
                // User (2026-11-08): «ماشین آمبولانس چرخش می‌چرخه جدا؟». Same trick as the cars: a loose wheel over each painted
                // one, turning with the distance. Measured on the keyed 1473x821 picture: centres (195, 690) and (1105, 700), r 100.
                var wheelTex = Resources.Load<Texture2D>("Art/Props/ambulance_wheel") ?? Resources.Load<Texture2D>("Art/Cars/wheel_1");
                var wheels = new List<Transform>();
                // ⚠️ 2026-11-12: the cartoon van (1515x845, rows padded below so the picture's bottom is the road) has empty
                // arches; wheels centred (220, 723) and (1144, 723), r 122. The old realistic one painted its own wheels.
                // ⚠️ user's phone (2026-11-12): «چرخشون مچ نیست». Android imports pictures wider than 1024 smaller, so the
                // van is recognised by its shape and every pixel below is scaled by the actual width over the original.
                bool toon = Mathf.Abs(closed.width / (float)closed.height - 1515f / 845f) < 0.02f;
                float ps = closed.width / (toon ? 1515f : 1473f);
                if (wheelTex != null && closed.width / (float)closed.height > 1.6f)
                {
                    var wsp = Sprite.Create(wheelTex, new Rect(0, 0, wheelTex.width, wheelTex.height), new Vector2(0.5f, 0.5f), 100f);
                    foreach (var c0 in toon ? new[] { new Vector2(220f, 723f), new Vector2(1144f, 723f) } : new[] { new Vector2(195f, 690f), new Vector2(1105f, 700f) })
                    {
                        var c = c0 * ps;
                        var w = new GameObject("Wheel").AddComponent<SpriteRenderer>();
                        w.sprite = wsp; w.sortingOrder = vr.sortingOrder + 1;
                        w.transform.SetParent(van, false);
                        w.transform.localPosition = new Vector3((c.x - closed.width * 0.5f) / 100f, (closed.height - c.y) / 100f, -0.01f);
                        w.transform.localScale = Vector3.one * ((toon ? 244f : 200f) * ps / wheelTex.width);
                        if (toon) w.sortingOrder = vr.sortingOrder - 1; // the arches cover the tyres' tops
                        wheels.Add(w.transform);
                    }
                }
                // body and wheels drawn as one picture by distance (the wheels sit under the body's order): a walker behind the
                // van is hidden by its tyres too (user 2026-11-13)
                van.gameObject.AddComponent<UnityEngine.Rendering.SortingGroup>().sortingOrder = vr.sortingOrder;
                float wheelR = (toon ? 1.22f : 1f) * ps * Mathf.Abs(van.localScale.y); // the wheel's radius at 100 ppu, scaled
                float x0 = van.position.x, x1 = t.position.x + side * 2.2f;
                // ⚠️ user (2026-11-13): «تصویر باز و بسته کردن آمبولانس رو باید روی این تصویر بسازی نه روی اونی که عموده تصویر
                // بهش». The scene is the user's sheet of seven three-quarter frames of the whole van (Art/Props/amb34_0..6,
                // aligned on the van, 1489x1000, wheels' bottom at y 960; the stretcher comes to rest at x ~1000, the van's
                // middle is x ~678). The side view drives in, the three-quarter van takes its place, the doors open, the
                // medics fetch him, the doors close, the side view drives off. The old rear-only frames (medics_out/load)
                // were laid on the side view and never matched it.
                var amb34 = new List<Texture2D>();
                for (int i = 0; i < 7; i++) { var tx = Resources.Load<Texture2D>("Art/Props/amb34_" + i); if (tx == null) { amb34.Clear(); break; } amb34.Add(tx); }
                bool hasScene = toon && amb34.Count == 7;
                float faceL = drivesRight ? -1f : 1f; // +1 = cab on the left
                const float sceneH = 2.81f;           // the three-quarter van about as tall as the side view (its rear is nearer)
                float sceneX = t.position.x - faceL * (1000f - 744.5f) / 1000f * sceneH; // stretcher over him
                if (hasScene) x1 = sceneX + faceL * (678f - 744.5f) / 1000f * sceneH; // the side view stops where the van will stand
                for (float k = 0f; k < 1f; k += Time.deltaTime / 2f)
                {
                    if (van == null) yield break;
                    float nx = Mathf.Lerp(x0, x1, 1f - (1f - k) * (1f - k));
                    float deg = (nx - van.position.x) / Mathf.Max(0.01f, wheelR) * Mathf.Rad2Deg * (drivesRight ? 1f : -1f);
                    foreach (var w in wheels) w.Rotate(0f, 0f, -deg, Space.Self);
                    van.position = new Vector3(nx, 0f, van.position.z); yield return null;
                }
                yield return new WaitForSeconds(0.3f);
                if (hasScene)
                {
                    System.Func<Texture2D, Sprite> mk = tx => Sprite.Create(tx, new Rect(0, 0, tx.width, tx.height), new Vector2(0.5f, 0f), 100f);
                    var scene = Make("AmbulanceScene", mk(amb34[0]), sceneH, new Vector3(sceneX, -40f / 1000f * sceneH, van.position.z));
                    var sr2 = scene.GetComponent<SpriteRenderer>();
                    sr2.sortingOrder = vr.sortingOrder;
                    if (drivesRight) scene.localScale = new Vector3(-scene.localScale.x, scene.localScale.y, scene.localScale.z);
                    van.gameObject.SetActive(false);
                    // closed, half open, open, out with the empty stretcher, ready (he is lifted on), loading, closing, closed
                    int[] seq = { 0, 1, 2, 3, 4, 5, 6, 0 };
                    float[] hold = { 0.35f, 0.3f, 0.45f, 0.5f, 0.55f, 0.6f, 0.45f, 0.35f };
                    for (int i = 0; i < seq.Length; i++)
                    {
                        if (scene == null) yield break;
                        sr2.sprite = mk(amb34[seq[i]]);
                        yield return new WaitForSeconds(hold[i]);
                        if (seq[i] == 4 && t != null) Destroy(t.gameObject); // lifted onto the stretcher
                    }
                    if (t != null) Destroy(t.gameObject);
                    Destroy(scene.gameObject);
                    if (van == null) yield break;
                    van.gameObject.SetActive(true);
                    float xe2 = drivesRight ? vEdge : -vEdge;
                    for (float k = 0f; k < 1f; k += Time.deltaTime / 2.4f)
                    {
                        if (van == null) yield break;
                        float nx = Mathf.Lerp(x1, xe2, k * k);
                        float deg = (nx - van.position.x) / Mathf.Max(0.01f, wheelR) * Mathf.Rad2Deg * (drivesRight ? 1f : -1f);
                        foreach (var w in wheels) w.Rotate(0f, 0f, -deg, Space.Self);
                        van.position = new Vector3(nx, 0f, van.position.z); yield return null;
                    }
                    Destroy(van.gameObject);
                    yield break;
                }
                foreach (var w in wheels) w.gameObject.SetActive(false); // the open picture is a three-quarter view: its own wheels
                vr.sprite = Sprite.Create(open, new Rect(0, 0, open.width, open.height), new Vector2(0.5f, 0f), 100f);
                yield return new WaitForSeconds(0.5f);
                // medics with the empty stretcher appear at the rear of the van and walk to him on the pavement
                var fe = new List<Sprite>(); foreach (var tx in emptyT) fe.Add(Sprite.Create(tx, new Rect(0, 0, tx.width, tx.height), new Vector2(0.5f, 0f), 100f));
                var fl = new List<Sprite>(); foreach (var tx in loadedT) fl.Add(Sprite.Create(tx, new Rect(0, 0, tx.width, tx.height), new Vector2(0.5f, 0f), 100f));
                float rearX = x1 + side * 1.6f; // the van stopped short of him on its approach side, so its rear is nearer him
                var med = Make("Medics", fe[0], 1.85f, new Vector3(rearX, 0f, farSidewalkZ - 0.25f));
                var mr = med.GetComponent<SpriteRenderer>();
                float mx0 = rearX, mx1 = t.position.x - side * 0.9f; // they stop beside him
                bool toRight = mx1 > mx0;
                if (toRight) med.localScale = new Vector3(-med.localScale.x, med.localScale.y, med.localScale.z);
                float f = 0f;
                // with one painted frame the walk is a step bob and a slight sway (user asked for frame B; it slots in as medics_*_b)
                for (float k = 0f; k < 1f; k += Time.deltaTime / 1.6f) { if (med == null) yield break; f += Time.deltaTime * 4f; mr.sprite = fe[(int)f % fe.Count]; med.position = new Vector3(Mathf.Lerp(mx0, mx1, k), fe.Count > 1 ? 0f : Mathf.Abs(Mathf.Sin(k * Mathf.PI * 5f)) * 0.04f, med.position.z); med.localRotation = Quaternion.Euler(0f, 0f, fe.Count > 1 ? 0f : Mathf.Sin(k * Mathf.PI * 10f) * 1.5f); yield return null; }
                yield return new WaitForSeconds(0.7f);
                if (t != null) Destroy(t.gameObject); // lifted onto the stretcher
                mr.sprite = fl[0];
                med.localScale = new Vector3(-med.localScale.x, med.localScale.y, med.localScale.z); // turn round
                yield return new WaitForSeconds(0.4f);
                for (float k = 0f; k < 1f; k += Time.deltaTime / 1.8f) { if (med == null) yield break; f += Time.deltaTime * 3.5f; mr.sprite = fl[(int)f % fl.Count]; med.position = new Vector3(Mathf.Lerp(mx1, mx0, k), fl.Count > 1 ? 0f : Mathf.Abs(Mathf.Sin(k * Mathf.PI * 5f)) * 0.03f, med.position.z); med.localRotation = Quaternion.Euler(0f, 0f, fl.Count > 1 ? 0f : Mathf.Sin(k * Mathf.PI * 10f) * 1.2f); yield return null; }
                yield return new WaitForSeconds(0.5f);
                Destroy(med.gameObject);
                yield return new WaitForSeconds(0.4f);
                vr.sprite = sp; // doors shut
                foreach (var w in wheels) w.gameObject.SetActive(true);
                yield return new WaitForSeconds(0.5f);
                float xe = drivesRight ? vEdge : -vEdge;
                for (float k = 0f; k < 1f; k += Time.deltaTime / 2.4f)
                {
                    if (van == null) yield break;
                    float nx = Mathf.Lerp(x1, xe, k * k);
                    float deg = (nx - van.position.x) / Mathf.Max(0.01f, wheelR) * Mathf.Rad2Deg * (drivesRight ? 1f : -1f);
                    foreach (var w in wheels) w.Rotate(0f, 0f, -deg, Space.Self);
                    van.position = new Vector3(nx, 0f, van.position.z); yield return null;
                }
                Destroy(van.gameObject);
            }
            else if (amb != null)
            {
                // one composite picture (the user's first): the van with the stretcher already being loaded drives in,
                // stops at him, he is gone behind it, it drives off. Painted facing left.
                bool drivesRight = t.position.x > 0f;
                var sp = Sprite.Create(amb, new Rect(0, 0, amb.width, amb.height), new Vector2(0.5f, 0f), 100f);
                var a = Make("Ambulance", sp, 2.6f, new Vector3(drivesRight ? -24f : 24f, 0f, roadZ + 1.2f));
                PlayFx("siren", 0.6f, ref lastSiren);
                if (drivesRight) a.localScale = new Vector3(-a.localScale.x, a.localScale.y, a.localScale.z);
                float x0 = a.position.x, x1 = t.position.x;
                for (float k = 0f; k < 1f; k += Time.deltaTime / 1.8f) { if (a == null) yield break; a.position = new Vector3(Mathf.Lerp(x0, x1, 1f - (1f - k) * (1f - k)), 0f, a.position.z); yield return null; }
                yield return new WaitForSeconds(0.6f);
                if (t != null) { for (float k = 0f; k < 0.5f; k += Time.deltaTime) { sr.color = new Color(1f, 1f, 1f, 1f - k / 0.5f); yield return null; } Destroy(t.gameObject); }
                yield return new WaitForSeconds(0.8f);
                float xe = drivesRight ? 26f : -26f;
                for (float k = 0f; k < 1f; k += Time.deltaTime / 2.2f) { if (a == null) yield break; a.position = new Vector3(Mathf.Lerp(x1, xe, k * k), 0f, a.position.z); yield return null; }
                Destroy(a.gameObject);
            }
            else
            {
                yield return new WaitForSeconds(2f);
                for (float k = 0f; k < 1f; k += Time.deltaTime) { if (t == null) yield break; sr.color = new Color(1f, 1f, 1f, 1f - k); blood.color = sr.color; yield return null; }
                if (t != null) Destroy(t.gameObject);
            }
        }

        // User (2026-11-05): «فولکس و واگن هم چرخ ندارن فقط قالپاق دارن، باید چرخ داشته باشن و چرخش بچرخه». Cars are now
        // a body without wheels (Art/Cars/car_N_name) and a separate wheel (wheel_N) that turns as the car drives;
        // Art/Cars/cars.txt holds, in body pixels, the sill line, the wheel radius and the arch centres.
        class CarDef { public Sprite body, wheel; public float w, h, wheelY, radius; public float[] xs; }
        readonly List<CarDef> paintedCars = new List<CarDef>();
        void LoadCars()
        {
            var txt = Resources.Load<TextAsset>("Art/Cars/cars");
            if (txt == null) return;
            foreach (var line in txt.text.Split('\n'))
            {
                var f = line.Trim().Split(' ');
                if (f.Length < 8 || f[0].StartsWith("#")) continue;
                var inv = System.Globalization.CultureInfo.InvariantCulture;
                var bt = Resources.Load<Texture2D>("Art/Cars/car_" + f[1] + "_" + f[0]);
                var wt = Resources.Load<Texture2D>("Art/Cars/wheel_" + f[1]);
                if (bt == null || wt == null) continue;
                float W = float.Parse(f[2], inv), H = float.Parse(f[3], inv), sill = float.Parse(f[4], inv), r = float.Parse(f[5], inv);
                float cy = sill - r * 0.15f, ground = cy + r; // wheel centre a little up into the arch, its bottom on the road
                var d = new CarDef { w = W, h = H, radius = r, wheelY = r };
                // the body's pivot is the road under it (below its own picture): pivot.y < 0
                d.body = Sprite.Create(bt, new Rect(0, 0, bt.width, bt.height), new Vector2(0.5f, -(ground - H) / H), 100f);
                d.wheel = Sprite.Create(wt, new Rect(0, 0, wt.width, wt.height), new Vector2(0.5f, 0.5f), 100f);
                d.xs = new float[f.Length - 6];
                for (int i = 6; i < f.Length; i++) d.xs[i - 6] = float.Parse(f[i], inv);
                paintedCars.Add(d);
            }
        }

        void SpawnCar(CarDef d, float speed)
        {
            bool toRight = Random.value < 0.5f;
            float scale = carLength / (d.w / 100f);
            var root = new GameObject("Car").transform;
            root.SetParent(transform, false);
            float edge = OffScreenX(roadZ, carLength * 0.5f, 30f);
            root.position = new Vector3(toRight ? -edge : edge, 0f, roadZ);
            root.localScale = new Vector3(toRight ? scale : -scale, scale, scale); // painted facing right
            var body = new GameObject("Body").AddComponent<SpriteRenderer>();
            body.sprite = d.body; body.transform.SetParent(root, false);
            var ws = new Transform[d.xs.Length];
            float wsz = 2f * d.radius / d.wheel.rect.width; // wheel picture -> wheel size in body pixels
            for (int i = 0; i < d.xs.Length; i++)
            {
                var wr = new GameObject("Wheel").AddComponent<SpriteRenderer>();
                wr.sprite = d.wheel;
                wr.transform.SetParent(root, false);
                // a hair behind the body so the arch edge overlaps the tyre (distance sorting, not sortingOrder)
                wr.transform.localPosition = new Vector3((d.xs[i] - d.w * 0.5f) / 100f, d.wheelY / 100f, 0.02f / scale);
                wr.sortingOrder = -1; // inside the car's sorting group: under the body, the arch edge overlaps the tyre
                wr.transform.localScale = Vector3.one * wsz;
                ws[i] = wr.transform;
            }
            root.gameObject.AddComponent<UnityEngine.Rendering.SortingGroup>(); // one picture by distance (user 2026-11-13)
            movers.Add(new Mover { t = root, sr = body, frames = new List<Sprite> { d.body }, speed = toRight ? speed : -speed, endX = edge,
                                   wheels = ws, wheelRadius = d.radius / 100f * scale });
        }

        void OnEnable() => GameEvents.WindChanged += OnWind;
        void OnDisable() => GameEvents.WindChanged -= OnWind;
        void OnWind(float w) => wind = w;

        /// <summary>"ped_01_b" -> "ped_01"; "car_02_beetle" stays "car_02_beetle" (a frame suffix is a single letter).</summary>
        static string SetKey(string name)
        {
            int i = name.LastIndexOf('_');
            return i > 0 && name.Length - i == 2 && char.IsLetter(name[i + 1]) ? name.Substring(0, i) : name;
        }

        void PlantTrees()
        {
            if (trees.Count == 0) return;
            float[] xs = { -11f, -7.5f, 7.5f, 11f };
            for (int i = 0; i < xs.Length; i++)
            {
                var sp = trees[i % trees.Count];
                var t = Make("Tree", sp, treeHeight, new Vector3(xs[i], 0f, nearSidewalkZ + 0.6f));
                treeTs.Add(t);
            }
        }

        Transform Make(string name, Sprite sp, float worldHeight, Vector3 pos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.transform.position = pos;
            var r = go.AddComponent<SpriteRenderer>();
            r.sprite = sp;
            float h = sp.bounds.size.y;
            go.transform.localScale = Vector3.one * (worldHeight / Mathf.Max(0.01f, h));
            return go.transform;
        }

        void Update()
        {
            float t = Time.time;
            for (int i = 0; i < treeTs.Count; i++)
            {
                float sway = Mathf.Sin(t * 1.1f + i * 1.7f) * (1.2f + Mathf.Abs(wind) * 4f) + Mathf.Sin(t * 3.7f + i) * 0.4f;
                treeTs[i].localRotation = Quaternion.Euler(0f, 0f, sway - wind * 3f);
            }
            // One pedestrian and one car at a time: with several the street looked like a parade of the same man.
            // User (2026-11-05): «چندتا رهگذر دیگه و دوچرخه‌سواری که چرخش تکون بخوره و پاش رو هم اضافه کنیم». Two walkers may be
            // out at once when they are far apart, and every third "walker" is a cyclist.
            int pedsOut = 0; bool carOut = false; float nearestPedX = 99f;
            foreach (var m in movers)
            {
                if (m.wheels != null && !m.cyclist) { carOut = true; continue; }
                pedsOut++; nearestPedX = Mathf.Min(nearestPedX, Mathf.Abs(m.t.position.x));
            }
            if (peds.Count > 0 && t >= nextPed && (pedsOut == 0 || (pedsOut == 1 && nearestPedX > 5f)))
            {
                nextPed = t + Random.Range(pedEvery.x, pedEvery.y) * (pedsOut == 0 ? 1f : 0.6f);
                ++walkers;
                // user (2026-11-12): «مرد ویلچری و دوچرخه‌سوار نباید باشن تو تصویر فعلا» -- switched off, code kept
                if (RidersOn && walkers % 5 == 0 && SpawnWheelchair()) { }
                else if (RidersOn && paintedCars.Count > 0 && walkers % 3 == 0) SpawnCyclist(peds[Random.Range(0, peds.Count)]);
                else
                {
                    // User (2026-11-05): «نباید دو رهگذر یک شکل هم‌زمان در تصویر باشن»: never the character already out
                    var set = peds[Random.Range(0, peds.Count)];
                    for (int tries = 0; tries < 8 && SetIsOut(set); tries++) set = peds[Random.Range(0, peds.Count)];
                    if (SetIsOut(set)) return;
                    Spawn(set, pedHeight * PedHeightScale(set), farSidewalkZ + Random.Range(-0.3f, 0.3f), pedSpeed * PedSpeedScale(set) * Random.Range(0.8f, 1.2f), 16f, pedsFaceRight);
                }
            }
            if (paintedCars.Count > 0 && t >= nextCar && !carOut)
            {
                nextCar = t + Random.Range(carEvery.x, carEvery.y);
                SpawnCar(paintedCars[Random.Range(0, paintedCars.Count)], carSpeed * Random.Range(0.85f, 1.15f));
            }
            else if (paintedCars.Count == 0 && cars.Count > 0 && t >= nextCar && !carOut)
            {
                nextCar = t + Random.Range(carEvery.x, carEvery.y);
                var set = cars[Random.Range(0, cars.Count)];
                var sp = set[0];
                float h = carLength * sp.bounds.size.y / Mathf.Max(0.01f, sp.bounds.size.x);
                Spawn(set, h, roadZ, carSpeed * Random.Range(0.85f, 1.15f), 30f, carsFaceRight);
            }
            for (int i = movers.Count - 1; i >= 0; i--)
            {
                var m = movers[i];
                m.t.position += Vector3.right * m.speed * Time.deltaTime;
                if (m.wheels != null)
                {
                    // rolling without slipping: angle = distance / radius; the mirrored root flips the sense by itself
                    float deg = Mathf.Abs(m.speed) * Time.deltaTime / Mathf.Max(0.01f, m.wheelRadius) * Mathf.Rad2Deg;
                    foreach (var w in m.wheels) w.Rotate(0f, 0f, -deg, Space.Self);
                }
                if (m.cyclist)
                {
                    if (m.frames.Count > 1)
                    {
                        m.frame += Time.deltaTime * 4f; // pedalling: the two painted poses, four a second
                        m.sr.sprite = m.frames[(int)m.frame % m.frames.Count];
                    }
                }
                else if (Mathf.Abs(m.speed) < 3f)
                {
                    // Walk cycle when frames exist; otherwise a small bob and lean so it at least does not glide.
                    if (m.frames.Count > 1)
                    {
                        m.frame += Time.deltaTime * walkFps;
                        m.sr.sprite = m.frames[(int)m.frame % m.frames.Count];
                    }
                    // user (2026-11-04): «حرکت عابر پیاده هنوز مصنوعیه». The rocking tilt looked like a toy: gone. The step bob
                    // now follows the walking speed (two steps per stride, ~0.7 m a step) and is a little smaller; a real walk
                    // still needs the three painted frames ped_NN_a/b/c (asked for).
                    float steps = Mathf.Abs(m.speed) / 0.7f;
                    float bob = Mathf.Abs(Mathf.Sin(t * Mathf.PI * steps)) * 0.025f;
                    m.t.position = new Vector3(m.t.position.x, bob, m.t.position.z);
                    m.t.localRotation = Quaternion.identity;
                }
                if ((m.speed > 0f && m.t.position.x > m.endX) || (m.speed < 0f && m.t.position.x < -m.endX))
                {
                    Destroy(m.t.gameObject);
                    movers.RemoveAt(i);
                }
            }
        }

        int walkers;
        const bool RidersOn = false;

        /// <summary>
        /// The x (absolute) just outside the picture at depth <paramref name="z"/> for something half
        /// <paramref name="halfWidth"/> wide. ⚠️ user (2026-11-12, phone): «ادم ها و ماشین ها باید از گوشه گوشه کادر وارد و
        /// خارج بشن». The fixed ±16 / ±30 were tuned on a 16:9 editor; the phone is ~20:9, so walkers popped in inside
        /// the picture. Measured from the camera every time instead.
        /// </summary>
        public static float OffScreenX(float z, float halfWidth, float fallback)
        {
            var cam = Camera.main;
            if (cam == null) return fallback;
            float d = z - cam.transform.position.z;
            if (d < 0.5f && !cam.orthographic) return fallback;
            float l = cam.ViewportToWorldPoint(new Vector3(0f, 0.3f, d)).x, r = cam.ViewportToWorldPoint(new Vector3(1f, 0.3f, d)).x;
            return Mathf.Max(Mathf.Abs(l), Mathf.Abs(r)) + halfWidth + 0.3f;
        }
        /// <summary>
        /// The man in the wheelchair (user 2026-11-05): two painted poses of man-and-chair (hand forward, hand back) and
        /// a separate wheel laid over the painted one, rolling with the distance; the poses alternate like the cyclist's.
        /// In the rider picture the big wheel is centred at (150, 429) of 456x609 with radius 150; the loose wheel
        /// picture is 389 px across.
        /// </summary>
        bool SpawnWheelchair()
        {
            var r0 = Resources.Load<Texture2D>("Art/Props/wheelchair_rider_a"); var r1 = Resources.Load<Texture2D>("Art/Props/wheelchair_rider_b");
            var wheelT = Resources.Load<Texture2D>("Art/Props/wheelchair_wheel");
            if (r0 == null || r1 == null || wheelT == null) return false;
            const float ppu = 100f, wheelCx = 150f, wheelCy = 429f, wheelR = 150f;
            float k = 1.35f / (r0.height / ppu); // a seated man is about 1.35 m tall
            bool toRight = Random.value < 0.5f;
            var root = new GameObject("Wheelchair").transform;
            root.SetParent(transform, false);
            root.position = new Vector3(toRight ? -16f : 16f, 0f, farSidewalkZ + Random.Range(-0.3f, 0.3f));
            root.localScale = new Vector3(toRight ? 1f : -1f, 1f, 1f); // painted facing right
            var body = new GameObject("Rider").AddComponent<SpriteRenderer>();
            var frames = new List<Sprite>
            {
                Sprite.Create(r0, new Rect(0, 0, r0.width, r0.height), new Vector2(0f, 1f), ppu),
                Sprite.Create(r1, new Rect(0, 0, r1.width, r1.height), new Vector2(0f, 1f), ppu),
            };
            body.sprite = frames[0]; body.transform.SetParent(root, false);
            body.transform.localPosition = new Vector3(-r0.width * 0.5f / ppu * k, r0.height / ppu * k, 0f);
            body.transform.localScale = Vector3.one * k;
            var wheel = new GameObject("Wheel").AddComponent<SpriteRenderer>();
            wheel.sprite = Sprite.Create(wheelT, new Rect(0, 0, wheelT.width, wheelT.height), new Vector2(0.5f, 0.5f), ppu);
            wheel.transform.SetParent(root, false);
            wheel.transform.localPosition = new Vector3((wheelCx - r0.width * 0.5f) / ppu * k, (r0.height - wheelCy) / ppu * k, -0.02f);
            wheel.transform.localScale = Vector3.one * (2f * wheelR / wheelT.width) * k;
            float speed = pedSpeed * 0.8f * Random.Range(0.9f, 1.1f);
            movers.Add(new Mover { t = root, sr = body, frames = frames, speed = toRight ? speed : -speed, endX = 16f, wheels = new[] { wheel.transform }, wheelRadius = wheelR / ppu * k, cyclist = true });
            return true;
        }

        /// <summary>
        /// The cyclist (user 2026-11-05, second sheet): man and bicycle painted together in two pedalling poses, the wheels
        /// drawn as plain black discs; a separate spoked wheel is laid over each disc and rolls with the distance (the same
        /// trick as the wheelchair). Measured on the 789x717 frame: rear disc centre (138, 652) r 123, front (582, 648) r 121,
        /// tyre bottom at y 774.
        /// </summary>
        void SpawnCyclist(List<Sprite> unused)
        {
            var r0 = Resources.Load<Texture2D>("Art/Props/cyclist_a"); var r1 = Resources.Load<Texture2D>("Art/Props/cyclist_b");
            var wheelT = Resources.Load<Texture2D>("Art/Props/cyclist_wheel");
            if (r0 == null || r1 == null || wheelT == null) return;
            const float ppu = 100f, groundPx = 774f;
            Vector2[] discs = { new Vector2(138f, 652f), new Vector2(582f, 648f) }; float[] radii = { 123f, 121f };
            float k = 1.8f / (r0.width / ppu); // a city bike is about 1.8 m long
            bool toRight = Random.value < 0.5f;
            float z = roadZ + 1.6f; // the near edge of the far lane
            var root = new GameObject("Cyclist").transform;
            root.SetParent(transform, false);
            root.position = new Vector3(toRight ? -18f : 18f, 0f, z);
            root.localScale = new Vector3(toRight ? 1f : -1f, 1f, 1f); // painted facing right
            var body = new GameObject("Rider").AddComponent<SpriteRenderer>();
            var frames = new List<Sprite>
            {
                Sprite.Create(r0, new Rect(0, 0, r0.width, r0.height), new Vector2(0f, 1f), ppu),
                Sprite.Create(r1, new Rect(0, 0, r1.width, r1.height), new Vector2(0f, 1f), ppu),
            };
            body.sprite = frames[0]; body.transform.SetParent(root, false);
            body.transform.localPosition = new Vector3(-r0.width * 0.5f / ppu * k, groundPx / ppu * k, 0f);
            body.transform.localScale = Vector3.one * k;
            var ws = new Transform[2];
            for (int i = 0; i < 2; i++)
            {
                var sr = new GameObject("Wheel").AddComponent<SpriteRenderer>();
                sr.sprite = Sprite.Create(wheelT, new Rect(0, 0, wheelT.width, wheelT.height), new Vector2(0.5f, 0.5f), ppu);
                sr.transform.SetParent(root, false);
                sr.transform.localPosition = new Vector3((discs[i].x - r0.width * 0.5f) / ppu * k, (groundPx - discs[i].y) / ppu * k, -0.02f);
                sr.transform.localScale = Vector3.one * (2f * radii[i] / wheelT.width) * k;
                ws[i] = sr.transform;
            }
            float speed = pedSpeed * 3.2f * Random.Range(0.9f, 1.1f);
            movers.Add(new Mover { t = root, sr = body, frames = frames, speed = toRight ? speed : -speed, endX = 18f, wheels = ws, wheelRadius = radii[0] / ppu * k, cyclist = true });
        }

        /// <summary>Who walks how: the old man with the cane (ped_03_oldman, user 2026-11-05) shuffles at ~0.6, a hurried
        /// man rushes at ~1.8; everybody else keeps the base pace.</summary>
        static float PedSpeedScale(List<Sprite> set)
        {
            string n = set.Count > 0 ? set[0].name : "";
            if (n.Contains("oldman")) return 0.6f;
            if (n.Contains("hurried")) return 1.8f;
            if (n.Contains("crutches")) return 0.5f; // ped_05 (user 2026-11-05): a cast and two crutches
            if (n.Contains("boy")) return 1.15f;    // ped_06: the boy with the ball
            return 1f;
        }
        static float PedHeightScale(List<Sprite> set)
        {
            string n = set.Count > 0 ? set[0].name : "";
            if (n.Contains("oldman")) return 0.93f; // stooped
            if (n.Contains("boy")) return 0.78f;    // a child
            return 1f;
        }

        bool SetIsOut(List<Sprite> set)
        {
            foreach (var m in movers) if (m.frames != null && m.frames.Count > 0 && set.Count > 0 && m.frames[0] == set[0]) return true;
            return false;
        }

        void Spawn(List<Sprite> frames, float height, float z, float speed, float halfRange, bool facesRight)
        {
            bool toRight = Random.value < 0.5f;
            halfRange = OffScreenX(z, height * frames[0].bounds.size.x / Mathf.Max(0.01f, frames[0].bounds.size.y) * 0.5f, halfRange);
            var tr = Make("Passer-by", frames[0], height, new Vector3(toRight ? -halfRange : halfRange, 0f, z));
            if (toRight != facesRight) tr.localScale = new Vector3(-tr.localScale.x, tr.localScale.y, tr.localScale.z);
            movers.Add(new Mover { t = tr, sr = tr.GetComponent<SpriteRenderer>(), frames = frames, speed = toRight ? speed : -speed, endX = halfRange });
        }
    }
}
