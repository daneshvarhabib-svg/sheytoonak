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

        // ------------------------------------------------------------------ painted sprites (Art/Poison, 2026-11-04)

        static readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
        /// <summary>Art/Poison/&lt;name&gt;.png as a sprite (bottom-centre pivot unless told), or null: the primitive stays.</summary>
        static Sprite Art(string name, float pivotY = 0f)
        {
            string key = name + pivotY;
            if (sprites.TryGetValue(key, out var sp)) return sp;
            var tex = Resources.Load<Texture2D>("Art/Poison/" + name);
            sp = tex != null ? Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, pivotY), 100f) : null;
            if (sp != null) sp.name = name;
            sprites[key] = sp;
            return sp;
        }

        static SpriteRenderer Spr(Sprite sp, Transform parent, Vector3 local, float worldHeight, bool flat = false)
        {
            var go = new GameObject(sp.name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = local;
            if (flat) go.transform.localRotation = Quaternion.Euler(60f, 0f, 0f); // lying on the ground, tilted toward the low camera (at 90° the puddle was a thin line, run 62)
            var r = go.AddComponent<SpriteRenderer>();
            r.sprite = sp;
            go.transform.localScale = Vector3.one * (worldHeight / Mathf.Max(0.01f, sp.bounds.size.y));
            return r;
        }

        /// <summary>Plays sprite frames once (hold the last) or in a loop.</summary>
        class Frames : MonoBehaviour
        {
            public SpriteRenderer r; public Sprite[] frames; public float fps = 8f; public bool loop = true; public float delay;
            float t;
            void Update()
            {
                if (r == null || frames == null || frames.Length == 0) return;
                t += Time.deltaTime;
                if (t < delay) { r.sprite = frames[0]; return; }
                int i = (int)((t - delay) * fps);
                r.sprite = frames[loop ? i % frames.Length : Mathf.Min(i, frames.Length - 1)];
            }
        }

        // ------------------------------------------------------------------ bus (Cover)

        /// <summary>A tall city bus that drives in and parks in front of the covered windows.</summary>
        public static GameObject Bus(MonoBehaviour host, Rect[] covered, float facadeZ, string id = "bus", int level = 1)
        {
            // the vehicle stands in front of the ground floor; windows of the floor above (levels 2..5) are hidden by
            // the cargo stacked on its roof (DuelManager.TargetsFor), so the vehicle's size comes from the ground floor only
            float low = float.MaxValue; foreach (var r in covered) low = Mathf.Min(low, r.yMin);
            float xMin = float.MaxValue, xMax = float.MinValue, top = 0f, fullTop = 0f;
            var upper = new List<Rect>();
            foreach (var r in covered)
            {
                fullTop = Mathf.Max(fullTop, r.yMax);
                if (r.yMin > low + 1.5f) { upper.Add(r); continue; }
                xMin = Mathf.Min(xMin, r.xMin); xMax = Mathf.Max(xMax, r.xMax); top = Mathf.Max(top, r.yMax);
            }
            if (xMin > xMax) { foreach (var r in covered) { xMin = Mathf.Min(xMin, r.xMin); xMax = Mathf.Max(xMax, r.xMax); top = Mathf.Max(top, r.yMax); } }
            // a vehicle keeps its real length (bus 11, lorry 8.5, cart 3.5 m) even in front of a shop front wider than
            // itself: it parks on the middle of the covered glass (the rules count the window as hidden by its centre)
            float realLen = id == "cart" ? 3.5f : id == "truck" ? 8.5f : 11f;
            float len = Mathf.Clamp(xMax - xMin + 1.4f, realLen * 0.8f, realLen), w = 2.5f;
            // ⚠️ the bus stands ~3 m in front of the facade, so seen from the low camera its roof reads much higher
            // than its real height (run 30: it hid the middle row too). Size it so its front roof edge lands just above
            // the covered row *as projected onto the facade*.
            float frontZ = facadeZ - w - 0.6f, h = top + 0.25f;
            var cam = Camera.main;
            if (cam != null)
            {
                Vector3 c = cam.transform.position;
                float dFacade = facadeZ - c.z, dBus = frontZ - c.z;
                if (dFacade > 0.5f && dBus > 0.5f) h = c.y + (top + 0.2f - c.y) * dBus / dFacade - 0.25f;
            }
            h = Mathf.Clamp(h, 2.2f, top + 0.25f);
            var root = new GameObject("Poison bus").transform;
            // the same Cover prop for every vehicle poison: bus (zaatar), lorry (um al-arabiyat), pickle cart (hamada)
            var busArt = Art(id) ?? Art("bus");
            if (busArt != null)
            {
                // painted side views: the bus faces left in its picture, the lorry right; all drive in from the left
                // ⚠️ until 2026-11-05 the picture was scaled up until it covered every target window (and its height came
                // from the row it had to hide): with the bottom row broken a cart grew into a two-storey cart. User:
                // «گاری گاهی سایزش خیلی بزرگه … اتوبوس هم گاهی خیلی بزرگ می‌شه … نمی‌دونم قوانین سایز چیه». Rule now: every
                // vehicle has its real height (bus 3.4 m, lorry 3.6 m, cart 2.4 m), allowed to stretch at most 20 % so it
                // hides the house's lowest row; DuelManager.TargetsFor only gives it windows its length reaches.
                float real = id == "cart" ? 2.4f : id == "truck" ? 3.6f : 3.4f;
                h = Mathf.Clamp(h, real, real * 1.2f);
                var r = Spr(busArt, root, new Vector3(0f, 0f, -w * 0.5f), h);
                r.flipX = busArt.name.StartsWith("bus");
                AddWheels(r, busArt.name);
                // parked on a hidden (unbroken) window: the middle of the covered run can fall on broken glass between two
                // covered windows, so the covered window nearest that middle is the parking spot
                float mid = (xMin + xMax) * 0.5f, parkX = mid, bestD = float.MaxValue;
                foreach (var cr in covered) if (cr.yMin <= low + 1.5f && Mathf.Abs(cr.center.x - mid) < bestD) { bestD = Mathf.Abs(cr.center.x - mid); parkX = cr.center.x; }
                if (id == "cart") mid = parkX; // a cart is shorter than most windows: it stands squarely in front of one
                else mid = Mathf.Lerp(mid, parkX, 0.5f);
                // ⚠️ user (2026-11-10, screenshot): «این گاری هنوز نمیاد جلوی پنجره». The vehicle stands ~3.5 m in front of the
                // wall, so from the camera anything off-centre looks further out than the window at the same x (the cart
                // on the far left showed a whole window's width to the left of its window). It is placed where the line
                // from the camera to the window's centre crosses the vehicle's plane.
                float camX = 0f, k = 1f;
                if (cam != null)
                {
                    Vector3 c = cam.transform.position;
                    float dF = facadeZ - c.z, dV = frontZ - c.z;
                    if (dF > 0.5f && dV > 0.5f) { camX = c.x; k = dV / dF; }
                }
                mid = camX + (mid - camX) * k;
                Vector3 at = new Vector3(mid, 0f, facadeZ - w * 0.5f - 0.6f);
                if (level > 1) Cargo(root, at, h, r.bounds.size.x, upper, fullTop, facadeZ, frontZ, w, r.sortingOrder, level - 1);
                root.position = at + Vector3.left * 30f;
                host.StartCoroutine(Slide(root, at, 1.1f));
                return root.gameObject;
            }
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

        /// <summary>
        /// Levels of a vehicle poison (user 2026-11-09): «ارتفاع می‌گیرن … یسری جعبه و بسته و چمدون و این چیزها میاد روشون».
        /// <paramref name="items"/> pieces of luggage piled on the roof, tall enough (as seen from the camera) to hide the
        /// windows of the floor above that the rules hid. Painted Art/Poison/cargo_0..3 when they exist, else boxes.
        /// </summary>
        static void Cargo(Transform root, Vector3 at, float roofH, float roofLen, List<Rect> upper, float fullTop, float facadeZ, float frontZ, float w, int order, int items)
        {
            // the height the pile must reach in front of the facade, projected like the vehicle itself
            float need = fullTop + 0.2f;
            var cam = Camera.main;
            if (cam != null)
            {
                Vector3 c = cam.transform.position;
                float dFacade = facadeZ - c.z, dFront = frontZ - c.z;
                if (dFacade > 0.5f && dFront > 0.5f) need = c.y + (fullTop + 0.2f - c.y) * dFront / dFacade;
            }
            float pile = Mathf.Max(0.8f * items, need - roofH);
            float each = pile / items;
            float cx = 0f; foreach (var r in upper) cx += r.center.x;
            float camX = 0f, k = 1f;
            if (cam != null)
            {
                Vector3 c = cam.transform.position;
                float dF = facadeZ - c.z, dV = frontZ - c.z;
                if (dF > 0.5f && dV > 0.5f) { camX = c.x; k = dV / dF; }
            }
            cx = upper.Count > 0 ? camX + (cx / upper.Count - camX) * k - at.x : 0f; // same parallax as the vehicle
            // ⚠️ run 206: the pile floated beside the bus at the hidden window's x and a little above the roof. It stands
            // on the roof (the painted roof line is a little under the picture's top: 0.92) and never past its ends.
            float halfLen = roofLen * 0.5f - 0.9f;
            cx = Mathf.Clamp(cx, -Mathf.Max(0f, halfLen), Mathf.Max(0f, halfLen));
            float y = roofH * 0.92f;
            for (int i = 0; i < items; i++)
            {
                float jitter = (i % 2 == 0 ? 0.25f : -0.3f);
                var local = new Vector3(cx + jitter, y, -w * 0.5f - 0.05f);
                var art = Art("cargo_" + (i % 4));
                if (art != null)
                {
                    var sr = Spr(art, root, local, each * 1.05f);
                    sr.sortingOrder = order;
                }
                else
                {
                    Color[] tint = { new Color(0.62f, 0.45f, 0.26f), new Color(0.6f, 0.15f, 0.12f), new Color(0.8f, 0.72f, 0.5f), new Color(0.25f, 0.35f, 0.6f) };
                    float bw = Mathf.Lerp(1.3f, 2.1f, (i * 0.37f) % 1f);
                    Part(PrimitiveType.Cube, root, local + Vector3.up * each * 0.5f, new Vector3(bw, each * 0.95f, 1.2f), M("cargo" + (i % 4), tint[i % 4]), new Vector3(0f, 0f, jitter * 6f));
                    // a strap so it reads as luggage, not a block
                    Part(PrimitiveType.Cube, root, local + new Vector3(0f, each * 0.5f, -0.61f), new Vector3(0.12f, each * 0.97f, 0.02f), M("strap", new Color(0.15f, 0.1f, 0.06f)));
                }
                y += each;
            }
        }

        /// <summary>
        /// Loose wheels for a vehicle picture listed in Art/Poison/wheels.txt (picture with empty arches, a wheel picture,
        /// the radius and the centres in the picture's pixels). User (2026-11-12): «اتوبوس و کامیون و گاری باید چرخشون
        /// بچرخه مثل ماشین های گذری». A <see cref="WheelSpin"/> turns them by the distance the vehicle moves.
        /// </summary>
        static void AddWheels(SpriteRenderer body, string picture)
        {
            var txt = Resources.Load<TextAsset>("Art/Poison/wheels");
            if (txt == null) return;
            foreach (var line in txt.text.Split('\n'))
            {
                var f = line.Trim().Split(' ');
                if (f.Length < 4 || f[0].StartsWith("#") || f[0] != picture) continue;
                var wheel = Art(f[1], 0.5f);
                if (wheel == null) return;
                var inv = System.Globalization.CultureInfo.InvariantCulture;
                float rad = float.Parse(f[2], inv);
                var tex = body.sprite.texture;
                var spin = body.gameObject.AddComponent<WheelSpin>();
                spin.radius = rad / 100f * body.transform.lossyScale.y;
                // "front": the wheels stand in front of the body (a cart's wheels are outside it); else the arches cover them
                bool front = System.Array.IndexOf(f, "front") >= 0;
                for (int i = 3; i < f.Length; i++)
                {
                    if (f[i] == "front") continue;
                    var c = f[i].Split(',');
                    float cx = float.Parse(c[0], inv), cy = float.Parse(c[1], inv);
                    var w = new GameObject("Wheel").AddComponent<SpriteRenderer>();
                    w.sprite = wheel; w.sortingOrder = body.sortingOrder + (front ? 1 : -1); // arches cover the tops of the tyres
                    w.transform.SetParent(body.transform, false);
                    float x = (cx - tex.width * 0.5f) / 100f;
                    w.transform.localPosition = new Vector3(body.flipX ? -x : x, (tex.height - cy) / 100f, 0.01f);
                    w.transform.localScale = Vector3.one * (2f * rad / wheel.rect.width);
                    spin.wheels.Add(w.transform);
                }
                return;
            }
        }

        /// <summary>Turns the loose wheels by the distance driven (rolling right = clockwise).</summary>
        class WheelSpin : MonoBehaviour
        {
            public readonly List<Transform> wheels = new List<Transform>();
            public float radius = 1f;
            float lastX; bool started;
            void LateUpdate()
            {
                float x = transform.position.x;
                if (started && radius > 0.001f)
                {
                    float deg = (x - lastX) / radius * Mathf.Rad2Deg;
                    foreach (var w in wheels) if (w != null) w.Rotate(0f, 0f, -deg, Space.Self);
                }
                lastX = x; started = true;
            }
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
        /// <summary>The sealed window of an even house (review 2026-11-07 §2.2): a rolled-down corrugated shutter, visibly
        /// not a target. Painted art (Art/shutter) when it exists, else grey slats.</summary>
        public static GameObject Shutter(Rect r, float facadeZ)
        {
            var root = new GameObject("Sealed window shutter").transform;
            root.position = new Vector3(r.center.x, r.center.y, facadeZ - 0.04f);
            var art = Art("shutter", 0.5f);
            if (art != null)
            {
                var sr = Spr(art, root, Vector3.zero, r.height * 1.04f, true);
                sr.transform.localScale = new Vector3(r.width * 1.06f / Mathf.Max(0.01f, art.bounds.size.x), sr.transform.localScale.y, 1f);
                return root.gameObject;
            }
            var steel = M("shutter", new Color(0.42f, 0.43f, 0.45f), 0.35f);
            var dark = M("shutter_dark", new Color(0.26f, 0.27f, 0.29f), 0.2f);
            Part(PrimitiveType.Cube, root, Vector3.zero, new Vector3(r.width * 1.06f, r.height * 1.04f, 0.03f), dark);
            int slats = Mathf.Max(4, Mathf.RoundToInt(r.height / 0.18f));
            for (int i = 0; i < slats; i++)
            {
                float y = r.height * 0.5f - (i + 0.5f) * r.height / slats;
                Part(PrimitiveType.Cube, root, new Vector3(0f, y, -0.02f), new Vector3(r.width * 1.06f, r.height / slats * 0.55f, 0.02f), steel);
            }
            return root.gameObject;
        }

        public static GameObject OpenWindow(Rect r, float facadeZ)
        {
            var root = new GameObject("Poison open window").transform;
            root.position = new Vector3(r.center.x, r.center.y, facadeZ - 0.05f);
            // User (2026-11-09): «تصویر زنی که پنجره رو باز کرده هنوز مسخره است». The cartoon casement (window_open_0/1) is
            // retired: the neighbour who opened the window is one of the painted residents (the ones who curse), leaning
            // out of a dark open frame, in the facades' own style.
            string[] who = { "granny2", "resident_01", "resident_02", "resident_03", "resident_04", "resident_05" };
            var res = Art(who[Random.Range(0, who.Length)], 0f);
            if (res != null)
            {
                Part(PrimitiveType.Cube, root, new Vector3(0f, 0f, 0.03f), new Vector3(r.width * 0.9f, r.height * 0.9f, 0.02f), M("room", new Color(0.08f, 0.07f, 0.07f)));
                var sr = Spr(res, root, new Vector3(0f, -r.height * 0.5f, -0.02f), 1f);
                float k = r.height * 1.05f / Mathf.Max(0.01f, res.bounds.size.y);
                if (res.bounds.size.x * k > r.width * 1.1f) k = r.width * 1.1f / res.bounds.size.x;
                sr.transform.localScale = Vector3.one * k;
                return root.gameObject;
            }
            var w0 = Art("window_open_0", 0.5f); var w1 = Art("window_open_1", 0.5f);
            if (w0 != null && w1 != null)
            {
                // the casement swings open, then the angry neighbour leans out (two painted frames); the painted frame
                // is ~60% of the picture's width (the sashes stick out), so it is sized from the window's width
                var sr = Spr(w0, root, Vector3.zero, 1f);
                sr.transform.localScale = Vector3.one * (r.width * 1.55f / Mathf.Max(0.01f, w0.bounds.size.x));
                var f = sr.gameObject.AddComponent<Frames>(); f.r = sr; f.frames = new[] { w0, w1 }; f.fps = 1f / 0.45f; f.loop = false; f.delay = 0f;
                return root.gameObject;
            }
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

        // ------------------------------------------------------------------ carpet (Close, rima)

        /// <summary>
        /// A rug thrown over the parapet, unrolling down the facade over <paramref name="span"/> (roof to just under the
        /// lowest covered window). Art/Poison/carpet.png is a long hanging rug, fringe at the bottom: the part that shows
        /// is cut from its bottom up (the rest is "still on the roof"), so the fringe always ends the rug; only when the
        /// span is longer than the painting is it stretched. Until it is painted, a striped red rug.
        /// </summary>
        public static GameObject Carpet(MonoBehaviour host, Rect span, float facadeZ)
        {
            var root = new GameObject("Poison carpet").transform;
            root.position = new Vector3(span.center.x, span.yMax, facadeZ - 0.06f); // pivot at the roof edge
            var tex = Resources.Load<Texture2D>("Art/Poison/carpet");
            if (tex != null) { host.StartCoroutine(Unroll(root, tex, span)); return root.gameObject; }
            var body = new GameObject("rug").transform; body.SetParent(root, false);
            var red = M("rug", new Color(0.62f, 0.12f, 0.12f)); var gold = M("rugstripe", new Color(0.85f, 0.65f, 0.25f));
            Part(PrimitiveType.Cube, body, new Vector3(0f, -0.5f, 0f), new Vector3(1f, 1f, 0.02f), red);
            for (int i = 1; i < 6; i++) Part(PrimitiveType.Cube, body, new Vector3(0f, -i / 6f, -0.012f), new Vector3(0.9f, 0.025f, 0.01f), gold);
            body.localScale = new Vector3(span.width, span.height, 1f);
            return root.gameObject;
        }

        /// <summary>
        /// User (2026-11-09): «فرش باید فرآیند پهن شدن داشته باشه، عین یه فرش لوله شده که از بالای پشت بوم پهن میشه و تو دو
        /// ثانیه کل پنجره رو بگیره … میتونی چند صدتا کات بدی از فرش و فرایند انیمیشنی بسازی». 120 cuts of the painting
        /// (one sprite each, all sharing the one texture): the part already hanging grows from the roof down at its true
        /// scale (never squashed), and under its lower edge rides the roll -- the next band of the rug pressed into a
        /// cylinder, darker at its rim, thinner as it unwinds. The fringe, at the end of the rug, comes out last.
        /// Smooth (ease-in-out, 2 s), no jumps: every frame picks the cut for its exact length.
        /// </summary>
        static IEnumerator Unroll(Transform root, Texture2D tex, Rect span)
        {
            const int Cuts = 120; const float Seconds = 2f;
            float scale = span.width / (tex.width / 100f);              // painting pixels → metres (same both ways)
            float shownPx = Mathf.Min(tex.height, span.height / scale * 100f); // the part of the rug that will hang
            var cuts = new Sprite[Cuts + 1];
            for (int i = 1; i <= Cuts; i++)
            {
                float h = Mathf.Max(1f, shownPx * i / Cuts);
                cuts[i] = Sprite.Create(tex, new Rect(0, tex.height - h, tex.width, h), new Vector2(0.5f, 1f), 100f);
            }
            var hang = new GameObject("rug").AddComponent<SpriteRenderer>();
            hang.transform.SetParent(root, false); hang.transform.localScale = Vector3.one * scale;
            var roll = new GameObject("roll").AddComponent<SpriteRenderer>();
            roll.transform.SetParent(root, false);
            roll.sortingOrder = hang.sortingOrder + 1;
            var rim = new GameObject("rim").AddComponent<SpriteRenderer>();
            rim.transform.SetParent(roll.transform, false);
            rim.sortingOrder = roll.sortingOrder + 1;
            rim.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
            rim.color = new Color(0f, 0f, 0f, 0.35f);
            float diam0 = Mathf.Clamp(span.width * 0.32f, 0.25f, 0.6f); // the rolled rug's thickness on the roof
            for (float t = 0f; ; t += Time.deltaTime / Seconds)
            {
                if (root == null) yield break;
                float k = Mathf.Clamp01(t), e = k * k * (3f - 2f * k);  // smoothstep: it starts slow, rolls, settles
                int i = Mathf.Clamp(Mathf.RoundToInt(e * Cuts), 1, Cuts);
                hang.sprite = cuts[i];
                float hungPx = shownPx * i / Cuts, left = shownPx - hungPx;
                float diam = diam0 * Mathf.Sqrt(left / shownPx);           // what is still rolled up
                if (left > 1f && diam > 0.02f)
                {
                    // the roll: the band right under the hanging part, pressed to the roll's thickness
                    float band = Mathf.Min(left, Mathf.Max(8f, tex.height * 0.06f));
                    if (roll.sprite != null) Object.Destroy(roll.sprite);
                    roll.sprite = Sprite.Create(tex, new Rect(0, tex.height - hungPx - band, tex.width, band), new Vector2(0.5f, 0.5f), 100f);
                    roll.transform.localPosition = new Vector3(0f, -hungPx / 100f * scale - diam * 0.35f, -0.03f);
                    roll.transform.localScale = new Vector3(scale * 1.02f, diam / (band / 100f), 1f);
                    roll.color = new Color(0.8f, 0.76f, 0.74f);
                    rim.transform.localScale = new Vector3(tex.width / 100f, band / 100f * 0.18f, 1f);
                    rim.transform.localPosition = new Vector3(0f, -band / 100f * 0.42f, -0.01f);
                    roll.enabled = rim.enabled = true;
                }
                else roll.enabled = rim.enabled = false;
                if (k >= 1f) break;
                yield return null;
            }
            if (roll != null) Object.Destroy(roll.gameObject);
        }

        // ------------------------------------------------------------------ puddle (Reach)

        /// <param name="level">a deeper, wider puddle at each level (and the antidote plank must be as strong)</param>
        public static GameObject Puddle(Vector3 at, int level = 1)
        {
            var root = new GameObject("Poison puddle").transform;
            root.position = at;
            root.localScale = Vector3.one * (1f + 0.22f * (Mathf.Clamp(level, 1, 5) - 1));
            var pa = Art("puddle", 0.5f);
            if (pa != null) { Spr(pa, root, new Vector3(0f, 0.012f, 0f), 1.4f, true); return root.gameObject; }
            var water = M("water", new Color(0.35f, 0.55f, 0.75f, 0.7f), 0.95f);
            Part(PrimitiveType.Cylinder, root, new Vector3(0f, 0.012f, 0f), new Vector3(1.8f, 0.005f, 1.1f), water);
            Part(PrimitiveType.Cylinder, root, new Vector3(0.5f, 0.013f, 0.35f), new Vector3(0.7f, 0.005f, 0.5f), water);
            return root.gameObject;
        }

        // ------------------------------------------------------------------ rain (Reach, bubakr)

        /// <summary>A rain cloud over the street in front of the house, bobbing, while the rain poison lasts (with a puddle).</summary>
        public static GameObject Rain(MonoBehaviour host, Vector3 at, int level = 1)
        {
            var art = Art("rain", 0.5f);
            if (art == null) return null;
            var root = new GameObject("Poison rain").transform;
            root.position = at;
            var r = Spr(art, root, Vector3.zero, 2.4f * (1f + 0.2f * (Mathf.Clamp(level, 1, 5) - 1)));
            host.StartCoroutine(Bob(root));
            return root.gameObject;
        }

        static IEnumerator Bob(Transform t)
        {
            Vector3 p0 = t != null ? t.position : Vector3.zero;
            while (t != null) { t.position = p0 + new Vector3(Mathf.Sin(Time.time * 0.6f) * 0.8f, Mathf.Sin(Time.time * 1.3f) * 0.1f, 0f); yield return null; }
        }

        // ------------------------------------------------------------------ wind (Accuracy)

        /// <summary>White streaks blowing across in front of the facade while the wind poison lasts.</summary>
        public static GameObject Wind(MonoBehaviour host, float facadeZ, int level = 1)
        {
            var root = new GameObject("Poison wind").transform;
            var streak = M("streak", new Color(1f, 1f, 1f, 0.45f));
            var list = new List<Transform>();
            // run 30: 14 streaks up to 9 m high cluttered the whole frame and the HUD band; keep a few, close to the wall
            // more streaks at each level (7 … 19): the wind you see is the wind the kick gets
            for (int i = 0; i < 7 + 3 * (Mathf.Clamp(level, 1, 5) - 1); i++)
            {
                var s = Part(PrimitiveType.Cube, root, Vector3.zero, new Vector3(Random.Range(0.8f, 1.6f), 0.03f, 0.03f), streak).transform;
                s.position = new Vector3(Random.Range(-6f, 6f), Random.Range(1.6f, 5.5f), Random.Range(facadeZ - 2f, facadeZ - 0.6f));
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
                    if (p.x > 7f) p.x = -7f;
                    s.position = p;
                }
                yield return null;
            }
        }

        // ------------------------------------------------------------------ pigeon (Chance)

        public static GameObject Pigeon()
        {
            var root = new GameObject("Pigeon").transform;
            var p0 = Art("pigeon_0", 0.5f); var p1 = Art("pigeon_1", 0.5f); var p2 = Art("pigeon_2", 0.5f);
            if (p0 != null && p1 != null && p2 != null)
            {
                var sr = Spr(p0, root, Vector3.zero, 0.38f);
                var f = sr.gameObject.AddComponent<Frames>(); f.r = sr; f.frames = new[] { p0, p1, p2, p1 }; f.fps = 12f;
                return root.gameObject;
            }
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
            if (a.Id == "rug_push") { root.position = heroPos; return root.gameObject; } // the stage pushes the rug off the roof (DuelStage.RugFall)
            var art = Art(a.Id, a.Id == "fart" || a.Id == "nails" || a.Id == "plank" || a.Id == "complaint" || a.Id == "flashlight" ? 0.5f : 0f);
            if (art != null)
            {
                switch (a.Id)
                {
                    case "fart": root.position = heroPos + Vector3.up * 0.9f; Spr(art, root, Vector3.zero, 1.3f); break;
                    case "nails":
                        root.position = new Vector3(0f, 0.02f, facadeZ - 6f);
                        for (int i = 0; i < 7; i++) Spr(art, root, new Vector3(-6f + i * 2f + Random.Range(-0.4f, 0.4f), 0f, Random.Range(-1f, 1f)), 0.6f, true);
                        break;
                    case "vane": root.position = heroPos + new Vector3(heroPos.x < 0f ? -0.6f : 0.6f, 0f, 0.3f); Spr(art, root, Vector3.zero, 1.4f); break;
                    case "plank": root.position = heroPos + new Vector3(0f, 0.04f, 1.0f); Spr(art, root, Vector3.zero, 0.9f, true); break;
                    case "complaint": root.position = heroPos + new Vector3(0f, 1.9f, 0f); Spr(art, root, Vector3.zero, 0.7f); break; // waved over the head
                    case "umbrella": root.position = heroPos + new Vector3(heroPos.x < 0f ? 0.3f : -0.3f, 0.6f, -0.1f); Spr(art, root, Vector3.zero, 1.5f); break;
                    case "flashlight": root.position = heroPos + new Vector3(0f, 1.2f, 0.2f); Spr(art, root, Vector3.zero, 0.45f); break;
                    default: root.position = heroPos + Vector3.up * 1.4f; Spr(art, root, Vector3.zero, 0.22f); break; // pebble
                }
                return root.gameObject;
            }
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
                    root.position = heroPos + new Vector3(heroPos.x < 0f ? -0.5f : 0.5f, 0f, 0.3f);
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
