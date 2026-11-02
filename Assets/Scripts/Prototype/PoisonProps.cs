using System.Collections;
using System.Collections.Generic;
using Sheytoonak.Core.Duel;
using UnityEngine;

namespace Sheytoonak.Prototype
{
    /// <summary>
    /// Recognisable stand-ins for the five starter poisons and their antidotes, built from primitives until
    /// the painted sprites arrive (docs/OBSTACLES.fa.md). Each poison prop lives as long as the poison does.
    /// </summary>
    public static class PoisonProps
    {
        static readonly Dictionary<string, Material> mats = new Dictionary<string, Material>();
        static Material M(string key, Color c, float smooth = 0.2f)
        {
            if (mats.TryGetValue(key, out var m) && m != null) return m;
            m = new Material(Shader.Find("Standard")) { color = c };
            m.SetFloat("_Glossiness", smooth);
            if (c.a < 1f)
            {
                m.SetFloat("_Mode", 3f);
                m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                m.SetInt("_ZWrite", 0);
                m.EnableKeyword("_ALPHABLEND_ON");
                m.renderQueue = 3000;
            }
            mats[key] = m;
            return m;
        }

        static GameObject Part(PrimitiveType t, Transform parent, Vector3 local, Vector3 scale, Material mat, Vector3 euler = default)
        {
            var go = GameObject.CreatePrimitive(t);
            Object.Destroy(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = local;
            go.transform.localScale = scale;
            go.transform.localRotation = Quaternion.Euler(euler);
            go.GetComponent<Renderer>().sharedMaterial = mat;
            return go;
        }

        // ------------------------------------------------------------------ bus (Cover)

        /// <summary>A tall city bus that drives in and parks in front of the covered windows.</summary>
        public static GameObject Bus(MonoBehaviour host, Rect[] covered, float facadeZ)
        {
            float xMin = float.MaxValue, xMax = float.MinValue, top = 0f;
            foreach (var r in covered) { xMin = Mathf.Min(xMin, r.xMin); xMax = Mathf.Max(xMax, r.xMax); top = Mathf.Max(top, r.yMax); }
            float len = Mathf.Max(6f, xMax - xMin + 1.4f), h = Mathf.Max(3.2f, top + 0.25f), w = 2.5f;
            var root = new GameObject("Poison bus").transform;
            var body = M("bus", new Color(0.95f, 0.78f, 0.15f));
            var stripe = M("busstripe", new Color(0.75f, 0.12f, 0.1f));
            var glass = M("busglass", new Color(0.15f, 0.22f, 0.3f), 0.8f);
            var tyre = M("tyre", new Color(0.08f, 0.08f, 0.08f));
            Part(PrimitiveType.Cube, root, new Vector3(0f, h * 0.5f + 0.25f, 0f), new Vector3(len, h, w), body);
            Part(PrimitiveType.Cube, root, new Vector3(0f, 0.9f, -w * 0.5f - 0.01f), new Vector3(len, 0.25f, 0.02f), stripe);
            // two decks of windows facing the camera
            for (int deck = 0; deck < (h > 4f ? 2 : 1); deck++)
                for (float x = -len * 0.5f + 0.7f; x < len * 0.5f - 0.6f; x += 1.2f)
                    Part(PrimitiveType.Cube, root, new Vector3(x + 0.45f, 1.6f + deck * 1.9f, -w * 0.5f - 0.02f), new Vector3(0.95f, 1.0f, 0.02f), glass);
            foreach (float x in new[] { -len * 0.32f, len * 0.32f })
                Part(PrimitiveType.Cylinder, root, new Vector3(x, 0.45f, -w * 0.5f), new Vector3(0.9f, 0.12f, 0.9f), tyre, new Vector3(90f, 0f, 0f));
            Vector3 park = new Vector3((xMin + xMax) * 0.5f, 0f, facadeZ - w * 0.5f - 0.6f);
            root.position = park + Vector3.left * 30f;
            host.StartCoroutine(Slide(root, park, 1.1f));
            return root.gameObject;
        }

        static IEnumerator Slide(Transform t, Vector3 to, float seconds)
        {
            Vector3 from = t.position;
            for (float k = 0f; k < 1f && t != null; k += Time.deltaTime / seconds)
            {
                float e = 1f - (1f - k) * (1f - k);
                t.position = Vector3.Lerp(from, to, e);
                yield return null;
            }
            if (t != null) t.position = to;
        }

        // ------------------------------------------------------------------ open window (Close)

        /// <summary>The neighbour leans out of an opened casement: two sashes swung outwards and a head.</summary>
        public static GameObject OpenWindow(Rect r, float facadeZ)
        {
            var root = new GameObject("Poison open window").transform;
            root.position = new Vector3(r.center.x, r.center.y, facadeZ - 0.05f);
            var frame = M("sash", new Color(0.92f, 0.9f, 0.84f));
            var dark = M("room", new Color(0.12f, 0.1f, 0.09f));
            var skin = M("skin", new Color(0.86f, 0.66f, 0.5f));
            var cloth = M("vest", new Color(0.85f, 0.85f, 0.88f));
            Part(PrimitiveType.Cube, root, new Vector3(0f, 0f, 0.02f), new Vector3(r.width * 0.92f, r.height * 0.92f, 0.02f), dark);
            float hw = r.width * 0.46f;
            Part(PrimitiveType.Cube, root, new Vector3(-r.width * 0.5f - hw * 0.35f, 0f, -hw * 0.35f), new Vector3(hw, r.height * 0.9f, 0.04f), frame, new Vector3(0f, 55f, 0f));
            Part(PrimitiveType.Cube, root, new Vector3(r.width * 0.5f + hw * 0.35f, 0f, -hw * 0.35f), new Vector3(hw, r.height * 0.9f, 0.04f), frame, new Vector3(0f, -55f, 0f));
            Part(PrimitiveType.Sphere, root, new Vector3(0f, r.height * 0.05f, -0.25f), Vector3.one * Mathf.Min(0.45f, r.height * 0.32f), skin);
            Part(PrimitiveType.Cube, root, new Vector3(0f, -r.height * 0.3f, -0.2f), new Vector3(r.width * 0.45f, r.height * 0.3f, 0.25f), cloth);
            return root.gameObject;
        }

        // ------------------------------------------------------------------ puddle (Reach)

        public static GameObject Puddle(Vector3 at)
        {
            var root = new GameObject("Poison puddle").transform;
            root.position = at;
            var water = M("water", new Color(0.35f, 0.55f, 0.75f, 0.7f), 0.95f);
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 0.012f, 0f), new Vector3(1.8f, 0.005f, 1.1f), water);
            Part(PrimitiveType.Cylinder, root, new Vector3(0.5f, 0.013f, 0.35f), new Vector3(0.7f, 0.005f, 0.5f), water);
            return root.gameObject;
        }

