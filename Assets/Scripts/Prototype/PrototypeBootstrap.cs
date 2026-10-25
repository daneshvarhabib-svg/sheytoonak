using System.Collections;
using Sheytoonak.Core;
using UnityEngine;

namespace Sheytoonak.Prototype
{
    /// <summary>
    /// Builds a complete, playable placeholder version of the game from code: camera, light, street,
    /// three grey-box buildings with window quads, a placeholder player seen from behind, a football, input, the game
    /// manager and a simple English IMGUI HUD. It exists so the project can be built in the cloud
    /// without anyone opening the Unity Editor. The real art replaces this later (docs/CODEX_BRIEF.md).
    /// The editor build script puts one of these in an otherwise empty scene.
    /// </summary>
    public class PrototypeBootstrap : MonoBehaviour
    {
        [Header("Materials (assigned by the editor build script, so their shaders are included in the build)")]
        public Material brick;
        public Material glass;
        public Material broken;
        public Material frame;
        public Material asphalt;
        public Material sidewalk;
        public Material neighbour;
        public Material shop;
        public Material awning;
        public Material jeans;
        public Material shoe;
        public Material ball;
        public Material metal;

        [Tooltip("The camera follows the player sideways along the sidewalk.")]
        public bool cameraFollowsPlayer = true;
        // Point of view (user, 2026-10-25, after seeing the placeholder body: "بهتره بازی از دید POV باشه که این
        // کاراکتر رو نداشته باشیم"). Eye height, a little behind the strike spot, slightly looking down so the
        // ball at the feet and the whole facade are both in frame. 2026-10-23..25 it was third-person from behind.
        // Geometry check (not a guess): ball at z 0.45 must be inside the bottom of the frame and the top
        // window row (~7.9 m on a 13 m painting at 13 m) inside the top. From (1.7, -2.0) the ball is 35 deg
        // below the horizon and the top row 22 deg above; pitch 12 with a vertical FOV of 70 covers -47..+23.
        public Vector3 cameraOffset = new Vector3(0f, 1.7f, -2.0f);
        public float cameraPitchDeg = 12f;
        public float cameraFov = 70f;

        static readonly Color[] BrickTints =
        {
            new Color(0.64f, 0.33f, 0.23f), new Color(0.71f, 0.42f, 0.27f), new Color(0.58f, 0.30f, 0.24f)
        };

        Camera cam;
        GameManager game;
        PlayerFoot foot;
        BallController ballCtl;
        MotionKickInput motion;
        TouchKickInput touch;
        KickInputRouter router;
        Transform facadeRoot, oldFacadeRoot;
        Renderer[] windowRenderers = new Renderer[0];
        float wind;
        string toast = "";
        float toastUntil;
        string endTitle;
        int endScore, endBest, endMedals;
        bool showStart = true;
        string calibrationText = "";

        // ------------------------------------------------------------------ setup

        void Awake()
        {
            EnsureMaterials();
            BuildCameraAndLight();
            BuildStreet();
            BuildGameObjects();
        }

        void OnEnable()
        {
            GameEvents.HouseLoaded += OnHouseLoaded;
            GameEvents.WindowBroken += OnWindowBroken;
            GameEvents.WalkStarted += OnWalkStarted;
            GameEvents.Kicked += OnKicked;
            GameEvents.KickRejected += OnKickRejected;
            GameEvents.LifeLost += OnLifeLost;
            GameEvents.BallInHouse += OnBallInHouse;
            GameEvents.MedalEarned += OnMedal;
            GameEvents.Bounced += OnBounced;
            GameEvents.WindChanged += OnWind;
            GameEvents.GameOver += OnGameOver;
            GameEvents.Victory += OnVictory;
        }

