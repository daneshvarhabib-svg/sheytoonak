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
            root.position = new Vector3(toRight ? -30f : 30f, 0f, roadZ);
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
                wr.transform.localScale = Vector3.one * wsz;
                ws[i] = wr.transform;
            }
            movers.Add(new Mover { t = root, sr = body, frames = new List<Sprite> { d.body }, speed = toRight ? speed : -speed, endX = 30f,
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
                if (paintedCars.Count > 0 && ++walkers % 3 == 0) SpawnCyclist(peds[Random.Range(0, peds.Count)]);
                else Spawn(peds[Random.Range(0, peds.Count)], pedHeight, farSidewalkZ + Random.Range(-0.3f, 0.3f), pedSpeed * Random.Range(0.8f, 1.2f), 16f, pedsFaceRight);
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
                        m.frame += Time.deltaTime * walkFps * 1.4f; // pedalling: the walk frames, a little faster
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
        /// <summary>
        /// A cyclist from what is painted already: a passer-by sitting a little forward on two car wheels (the Beetle rim at a
        /// fifth of the size), the wheels rolling with the distance, the legs the walk frames at pedal speed. Not a painted
        /// bicycle; a prompt for one is noted in docs/ART_SPEC.fa.md.
        /// </summary>
        void SpawnCyclist(List<Sprite> rider)
        {
            var d = paintedCars[Random.Range(0, paintedCars.Count)];
            bool toRight = Random.value < 0.5f;
            float z = roadZ + 1.6f; // the near edge of the far lane
            var root = new GameObject("Cyclist").transform;
            root.SetParent(transform, false);
            root.position = new Vector3(toRight ? -18f : 18f, 0f, z);
            float wr = 0.34f;
            var ws = new Transform[2];
            for (int i = 0; i < 2; i++)
            {
                var sr = new GameObject("Wheel").AddComponent<SpriteRenderer>();
                sr.sprite = d.wheel; sr.transform.SetParent(root, false);
                sr.transform.localPosition = new Vector3(i == 0 ? -0.52f : 0.52f, wr, 0.01f);
                sr.transform.localScale = Vector3.one * (2f * wr / d.wheel.bounds.size.x);
                ws[i] = sr.transform;
            }
            // frame: two dark bars (seat tube and top tube) so the wheels read as one bicycle
            var bar = M(new Color(0.12f, 0.1f, 0.09f));
            Bar(root, new Vector3(-0.1f, wr + 0.42f, 0.02f), new Vector3(0.95f, 0.05f, 0.02f), 0f, bar);
            Bar(root, new Vector3(0.1f, wr + 0.28f, 0.02f), new Vector3(0.75f, 0.05f, 0.02f), 55f, bar);
            Bar(root, new Vector3(-0.3f, wr + 0.28f, 0.02f), new Vector3(0.75f, 0.05f, 0.02f), -65f, bar);
            var body = new GameObject("Rider").AddComponent<SpriteRenderer>();
            body.sprite = rider[0]; body.transform.SetParent(root, false);
            float k = pedHeight * 0.92f / Mathf.Max(0.01f, rider[0].bounds.size.y);
            body.transform.localScale = Vector3.one * k;
            body.transform.localPosition = new Vector3(-0.15f, wr + 0.2f, -0.01f); // sitting: feet at the pedals, not the road
            body.transform.localRotation = Quaternion.Euler(0f, 0f, toRight == pedsFaceRight ? -8f : 8f); // leaning forward
            if (toRight != pedsFaceRight) root.localScale = new Vector3(-1f, 1f, 1f);
            float speed = pedSpeed * 3.2f * Random.Range(0.9f, 1.1f);
            movers.Add(new Mover { t = root, sr = body, frames = rider, speed = toRight ? speed : -speed, endX = 18f, wheels = ws, wheelRadius = wr, cyclist = true });
        }

        static Material M(Color c) { var m = new Material(Shader.Find("Standard")) { color = c }; m.SetFloat("_Glossiness", 0.1f); return m; }
        static void Bar(Transform parent, Vector3 pos, Vector3 size, float zDeg, Material m)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var c = go.GetComponent<Collider>(); if (c != null) { c.enabled = false; Destroy(c); }
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos; go.transform.localScale = size; go.transform.localRotation = Quaternion.Euler(0f, 0f, zDeg);
            go.GetComponent<Renderer>().sharedMaterial = m;
        }

        void Spawn(List<Sprite> frames, float height, float z, float speed, float halfRange, bool facesRight)
        {
            bool toRight = Random.value < 0.5f;
            var tr = Make("Passer-by", frames[0], height, new Vector3(toRight ? -halfRange : halfRange, 0f, z));
            if (toRight != facesRight) tr.localScale = new Vector3(-tr.localScale.x, tr.localScale.y, tr.localScale.z);
            movers.Add(new Mover { t = tr, sr = tr.GetComponent<SpriteRenderer>(), frames = frames, speed = toRight ? speed : -speed, endX = halfRange });
        }
    }
}
