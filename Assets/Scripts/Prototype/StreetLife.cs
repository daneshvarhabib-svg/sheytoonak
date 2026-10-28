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
            PlantTrees();
            nextPed = Time.time + Random.Range(1f, 3f);
            nextCar = Time.time + Random.Range(2f, 6f);
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
            bool pedOut = false, carOut = false;
            foreach (var m in movers) { if (Mathf.Abs(m.speed) < 3f) pedOut = true; else carOut = true; }
            if (peds.Count > 0 && t >= nextPed && !pedOut)
            {
                nextPed = t + Random.Range(pedEvery.x, pedEvery.y);
                Spawn(peds[Random.Range(0, peds.Count)], pedHeight, farSidewalkZ + Random.Range(-0.3f, 0.3f), pedSpeed * Random.Range(0.8f, 1.2f), 16f, pedsFaceRight);
            }
            if (cars.Count > 0 && t >= nextCar && !carOut)
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
                if (Mathf.Abs(m.speed) < 3f)
                {
                    // Walk cycle when frames exist; otherwise a small bob and lean so it at least does not glide.
                    if (m.frames.Count > 1)
                    {
                        m.frame += Time.deltaTime * walkFps;
                        m.sr.sprite = m.frames[(int)m.frame % m.frames.Count];
                    }
                    float bob = Mathf.Abs(Mathf.Sin(t * 7f)) * 0.04f;
                    m.t.position = new Vector3(m.t.position.x, bob, m.t.position.z);
                    m.t.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 7f) * 1.5f);
                }
                if ((m.speed > 0f && m.t.position.x > m.endX) || (m.speed < 0f && m.t.position.x < -m.endX))
                {
                    Destroy(m.t.gameObject);
                    movers.RemoveAt(i);
                }
            }
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