        void OnDisable()
        {
            GameEvents.HouseLoaded -= OnHouseLoaded;
            GameEvents.WindowBroken -= OnWindowBroken;
            GameEvents.WalkStarted -= OnWalkStarted;
            GameEvents.Kicked -= OnKicked;
            GameEvents.KickRejected -= OnKickRejected;
            GameEvents.LifeLost -= OnLifeLost;
            GameEvents.BallInHouse -= OnBallInHouse;
            GameEvents.MedalEarned -= OnMedal;
            GameEvents.Bounced -= OnBounced;
            GameEvents.WindChanged -= OnWind;
            GameEvents.GameOver -= OnGameOver;
            GameEvents.Victory -= OnVictory;
        }

        void EnsureMaterials()
        {
            // Fallback for someone dropping the component into a scene by hand: works in the Editor,
            // but in a build Shader.Find only succeeds when a material asset already references the shader.
            brick = brick ? brick : Mat(new Color(0.64f, 0.33f, 0.23f));
            glass = glass ? glass : Mat(new Color(0.45f, 0.65f, 0.85f), 0.9f);
            broken = broken ? broken : Mat(new Color(0.06f, 0.05f, 0.05f));
            frame = frame ? frame : Mat(new Color(0.85f, 0.80f, 0.70f));
            asphalt = asphalt ? asphalt : Mat(new Color(0.27f, 0.28f, 0.30f));
            sidewalk = sidewalk ? sidewalk : Mat(new Color(0.78f, 0.75f, 0.69f));
            neighbour = neighbour ? neighbour : Mat(new Color(0.80f, 0.78f, 0.72f));
            shop = shop ? shop : Mat(new Color(0.95f, 0.78f, 0.45f));
            awning = awning ? awning : Mat(new Color(0.12f, 0.42f, 0.35f));
            jeans = jeans ? jeans : Mat(new Color(0.17f, 0.25f, 0.47f));
            shoe = shoe ? shoe : Mat(new Color(0.95f, 0.95f, 0.93f));
            ball = ball ? ball : Mat(Color.white, 0.4f);
            metal = metal ? metal : Mat(new Color(0.17f, 0.18f, 0.2f));
        }

        static Material Mat(Color c, float smooth = 0.1f)
        {
            var m = new Material(Shader.Find("Standard")) { color = c };
            m.SetFloat("_Glossiness", smooth);
            return m;
        }

        void BuildCameraAndLight()
        {
            var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
            cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.55f, 0.76f, 0.95f);
            cam.fieldOfView = cameraFov;
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 250f;
            camGo.transform.SetPositionAndRotation(cameraOffset, Quaternion.Euler(cameraPitchDeg, 0f, 0f));
            camGo.AddComponent<AudioListener>();