        // ------------------------------------------------------------------ wind (Accuracy)

        /// <summary>White streaks blowing across in front of the facade while the wind poison lasts.</summary>
        public static GameObject Wind(MonoBehaviour host, float facadeZ)
        {
            var root = new GameObject("Poison wind").transform;
            var streak = M("streak", new Color(1f, 1f, 1f, 0.55f));
            var list = new List<Transform>();
            for (int i = 0; i < 14; i++)
            {
                var s = Part(PrimitiveType.Cube, root, Vector3.zero, new Vector3(Random.Range(1.2f, 2.6f), 0.04f, 0.04f), streak).transform;
                s.position = new Vector3(Random.Range(-9f, 9f), Random.Range(1.5f, 9f), Random.Range(facadeZ - 6f, facadeZ - 1f));
                list.Add(s);
            }
            host.StartCoroutine(Blow(root, list));
            return root.gameObject;
        }

        static IEnumerator Blow(Transform root, List<Transform> streaks)
        {
            while (root != null)
            {
                foreach (var s in streaks)
                {
                    if (s == null) continue;
                    var p = s.position + Vector3.right * 9f * Time.deltaTime;
                    if (p.x > 10f) p.x = -10f;
                    s.position = p;
                }
                yield return null;
            }
        }

        // ------------------------------------------------------------------ pigeon (Chance)

