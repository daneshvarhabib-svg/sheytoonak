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

        readonly List<Sprite> trees = new List<Sprite>(), peds = new List<Sprite>(), cars = new List<Sprite>();
        readonly List<Transform> treeTs = new List<Transform>();
        readonly List<(Transform t, float speed, float endX)> movers = new List<(Transform, float, float)>();
        float nextPed, nextCar, wind;

        void Awake()
        {
            foreach (var tex in Resources.LoadAll<Texture2D>("Art/Props"))
            {
                var sp = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0f), 100f);
                if (tex.name.StartsWith("tree")) trees.Add(sp);
                else if (tex.name.StartsWith("ped")) peds.Add(sp);
                else if (tex.name.StartsWith("car")) cars.Add(sp);
            }
            PlantTrees();
            nextPed = Time.time + Random.Range(1f, 3f);
            nextCar = Time.time + Random.Range(2f, 6f);
        }

        void OnEnable() => GameEvents.WindChanged += OnWind;
        void OnDisable() => GameEvents.WindChanged -= OnWind;
        void OnWind(float w) => wind = w;

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
            if (peds.Count > 0 && t >= nextPed)
            {
                nextPed = t + Random.Range(pedEvery.x, pedEvery.y);
                Spawn(peds[Random.Range(0, peds.Count)], pedHeight, farSidewalkZ + Random.Range(-0.3f, 0.3f), pedSpeed * Random.Range(0.8f, 1.2f), 16f);
            }
            if (cars.Count > 0 && t >= nextCar)
            {
                nextCar = t + Random.Range(carEvery.x, carEvery.y);
                var sp = cars[Random.Range(0, cars.Count)];
                float h = carLength * sp.bounds.size.y / Mathf.Max(0.01f, sp.bounds.size.x);
                Spawn(sp, h, roadZ, carSpeed * Random.Range(0.85f, 1.15f), 30f);
            }
            for (int i = movers.Count - 1; i >= 0; i--)
            {
                var m = movers[i];
                m.t.position += Vector3.right * m.speed * Time.deltaTime;
                // Walking bob for the slow ones.
                if (Mathf.Abs(m.speed) < 3f) m.t.position = new Vector3(m.t.position.x, Mathf.Abs(Mathf.Sin(t * 6f)) * 0.03f, m.t.position.z);
                if ((m.speed > 0f && m.t.position.x > m.endX) || (m.speed < 0f && m.t.position.x < -m.endX))
                {
                    Destroy(m.t.gameObject);
                    movers.RemoveAt(i);
                }
            }
        }

        void Spawn(Sprite sp, float height, float z, float speed, float halfRange)
        {
            bool toRight = Random.value < 0.5f;
            var tr = Make("Passer-by", sp, height, new Vector3(toRight ? -halfRange : halfRange, 0f, z));
            if (!toRight) tr.localScale = new Vector3(-tr.localScale.x, tr.localScale.y, tr.localScale.z); // sprites face right
            movers.Add((tr, toRight ? speed : -speed, halfRange));
        }
    }
}