            var sun = new GameObject("Sun").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.93f, 0.82f);
            sun.intensity = 1.15f;
            sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(45f, -35f, 0f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.55f, 0.55f, 0.6f);
        }

        void BuildStreet()
        {
            var root = new GameObject("Street (visual)").transform;
            Box(root, new Vector3(0f, -0.05f, 0f), new Vector3(80f, 0.1f, 6f), sidewalk);           // near sidewalk, z -3..3
            Box(root, new Vector3(0f, 0.04f, 3.0f), new Vector3(80f, 0.16f, 0.25f), frame);          // curb
            Box(root, new Vector3(0f, -0.06f, 18f), new Vector3(80f, 0.1f, 30f), asphalt);           // street and beyond
            for (int i = -6; i <= 6; i++)
                Box(root, new Vector3(i * 4f, -0.005f, 8.0f), new Vector3(2f, 0.01f, 0.15f), shoe);   // centre line
            Box(root, new Vector3(0f, -0.04f, 12f), new Vector3(80f, 0.1f, 2f), sidewalk);           // far sidewalk
            Box(root, new Vector3(-3.4f, 3f, 2.4f), new Vector3(0.12f, 6f, 0.12f), metal);           // lamp post
            Box(root, new Vector3(-2.9f, 5.95f, 2.4f), new Vector3(1.0f, 0.1f, 0.12f), metal);
            for (int i = 0; i < 2; i++)
                Box(root, new Vector3(2.6f + i * 1.6f, 0.45f, 2.6f), new Vector3(0.1f, 0.9f, 0.1f), metal); // bollards
        }

        void BuildGameObjects()
        {
            var config = ScriptableObject.CreateInstance<GameConfig>();
            var houses = new[] { House1(), House2(), House3() };

            // Player: foot position + window mapping. No body: the game is first-person (POV).
            var player = new GameObject("Player");
            foot = player.AddComponent<PlayerFoot>();
            var mapper = player.AddComponent<BackdropWindowMapper>();
            mapper.cam = cam;

            // Football: a sphere with a procedural black-and-white texture.
            var ballGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ballGo.name = "Ball";
            ballGo.transform.localScale = Vector3.one * (config.ballRadius * 2f);
            var r = ballGo.GetComponent<Renderer>();
            r.sharedMaterial = ball;
            r.material.mainTexture = FootballTexture();
            ballGo.GetComponent<SphereCollider>().sharedMaterial = new PhysicsMaterial("Ball")
            {
                bounciness = 0.6f,
                dynamicFriction = 0.5f,
                staticFriction = 0.6f,
                bounceCombine = PhysicsMaterialCombine.Maximum,
                frictionCombine = PhysicsMaterialCombine.Average
            };
            ballGo.transform.position = new Vector3(0f, config.ballRadius, config.serveForwardOffset);
            ballGo.AddComponent<Rigidbody>().isKinematic = true; // rests until the first serve
            ballCtl = ballGo.AddComponent<BallController>();

            // Input: fields must be set before the components are enabled, so build inactive.
            var inputGo = new GameObject("Input");
            inputGo.SetActive(false);
            motion = inputGo.AddComponent<MotionKickInput>();
            motion.config = config;
            touch = inputGo.AddComponent<TouchKickInput>();
            router = inputGo.AddComponent<KickInputRouter>();
            router.motion = motion;
            router.touch = touch;
            inputGo.SetActive(true);

            var gameGo = new GameObject("Game");
            gameGo.SetActive(false);
            game = gameGo.AddComponent<GameManager>();
            game.config = config;
            game.houses = houses;
            game.ball = ballCtl;
            game.foot = foot;
            game.mapper = mapper;
            game.inputSource = router;
            game.autoStart = false;
            gameGo.AddComponent<ProceduralSfx>();
            var overlay = gameGo.AddComponent<SensorDebugOverlay>();
            overlay.motion = motion;
            overlay.game = game;
            gameGo.SetActive(true);
        }

        // ------------------------------------------------------------------ houses (placeholder data)

        // Easier since 2026-10-24 (user: "بازی سختیه و باید ساده‌تر شه"): slower returns, the ball lands
        // closer to the player's line (spread), bigger window hit zones, less wind.
        // House 1 is the first painted facade (ChatGPT, 2026-10-25, docs/ART_SPEC.fa.md). Window rectangles are
        // pixel bounds measured on the 1536x1024 image (frame included), converted to uv with origin bottom-left.
        HouseDefinition House1() => MakePaintedHouse("House 1", "Art/house_01", 1536f, 1024f, 13f,
            new[] { new Rect(268f, 40f, 224f, 205f), new Rect(662f, 40f, 218f, 205f), new Rect(1055f, 40f, 220f, 205f),
                    new Rect(268f, 350f, 224f, 205f), new Rect(662f, 350f, 218f, 205f), new Rect(1055f, 350f, 220f, 205f) },
            0f, 2.1f, 0.4f, 1.15f);
        HouseDefinition House2() => MakePaintedHouse("House 2", "Art/house_02", 1536f, 1024f, 13f,
            Grid(new[] { (268f, 490f), (660f, 880f), (1055f, 1275f) }, new[] { (25f, 195f), (265f, 440f), (510f, 675f) }),
            0.25f, 1.95f, 0.7f, 1.1f);
        HouseDefinition House3() => MakePaintedHouse("House 3", "Art/house_03", 1536f, 1024f, 13f,
            Grid(new[] { (215f, 400f), (540f, 720f), (860f, 1045f), (1185f, 1365f) }, new[] { (30f, 190f), (265f, 430f), (500f, 640f) }),
            0.5f, 1.8f, 1.0f, 1.0f);

        /// <summary>Pixel rects for a regular grid of windows: column x-ranges by row y-ranges (image pixels, origin top-left).</summary>
        static Rect[] Grid((float, float)[] cols, (float, float)[] rows)
        {
            var rects = new Rect[cols.Length * rows.Length];
            int i = 0;
            foreach (var (y0, y1) in rows)
                foreach (var (x0, x1) in cols)
                    rects[i++] = Rect.MinMaxRect(x0, y0, x1, y1);
            return rects;
        }

        /// <summary>Facade from a painting: pixel rects (origin top-left, as measured on the image) become world rects on a quad w metres wide.</summary>
        static HouseDefinition MakePaintedHouse(string name, string texture, float imgW, float imgH, float w, Rect[] pixelWindows,
                                                float wind, float returnTime, float spread, float hitScale)
        {
            float h = w * imgH / imgW;
            var house = ScriptableObject.CreateInstance<HouseDefinition>();
            house.name = name;
            house.displayName = name;
            house.paintedTexture = texture;
            house.facadeDistance = 13f;
            house.worldRoofHeight = h;          // where the painting (and the windows) stop; the wall itself continues
            house.worldFacadeWidth = w;
            house.windRange = new Vector2(-wind, wind);
            house.returnFlightTime = returnTime;
            house.returnSpreadX = spread;
            house.windowHitScale = hitScale;
            var rects = new Rect[pixelWindows.Length];
            for (int i = 0; i < rects.Length; i++)
            {
                Rect p = pixelWindows[i];
                float x0 = (p.xMin / imgW - 0.5f) * w, x1 = (p.xMax / imgW - 0.5f) * w;
                float y0 = (1f - p.yMax / imgH) * h, y1 = (1f - p.yMin / imgH) * h;
                rects[i] = Rect.MinMaxRect(x0, y0, x1, y1);
            }
            house.worldWindows = rects;
            house.windows = new WindowSpot[rects.Length];
            return house;
        }

        static HouseDefinition MakeHouse(string name, float[] rows, float[] cols, float w, float h, float roof,
                                         float wind, float returnTime, float spread, float hitScale)
        {
            var house = ScriptableObject.CreateInstance<HouseDefinition>();
            house.name = name;
            house.displayName = name;
            house.facadeDistance = 13f; // was 10; user 2026-10-24: "فاصله فرد رو از دیوار بیشتر کن"
            house.worldRoofHeight = roof;
            house.worldFacadeWidth = 9f;
            house.windRange = new Vector2(-wind, wind);
            house.returnFlightTime = returnTime;
            house.returnSpreadX = spread;
            house.windowHitScale = hitScale;
            var rects = new Rect[rows.Length * cols.Length];
            int i = 0;
            foreach (float y in rows)
                foreach (float x in cols)
                    rects[i++] = new Rect(x - w * 0.5f, y - h * 0.5f, w, h);
            house.worldWindows = rects;
            house.windows = new WindowSpot[rects.Length]; // rules count windows from this array
            return house;
        }

        // ------------------------------------------------------------------ events -> visuals

        void OnHouseLoaded(HouseDefinition h, int index)
        {
            if (facadeRoot != null) Destroy(facadeRoot.gameObject);
            if (oldFacadeRoot != null) Destroy(oldFacadeRoot.gameObject);
            facadeRoot = BuildFacade(h, index);
        }

        Transform BuildFacade(HouseDefinition h, int index)
        {
            float z = foot.transform.position.z + h.facadeDistance;
            var root = new GameObject("Facade " + h.displayName).transform;
            root.position = new Vector3(0f, 0f, z);
            float w = h.worldFacadeWidth, roof = h.worldRoofHeight;
            if (!string.IsNullOrEmpty(h.paintedTexture)) return BuildPaintedFacade(h, root, w, roof);

            // Roofless (user, 2026-10-25): the brick runs up past the top of the screen, so no ball is ever "over the roof".
            float wallH = h.hasRoof ? roof : 60f;
            var body = Box(root, new Vector3(0f, wallH * 0.5f, 0.25f), new Vector3(w, wallH, 0.5f), brick);
            body.GetComponent<Renderer>().material.color = BrickTints[index % BrickTints.Length];
            if (h.hasRoof) Box(root, new Vector3(0f, roof + 0.1f, 0.2f), new Vector3(w + 0.3f, 0.2f, 0.7f), frame); // cornice
            float nl = h.hasRoof ? roof + 1.5f : wallH, nr = h.hasRoof ? roof - 1f : wallH;
            Box(root, new Vector3(-(w * 0.5f + 5f), nl * 0.5f, 0.6f), new Vector3(10f, nl, 0.5f), neighbour);
            Box(root, new Vector3(w * 0.5f + 5f, nr * 0.5f, 0.6f), new Vector3(10f, nr, 0.5f), neighbour);
            Box(root, new Vector3(0f, 1.15f, -0.02f), new Vector3(w - 0.4f, 2.3f, 0.04f), shop);               // shop windows
            Box(root, new Vector3(0f, 1.1f, -0.05f), new Vector3(1.2f, 2.2f, 0.04f), metal);                   // door
            Box(root, new Vector3(-w * 0.27f, 2.55f, -0.4f), new Vector3(w * 0.4f, 0.08f, 0.8f), awning);
            Box(root, new Vector3(w * 0.27f, 2.55f, -0.4f), new Vector3(w * 0.4f, 0.08f, 0.8f), awning);

            var rects = h.worldWindows;
            windowRenderers = new Renderer[rects.Length];
            for (int i = 0; i < rects.Length; i++)
            {
                Rect r = rects[i];
                Box(root, new Vector3(r.center.x, r.center.y, -0.02f), new Vector3(r.width + 0.18f, r.height + 0.18f, 0.03f), frame);
                windowRenderers[i] = Box(root, new Vector3(r.center.x, r.center.y, -0.045f), new Vector3(r.width, r.height, 0.02f), glass)
                    .GetComponent<Renderer>();
                Box(root, new Vector3(r.center.x, r.yMin - 0.12f, -0.12f), new Vector3(r.width + 0.3f, 0.07f, 0.22f), frame); // sill
            }
            return root;
        }

        /// <summary>The painting on a quad, plain brick above it to the sky, and an invisible dark pane per window that shows once broken.</summary>
        Transform BuildPaintedFacade(HouseDefinition h, Transform root, float w, float paintedHeight)
        {
            var tex = Resources.Load<Texture2D>(h.paintedTexture);
            if (tex == null)
            {
                Debug.LogError("Painted facade texture not found in Resources: " + h.paintedTexture);
                return root;
            }
            var mat = new Material(Shader.Find("Standard")) { mainTexture = tex, color = Color.white };
            mat.SetFloat("_Glossiness", 0.05f);
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Strip(quad);
            quad.name = "Painting";
            quad.transform.SetParent(root, false);
            quad.transform.localPosition = new Vector3(0f, paintedHeight * 0.5f, 0f);
            quad.transform.localScale = new Vector3(w, paintedHeight, 1f);
            quad.GetComponent<Renderer>().sharedMaterial = mat;

            // The wall keeps going above the painting (no roof): a box in the painting's own brick colour.
            Color top = AverageColour(tex, 0.02f, 0.06f);
            var wallMat = Mat(top);
            float above = 60f - paintedHeight;
            Box(root, new Vector3(0f, paintedHeight + above * 0.5f, 0.3f), new Vector3(w, above, 0.5f), wallMat);
            Box(root, new Vector3(-(w * 0.5f + 5f), 30f, 0.6f), new Vector3(10f, 60f, 0.5f), neighbour);
            Box(root, new Vector3(w * 0.5f + 5f, 30f, 0.6f), new Vector3(10f, 60f, 0.5f), neighbour);

            var rects = h.worldWindows;
            windowRenderers = new Renderer[rects.Length];
            for (int i = 0; i < rects.Length; i++)
            {
                Rect r = rects[i];
                var pane = Box(root, new Vector3(r.center.x, r.center.y, -0.02f), new Vector3(r.width * 0.9f, r.height * 0.9f, 0.02f), broken);
                pane.name = "Broken pane " + i;
                windowRenderers[i] = pane.GetComponent<Renderer>();
                windowRenderers[i].enabled = false; // the painting shows the intact glass
            }
            return root;
        }

        static Color AverageColour(Texture2D tex, float fromV, float toV)
        {
            // Texture must be readable; if it is not, fall back to the placeholder brick tint.
            try
            {
                int y0 = Mathf.Clamp((int)((1f - toV) * tex.height), 0, tex.height - 1), y1 = Mathf.Clamp((int)((1f - fromV) * tex.height), y0 + 1, tex.height);
                var px = tex.GetPixels(0, y0, tex.width, y1 - y0);
                Color sum = Color.black;
                foreach (var c in px) sum += c;
                return sum / Mathf.Max(1, px.Length);
            }
            catch (UnityException)
            {
                return new Color(0.64f, 0.33f, 0.23f);
            }
        }

        void OnWindowBroken(int i, Vector3 point)
        {
            if (i < 0 || i >= windowRenderers.Length) return;
            windowRenderers[i].enabled = true;
            windowRenderers[i].sharedMaterial = broken;
            for (int k = 0; k < 10; k++)
            {
                var shard = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Strip(shard);
                shard.transform.position = point + Random.insideUnitSphere * 0.3f + Vector3.back * 0.1f;
                shard.transform.localScale = new Vector3(0.12f, 0.12f, 0.01f) * Random.Range(0.5f, 1.3f);
                shard.transform.rotation = Random.rotation;
                shard.GetComponent<Renderer>().sharedMaterial = glass;
                var rb = shard.AddComponent<Rigidbody>();
                rb.linearVelocity = new Vector3(Random.Range(-1.5f, 1.5f), Random.Range(0f, 1.5f), Random.Range(-3f, -1f));
                rb.angularVelocity = Random.insideUnitSphere * 10f;
                Destroy(shard, 2.5f);
            }
            Toast("+1  Window smashed!", 1.2f);
        }

        void OnWalkStarted(HouseDefinition from, HouseDefinition to, float duration)
        {
            oldFacadeRoot = facadeRoot;
            facadeRoot = null;
            StartCoroutine(SlideAway(oldFacadeRoot, duration));
            Toast("Walking to " + to.displayName + "...", duration);
        }

        IEnumerator SlideAway(Transform t, float duration)
        {
            if (t == null) yield break;
            Vector3 start = t.position;
            for (float k = 0f; k < 1f && t != null; k += Time.deltaTime / Mathf.Max(0.1f, duration))
            {
                t.position = start + Vector3.left * (14f * k * k * (3f - 2f * k));
                yield return null;
            }
        }

        void OnKicked(KickGesture g, float quality, Vector3 velocity)
        {
            if (quality > 0.85f) Toast("Clean strike!", 0.7f);
        }

        void OnKickRejected(KickRejectReason why)
        {
            switch (why)
            {
                case KickRejectReason.NeedsBounce: Toast("Let it bounce once first", 0.9f); break;
                case KickRejectReason.OutOfReach: Toast("Too far - move to the ball", 0.9f); break;
            }
        }

        void OnLifeLost(int left, LossReason reason)
        {
            string why;
            switch (reason)
            {
                case LossReason.SecondBounce: why = "It bounced twice"; break;
                case LossReason.ShortShot: why = "Too short"; break;
                case LossReason.OverRoof: why = "Over the roof!"; break;
                case LossReason.OutOfBounds: why = "Out into the street"; break;
                case LossReason.BallLostInHouse: why = "The owner kept the ball"; break;
                default: why = "It got past you"; break;
            }
            Toast(why + "  (-1 life)", 1.4f);
        }

        void OnBallInHouse(int window, bool thrownBack)
        {
            Toast(thrownBack ? "Into the flat... the owner throws it back!" : "Into the flat!", 1.3f);
        }

        void OnMedal(int medals, HouseDefinition h) => Toast("MEDAL! " + h.displayName + " has no glass left", 2f);

        void OnBounced(Vector3 p, int bounce, bool returning)
        {
            if (returning && bounce == 1) Toast("NOW!", 0.4f);
        }

        void OnWind(float w) => wind = w;

        void OnGameOver(int score, int best, int medals) { endTitle = "Game over"; endScore = score; endBest = best; endMedals = medals; }
        void OnVictory(int score, int best, int medals) { endTitle = "You smashed the whole street!"; endScore = score; endBest = best; endMedals = medals; }

        void Toast(string text, float seconds)
        {
            toast = text;
            toastUntil = Time.time + seconds;
        }

        // ------------------------------------------------------------------ camera

        void LateUpdate()
        {
            if (!cameraFollowsPlayer || cam == null || foot == null) return;
            var p = cam.transform.position;
            // POV: the eyes are over the feet, so follow the strike spot fully (smoothed).
            float x = Mathf.Lerp(p.x, foot.X, 1f - Mathf.Exp(-10f * Time.unscaledDeltaTime));
            cam.transform.position = new Vector3(x, cameraOffset.y, cameraOffset.z);
        }

        // ------------------------------------------------------------------ HUD (English placeholder; Persian UI is a later step)

        void OnGUI()
        {
            float u = Mathf.Max(1f, Screen.height / 900f);
            var label = new GUIStyle(GUI.skin.label) { fontSize = Mathf.RoundToInt(22 * u), normal = { textColor = Color.white } };
            var big = new GUIStyle(label) { fontSize = Mathf.RoundToInt(40 * u), alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            var button = new GUIStyle(GUI.skin.button) { fontSize = Mathf.RoundToInt(28 * u) };
            float pad = 16f * u;
            var rules = game != null ? game.Rules : null;

            if (rules != null && !showStart && endTitle == null)
            {
                Shadowed(new Rect(pad, pad + SafeTop(), Screen.width, 50 * u), "Glass " + rules.Score, big, TextAnchor.UpperLeft);
                string lives = new string('O', Mathf.Max(0, rules.Lives)) + new string('-', Mathf.Max(0, game.config.lives - rules.Lives));
                Shadowed(new Rect(pad, pad + SafeTop() + 55 * u, Screen.width, 40 * u),
                    "Lives " + lives + "   House " + (rules.HouseIndex + 1) + "/" + rules.HouseCount +
                    "   Windows " + (rules.WindowCount - rules.WindowsLeft) + "/" + rules.WindowCount, label, TextAnchor.UpperLeft);
                Shadowed(new Rect(pad, pad + SafeTop() + 90 * u, Screen.width, 40 * u),
                    "Wind " + (Mathf.Abs(wind) < 0.05f ? "calm" : (wind > 0f ? "-> " : "<- ") + Mathf.Abs(wind).ToString("0.0")) +
                    "   Medals " + rules.Medals + "   " + (router != null && router.UsingMotion ? "[motion]" : "[touch]"), label, TextAnchor.UpperLeft);
                if (Time.time < toastUntil)
                    Shadowed(new Rect(0, Screen.height * 0.42f, Screen.width, 60 * u), toast, big, TextAnchor.MiddleCenter);
                if (rules.Phase == RallyPhase.Serve)
                    Shadowed(new Rect(0, Screen.height * 0.55f, Screen.width, 40 * u),
                        router != null && router.UsingMotion ? "Tap where you want the ball to hit; tilt the phone to move" : "Tap where you want the ball to hit",
                        new GUIStyle(label) { alignment = TextAnchor.MiddleCenter }, TextAnchor.MiddleCenter);
            }

            if (showStart)
            {
                Panel();
                GUILayout.BeginArea(new Rect(pad * 2, Screen.height * 0.2f, Screen.width - pad * 4, Screen.height * 0.7f));
                GUILayout.Label("SHEYTOONAK - prototype", big);
                GUILayout.Label("Placeholder graphics. Tilt or move the phone left/right to reach the ball. " +
                                "Tap the screen to kick: the ball goes where you tap - left/right aims, low on the screen hits the low windows, high hits the top row. " +
                                "A returning ball must bounce exactly once before you kick it.", new GUIStyle(label) { wordWrap = true });
                GUILayout.Space(20 * u);
                if (GUILayout.Button("Play", button, GUILayout.Height(80 * u))) { showStart = false; endTitle = null; touch?.IgnoreFor(0.5f); game.NewGame(); }
                if (motion != null && motion.IsAvailable && router != null && router.kickFromMotion)
                {
                    GUILayout.Space(10 * u);
                    if (GUILayout.Button("Calibrate kick (swing once in 3 s)", button, GUILayout.Height(70 * u)))
                    {
                        calibrationText = "Swing now...";
                        motion.CalibrateKick(3f, ok => calibrationText = ok ? "Calibrated." : "No swing detected - try a bigger swing.");
                    }
                    GUILayout.Label(calibrationText, label);
                }
                else if (motion == null || !motion.IsAvailable) GUILayout.Label("No gyroscope found: finger position moves you.", label);
                GUILayout.Label("Three-finger tap: sensor debug overlay.", label);
                GUILayout.EndArea();
            }
            else if (endTitle != null)
            {
                Panel();
                GUILayout.BeginArea(new Rect(pad * 2, Screen.height * 0.3f, Screen.width - pad * 4, Screen.height * 0.5f));
                GUILayout.Label(endTitle, big);
                GUILayout.Label("Glass smashed: " + endScore + "   Best: " + endBest + "   Medals: " + endMedals, new GUIStyle(label) { alignment = TextAnchor.MiddleCenter });
                GUILayout.Space(20 * u);
                if (GUILayout.Button("Play again", button, GUILayout.Height(80 * u))) { endTitle = null; touch?.IgnoreFor(0.5f); game.NewGame(); }
                GUILayout.EndArea();
            }
        }

        static float SafeTop() => Screen.height - Screen.safeArea.yMax;

        static void Panel()
        {
            var old = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.65f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = old;
        }

        static void Shadowed(Rect r, string text, GUIStyle style, TextAnchor anchor)
        {
            var s = new GUIStyle(style) { alignment = anchor };
            var shadow = new GUIStyle(s) { normal = { textColor = new Color(0f, 0f, 0f, 0.7f) } };
            GUI.Label(new Rect(r.x + 2, r.y + 2, r.width, r.height), text, shadow);
            GUI.Label(r, text, s);
        }

        // ------------------------------------------------------------------ helpers

        static GameObject Box(Transform parent, Vector3 localPos, Vector3 scale, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Strip(go);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            return go;
        }

        /// <summary>Visual-only primitives must not collide: gameplay uses its own invisible colliders.</summary>
        static void Strip(GameObject go)
        {
            var c = go.GetComponent<Collider>();
            if (c != null)
            {
                c.enabled = false; // effective immediately; Destroy only happens at the end of the frame
                Destroy(c);
            }
        }

        static Texture2D FootballTexture()
        {
            const int n = 64;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Repeat };
            var px = new Color32[n * n];
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    // Staggered dark patches: reads as a football while it spins.
                    int cx = (x + ((y / 16) % 2) * 8) % 16, cy = y % 16;
                    bool dark = (cx - 8) * (cx - 8) + (cy - 8) * (cy - 8) < 20;
                    px[y * n + x] = dark ? new Color32(25, 25, 25, 255) : new Color32(245, 245, 245, 255);
                }
            tex.SetPixels32(px);
            tex.Apply();
            return tex;
        }
    }
}