        public static GameObject Pigeon()
        {
            var root = new GameObject("Pigeon").transform;
            var grey = M("pigeon", new Color(0.55f, 0.57f, 0.62f));
            var neck = M("pigeonneck", new Color(0.35f, 0.5f, 0.45f));
            var beak = M("beak", new Color(0.9f, 0.6f, 0.2f));
            Part(PrimitiveType.Sphere, root, Vector3.zero, new Vector3(0.38f, 0.26f, 0.26f), grey);
            Part(PrimitiveType.Sphere, root, new Vector3(0.22f, 0.08f, 0f), Vector3.one * 0.16f, neck);
            Part(PrimitiveType.Cube, root, new Vector3(0.32f, 0.07f, 0f), new Vector3(0.08f, 0.03f, 0.03f), beak);
            var l = Part(PrimitiveType.Cube, root, new Vector3(0f, 0.04f, 0.2f), new Vector3(0.3f, 0.02f, 0.4f), grey);
            var r = Part(PrimitiveType.Cube, root, new Vector3(0f, 0.04f, -0.2f), new Vector3(0.3f, 0.02f, 0.4f), grey);
            root.gameObject.AddComponent<Flap>().wings = new[] { l.transform, r.transform };
            return root.gameObject;
        }

        class Flap : MonoBehaviour
        {
            public Transform[] wings;
            void Update()
            {
                float a = Mathf.Sin(Time.time * 22f) * 50f;
                if (wings[0] != null) wings[0].localRotation = Quaternion.Euler(a, 0f, 0f);
                if (wings[1] != null) wings[1].localRotation = Quaternion.Euler(-a, 0f, 0f);
            }
        }

        // ------------------------------------------------------------------ antidotes: a short visible gesture

        /// <summary>What an antidote looks like for a moment. Returns the object; the caller destroys it later.</summary>
        public static GameObject Antidote(Tool a, Vector3 heroPos, float facadeZ)
        {
            var root = new GameObject("Antidote " + a.Id).transform;
            switch (a.Id)
            {
                case "fart": // a green cloud: nothing alive wants to pass
                    root.position = heroPos + Vector3.up * 0.6f;
                    var gas = M("gas", new Color(0.55f, 0.75f, 0.25f, 0.45f));
                    for (int i = 0; i < 6; i++)
                        Part(PrimitiveType.Sphere, root, Random.insideUnitSphere * 0.5f, Vector3.one * Random.Range(0.5f, 0.9f), gas);
                    break;
                case "nails": // nails scattered on the road: no car will come
                    root.position = new Vector3(0f, 0.02f, facadeZ - 6f);
                    var metal = M("nail", new Color(0.7f, 0.7f, 0.72f), 0.8f);
                    for (int i = 0; i < 40; i++)
                        Part(PrimitiveType.Cube, root, new Vector3(Random.Range(-6f, 6f), 0f, Random.Range(-1.5f, 1.5f)), new Vector3(0.12f, 0.02f, 0.02f), metal, new Vector3(0f, Random.Range(0f, 180f), 0f));
                    break;
                case "vane": // a weather vane on a pole beside the hero
                    root.position = heroPos + new Vector3(0.4f, 0f, 0f);
                    var iron = M("iron", new Color(0.15f, 0.15f, 0.17f));
                    Part(PrimitiveType.Cylinder, root, new Vector3(0f, 1.0f, 0f), new Vector3(0.05f, 1.0f, 0.05f), iron);
                    Part(PrimitiveType.Cube, root, new Vector3(0f, 2.05f, 0f), new Vector3(0.6f, 0.08f, 0.02f), iron);
                    Part(PrimitiveType.Cube, root, new Vector3(-0.3f, 2.05f, 0f), new Vector3(0.12f, 0.25f, 0.02f), iron);
                    break;
                case "plank": // a board over the puddle
                    root.position = heroPos + new Vector3(0f, 0.04f, 1.0f);
                    Part(PrimitiveType.Cube, root, Vector3.zero, new Vector3(1.6f, 0.05f, 0.4f), M("plank", new Color(0.6f, 0.42f, 0.24f)));
                    break;
                default: // pebble: a stone thrown at the open window
                    root.position = heroPos + Vector3.up * 1.4f;
                    Part(PrimitiveType.Sphere, root, Vector3.zero, Vector3.one * 0.15f, M("stone", new Color(0.5f, 0.48f, 0.45f)));
                    break;
            }
            return root.gameObject;
        }
    }
}
