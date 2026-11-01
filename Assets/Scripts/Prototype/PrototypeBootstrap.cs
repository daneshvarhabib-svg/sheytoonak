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
        // User, 2026-10-25: "خونه‌ها رو درشت نشون نمی‌ده، دورش حاشیه دیده". A vertical FOV on a 20:9 phone becomes
        // a ~110 deg horizontal FOV and shrinks the facade to a quarter of the width, so the FOV is now given
        // horizontally (80 deg) and converted per screen. Geometry for a 13 m painting at 13 m from the strike
        // spot, camera 4 m behind it at eye height: ball at the feet 20 deg below the horizon, top window row
        // ~19 deg above, facade +-21 deg sideways -> with pitch 1 the vertical span is -22..+20.
        public Vector3 cameraOffset = new Vector3(0f, 1.6f, -4.0f);
        public float cameraPitchDeg = 1f;
        [Tooltip("Horizontal field of view in degrees; the vertical one is derived from the screen aspect each frame.")]
        public float cameraHorizontalFov = 80f;
        [Tooltip("Never narrower than this vertically, so a very wide phone still shows the ball at the feet.")]
        public float cameraMinVerticalFov = 42f;

        static readonly Color[] BrickTints =
        {
            new Color(0.64f, 0.33f, 0.23f), new Color(0.71f, 0.42f, 0.27f), new Color(0.58f, 0.30f, 0.24f)
        };

        Camera cam;
        CameraShake shake;
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

        /// <summary>Same as pressing the start button; used by AutoScreenshot in CI.</summary>
        public void StartGameFromScript() { showStart = false; endTitle = null; touch?.IgnoreFor(0.5f); game.NewGame(); }
        public KickInputRouter Router => router;

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

        /// <summary>Material from a Resources texture, or the given flat fallback when the texture is missing.</summary>
        static Material GroundMat(string resource, Material fallback, float tileMetres)
        {
            var tex = Resources.Load<Texture2D>(resource);
            if (tex == null) return fallback;
            tex.wrapMode = TextureWrapMode.Repeat;
            var m = new Material(Shader.Find("Standard")) { mainTexture = tex, color = Color.white };
            m.SetFloat("_Glossiness", 0.05f);
            return m;
        }

        /// <summary>A cube face maps the whole texture once, so a box sized x by z metres needs x/tile by z/tile repeats.</summary>
        static Material Tiled(Material m, float sizeX, float sizeZ, float tileMetres)
        {
            if (m.mainTexture == null) return m;
            var copy = new Material(m) { mainTextureScale = new Vector2(sizeX / tileMetres, sizeZ / tileMetres) };
            return copy;
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
            ApplyFov();
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 250f;
            camGo.transform.SetPositionAndRotation(cameraOffset, Quaternion.Euler(cameraPitchDeg, 0f, 0f));
            camGo.AddComponent<AudioListener>();
            shake = camGo.AddComponent<CameraShake>();

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
            // Painted ground (ChatGPT, 2026-10-27): tiled every 2 m; falls back to the flat colours without the textures.
            var paveMat = GroundMat("Art/ground_sidewalk", sidewalk, 2f);
            var roadMat = GroundMat("Art/ground_asphalt", asphalt, 2.5f);
            Box(root, new Vector3(0f, -0.05f, 0f), new Vector3(80f, 0.1f, 6f), Tiled(paveMat, 80f, 6f, 2f));   // near sidewalk, z -3..3
            Box(root, new Vector3(0f, 0.04f, 3.0f), new Vector3(80f, 0.16f, 0.25f), frame);          // curb
            Box(root, new Vector3(0f, -0.06f, 18f), new Vector3(80f, 0.1f, 30f), Tiled(roadMat, 80f, 30f, 2.5f)); // street and beyond
            for (int i = -6; i <= 6; i++)
                Box(root, new Vector3(i * 4f, -0.005f, 8.0f), new Vector3(2f, 0.01f, 0.15f), shoe);   // centre line
            Box(root, new Vector3(0f, -0.04f, 12f), new Vector3(80f, 0.1f, 2f), Tiled(paveMat, 80f, 2f, 2f));  // far sidewalk
            Box(root, new Vector3(-3.4f, 3f, 2.4f), new Vector3(0.12f, 6f, 0.12f), metal);           // lamp post
            Box(root, new Vector3(-2.9f, 5.95f, 2.4f), new Vector3(1.0f, 0.1f, 0.12f), metal);
            for (int i = 0; i < 2; i++)
                Box(root, new Vector3(2.6f + i * 1.6f, 0.45f, 2.6f), new Vector3(0.1f, 0.9f, 0.1f), metal); // bollards
        }

        void BuildGameObjects()
        {
            var config = ScriptableObject.CreateInstance<GameConfig>();
            var houses = new[] { House1(), House2(), House3(), House4(), House5() };

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
            // No sideways movement any more (user, 2026-10-27), so no motion sensors: the ball always
            // comes back to the player's line and every control is a tap.
            motion = null;
            touch = inputGo.AddComponent<TouchKickInput>();
            router = inputGo.AddComponent<KickInputRouter>();
            router.motion = null;
            router.useMotion = false;
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
            gameGo.AddComponent<CurseVoice>();
            gameGo.AddComponent<StreetLife>();
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
        HouseDefinition House1() => MakePaintedHouse("البيت ١", "Art/house_01", 1536f, 1024f, 13f,
            new[] { new Rect(268f, 40f, 224f, 205f), new Rect(662f, 40f, 218f, 205f), new Rect(1055f, 40f, 220f, 205f),
                    new Rect(268f, 350f, 224f, 205f), new Rect(662f, 350f, 218f, 205f), new Rect(1055f, 350f, 220f, 205f) },
            0f, 2.1f, 0f, 1.15f,
            Details(curtains: 6, pots: new[] { 0, 2, 3, 4, 5 }));
        HouseDefinition House2() => MakePaintedHouse("البيت ٢", "Art/house_02", 1536f, 1024f, 13f,
            Grid(new[] { (268f, 490f), (660f, 880f), (1055f, 1275f) }, new[] { (25f, 195f), (265f, 440f), (510f, 675f) }),
            0.25f, 1.95f, 0f, 1.1f,
            Details(curtains: 9, pots: new[] { 0, 1, 2, 4, 5, 6 }, laundry: new[] { 3 }, cats: new[] { 8 }));
        HouseDefinition House3() => MakePaintedHouse("البيت ٣", "Art/house_03", 1536f, 1024f, 13f,
            Grid(new[] { (215f, 400f), (540f, 720f), (860f, 1045f), (1185f, 1365f) }, new[] { (30f, 190f), (265f, 430f), (500f, 640f) }),
            0.5f, 1.8f, 0f, 1.0f,
            Details(curtains: 12, pots: new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }));

        HouseDefinition House4() => MakePaintedHouse("البيت ٤", "Art/house_04", 1536f, 1024f, 13f,
            Grid(new[] { (318f, 495f), (685f, 860f), (1045f, 1225f) }, new[] { (25f, 185f), (235f, 395f), (435f, 595f), (630f, 760f) }),
            0.6f, 1.7f, 0f, 0.95f,
            Details(curtains: 12, pots: new[] { 0, 1, 2, 3, 4, 5, 7, 9, 11 }));

        // Beirut pharmacy (ChatGPT, 2026-10-28): shutters beside each window; only the glass between them is hittable.
        HouseDefinition House5() => MakePaintedHouse("البيت ٥", "Art/house_05", 1536f, 1024f, 13f,
            Grid(new[] { (300f, 395f), (555f, 650f), (810f, 905f), (1065f, 1160f) }, new[] { (45f, 195f), (255f, 405f), (455f, 610f), (650f, 805f) }),
            0.7f, 1.6f, 0f, 1.0f,
            Details(curtains: 16));

        static WindowDetail[] Details(int curtains, int[] pots = null, int[] laundry = null, int[] cats = null)
        {
            var list = new System.Collections.Generic.List<WindowDetail>();
            for (int i = 0; i < curtains; i++) list.Add(new WindowDetail(i, DetailKind.Curtain));
            foreach (int i in pots ?? new int[0]) list.Add(new WindowDetail(i, DetailKind.Pot));
            foreach (int i in laundry ?? new int[0]) list.Add(new WindowDetail(i, DetailKind.Laundry));
            foreach (int i in cats ?? new int[0]) list.Add(new WindowDetail(i, DetailKind.Cat));
            return list.ToArray();
        }

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
                                                float wind, float returnTime, float spread, float hitScale, WindowDetail[] details = null)
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
            // ⚠️ 2026-10-28: the automatic cut-outs (a copy of the window region as "curtain", its lower part as
            // "pot") looked like the whole window popping out with another window behind it. Details now only
            // exist when a real pixel rect of a real object is given; the defaults are gone.
            house.details = System.Array.FindAll(details ?? System.Array.Empty<WindowDetail>(), d => d.pixel.width > 0f && d.pixel.height > 0f);
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
            // Neighbours: painted blank walls (Art/wall_left, Art/wall_right: houses without visible windows,
            // user 2026-10-27) when they exist, otherwise a darker shade of this painting's brick.
            var sideMat = Mat(top * 0.88f);
            SideWall(root, "Art/wall_left", -1f, w, paintedHeight, sideMat);
            SideWall(root, "Art/wall_right", 1f, w, paintedHeight, sideMat);

            var rects = h.worldWindows;
            windowRenderers = new Renderer[rects.Length];
            for (int i = 0; i < rects.Length; i++)
            {
                Rect r = rects[i];
                var pane = Box(root, new Vector3(r.center.x, r.center.y, -0.02f), new Vector3(r.width * 0.9f, r.height * 0.9f, 0.02f), BrokenGlassMat());
                pane.name = "Broken pane " + i;
                windowRenderers[i] = pane.GetComponent<Renderer>();
                windowRenderers[i].enabled = false; // the painting shows the intact glass
            }

            // Moving details cut out of the painting itself (user, 2026-10-27: curtains flutter, pots and cats
            // fall, laundry sways). Laundry is alive from the start; the rest wake up when the window breaks.
            paintedTex = tex; paintedW = w; paintedH = paintedHeight; detailRoot = root;
            foreach (var d in h.details)
                if (d.kind == DetailKind.Laundry) Laundry(d);
            return root;
        }

        Texture2D paintedTex; float paintedW, paintedH; Transform detailRoot;

        Rect DetailPixels(WindowDetail d)
        {
            if (d.pixel.width > 0f && d.pixel.height > 0f) return d.pixel;
            // Default regions, in image pixels from the window's world rect.
            Rect w = game.houses[Mathf.Clamp(game.Rules != null ? game.Rules.HouseIndex : 0, 0, game.houses.Length - 1)].worldWindows[d.window];
            float px0 = (w.xMin / paintedW + 0.5f) * paintedTex.width, px1 = (w.xMax / paintedW + 0.5f) * paintedTex.width;
            float py0 = (1f - w.yMax / paintedH) * paintedTex.height, py1 = (1f - w.yMin / paintedH) * paintedTex.height;
            float pw = px1 - px0, ph = py1 - py0;
            switch (d.kind)
            {
                case DetailKind.Pot: return new Rect(px0 + pw * 0.15f, py1 - ph * 0.38f, pw * 0.7f, ph * 0.38f);
                case DetailKind.Cat: return new Rect(px0 + pw * 0.5f, py1 - ph * 0.5f, pw * 0.5f, ph * 0.5f);
                default: return new Rect(px0, py0, pw, ph);
            }
        }

        /// <summary>A sprite cut from the painting at the given pixel rect, placed exactly over the same spot.</summary>
        SpriteRenderer Cutout(WindowDetail d, float zOffset, out Rect pix)
        {
            pix = DetailPixels(d);
            int x = Mathf.Clamp(Mathf.RoundToInt(pix.xMin), 0, paintedTex.width - 2), y = Mathf.Clamp(Mathf.RoundToInt(paintedTex.height - pix.yMax), 0, paintedTex.height - 2);
            int cw = Mathf.Clamp(Mathf.RoundToInt(pix.width), 1, paintedTex.width - x), ch = Mathf.Clamp(Mathf.RoundToInt(pix.height), 1, paintedTex.height - y);
            var tex = new Texture2D(cw, ch, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            tex.SetPixels(paintedTex.GetPixels(x, y, cw, ch));
            tex.Apply();
            float ppu = paintedTex.width / paintedW;
            var go = new GameObject(d.kind + " " + d.window);
            go.transform.SetParent(detailRoot, false);
            float wx = (pix.center.x / paintedTex.width - 0.5f) * paintedW, wy = (1f - pix.center.y / paintedTex.height) * paintedH;
            go.transform.localPosition = new Vector3(wx, wy, zOffset);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, cw, ch), new Vector2(0.5f, 0.5f), ppu);
            return sr;
        }

        void Laundry(WindowDetail d)
        {
            var sr = Cutout(d, -0.03f, out _);
            StartCoroutine(Sway(sr.transform));
        }

        IEnumerator Sway(Transform t)
        {
            float phase = Random.value * 10f;
            while (t != null)
            {
                float a = (0.6f + Mathf.Abs(wind) * 4f) * Mathf.Sin(Time.time * 1.6f + phase) + 0.25f * Mathf.Sin(Time.time * 4.1f + phase);
                t.localRotation = Quaternion.Euler(0f, 0f, a);
                t.localScale = new Vector3(1f + 0.03f * Mathf.Sin(Time.time * 2.3f + phase), 1f, 1f);
                yield return null;
            }
        }

        void WakeDetails(int window)
        {
            if (paintedTex == null || detailRoot == null || game == null) return;
            var house = game.houses[Mathf.Clamp(game.Rules.HouseIndex, 0, game.houses.Length - 1)];
            foreach (var d in house.details)
            {
                if (d.window != window) continue;
                switch (d.kind)
                {
                    case DetailKind.Curtain:
                    {
                        var sr = Cutout(d, -0.04f, out _);
                        StartCoroutine(Flutter(sr.transform));
                        break;
                    }
                    case DetailKind.Pot:
                    case DetailKind.Cat:
                    {
                        var sr = Cutout(d, -0.06f, out Rect pix);
                        var rb = sr.gameObject.AddComponent<Rigidbody>();
                        rb.mass = 0.5f;
                        rb.linearVelocity = new Vector3(Random.Range(-0.4f, 0.4f), d.kind == DetailKind.Cat ? 1.5f : 0.3f, Random.Range(-2.5f, -1.2f));
                        rb.angularVelocity = new Vector3(0f, 0f, Random.Range(-3f, 3f));
                        var col = sr.gameObject.AddComponent<SphereCollider>();
                        col.radius = 0.12f;
                        Destroy(sr.gameObject, 3.5f);
                        break;
                    }
                }
            }
        }

        /// <summary>The curtain behind a smashed pane billows for a couple of seconds, then hangs still (never torn off).</summary>
        IEnumerator Flutter(Transform t)
        {
            float start = Time.time;
            while (t != null && Time.time - start < 2.5f)
            {
                float k = 1f - (Time.time - start) / 2.5f;
                float a = Mathf.Sin((Time.time - start) * 11f) * 6f * k;
                t.localRotation = Quaternion.Euler(0f, 0f, a);
                t.localScale = new Vector3(1f + 0.12f * k * Mathf.Sin((Time.time - start) * 7f), 1f, 1f);
                yield return null;
            }
            if (t != null) { t.localRotation = Quaternion.identity; t.localScale = Vector3.one; }
        }

        void SideWall(Transform root, string resource, float side, float w, float paintedHeight, Material fallback)
        {
            var tex = Resources.Load<Texture2D>(resource);
            if (tex == null)
            {
                Box(root, new Vector3(side * (w * 0.5f + 10f), 30f, 0.6f), new Vector3(20f, 60f, 0.5f), fallback);
                return;
            }
            float ww = paintedHeight * tex.width / tex.height; // same vertical scale as the facade painting
            var mat = new Material(Shader.Find("Standard")) { mainTexture = tex, color = Color.white };
            mat.SetFloat("_Glossiness", 0.05f);
            for (int k = 0; k < 2; k++) // two copies outward, enough to fill the widest phone
            {
                var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                Strip(quad);
                quad.transform.SetParent(root, false);
                quad.transform.localPosition = new Vector3(side * (w * 0.5f + ww * (0.5f + k)), paintedHeight * 0.5f, 0.05f);
                quad.transform.localScale = new Vector3(ww, paintedHeight, 1f);
                quad.GetComponent<Renderer>().sharedMaterial = mat;
            }
            var above = Mat(AverageColour(tex, 0.02f, 0.06f));
            Box(root, new Vector3(side * (w * 0.5f + ww), paintedHeight + (60f - paintedHeight) * 0.5f, 0.3f), new Vector3(ww * 2f, 60f - paintedHeight, 0.5f), above);
        }

        Material brokenGlassMat;

        /// <summary>Dark interior with a web of pale cracks and a jagged hole: what a smashed pane looks like from the street.</summary>
        Material BrokenGlassMat()
        {
            if (brokenGlassMat != null) return brokenGlassMat;
            const int n = 256;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            var px = new Color32[n * n];
            var dark = new Color32(18, 16, 20, 255);
            var crack = new Color32(215, 225, 235, 255);
            var rng = new System.Random(9);
            for (int i = 0; i < px.Length; i++) px[i] = dark;
            float cx = n * 0.5f + (float)(rng.NextDouble() - 0.5) * 60f, cy = n * 0.5f + (float)(rng.NextDouble() - 0.5) * 60f;
            // Radial cracks with a wobble, plus a few concentric rings.
            for (int k = 0; k < 11; k++)
            {
                float ang = k * Mathf.PI * 2f / 11f + (float)rng.NextDouble() * 0.4f;
                float len = n * (0.45f + (float)rng.NextDouble() * 0.4f);
                for (float d = 0; d < len; d += 0.7f)
                {
                    float wob = Mathf.Sin(d * 0.08f + k) * 3f;
                    int x = Mathf.RoundToInt(cx + Mathf.Cos(ang) * d - Mathf.Sin(ang) * wob), y = Mathf.RoundToInt(cy + Mathf.Sin(ang) * d + Mathf.Cos(ang) * wob);
                    if (x >= 0 && x < n && y >= 0 && y < n) px[y * n + x] = crack;
                }
            }
            for (int ring = 1; ring <= 3; ring++)
            {
                float rr = 22f * ring + (float)rng.NextDouble() * 10f;
                for (float a = 0; a < Mathf.PI * 2f; a += 0.01f)
                {
                    if (rng.NextDouble() < 0.25) continue; // broken rings
                    int x = Mathf.RoundToInt(cx + Mathf.Cos(a) * rr), y = Mathf.RoundToInt(cy + Mathf.Sin(a) * rr);
                    if (x >= 0 && x < n && y >= 0 && y < n) px[y * n + x] = crack;
                }
            }
            tex.SetPixels32(px);
            tex.Apply();
            brokenGlassMat = new Material(Shader.Find("Standard")) { mainTexture = tex, color = Color.white };
            brokenGlassMat.SetFloat("_Glossiness", 0.6f);
            return brokenGlassMat;
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
            windowRenderers[i].enabled = true; // the pane shows the dark room with a web of cracks (its material is set at build time)
            WakeDetails(i);
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
            Toast(Ar.Smashed, 1.2f);
        }

        void OnWalkStarted(HouseDefinition from, HouseDefinition to, float duration)
        {
            oldFacadeRoot = facadeRoot;
            facadeRoot = null;
            StartCoroutine(SlideAway(oldFacadeRoot, duration));
            Toast(Ar.Walking(to.displayName), duration);
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
            if (quality > 0.85f) Toast("ضربة نضيفة!", 0.7f);
        }

        void OnKickRejected(KickRejectReason why)
        {
            switch (why)
            {
                case KickRejectReason.NeedsBounce: Toast(Ar.BounceFirst, 0.9f); break;
                case KickRejectReason.OutOfReach: Toast(Ar.TooFar, 0.9f); break;
            }
        }

        void OnLifeLost(int left, LossReason reason)
        {
            string why;
            switch (reason)
            {
                case LossReason.SecondBounce: why = Ar.LostTwice; break;
                case LossReason.ShortShot: why = Ar.LostShort; break;
                case LossReason.OutOfBounds: why = Ar.LostStreet; break;
                case LossReason.BallLostInHouse: why = Ar.LostKept; break;
                default: why = Ar.LostPast; break;
            }
            Toast(why + "  " + Ar.LifeLost, 1.4f);
        }

        void OnBallInHouse(int window, bool thrownBack)
        {
            Toast(thrownBack ? Ar.IntoFlatBack : Ar.IntoFlat, 1.3f);
        }

        void OnMedal(int medals, HouseDefinition h) => Toast(Ar.Medal(h.displayName), 2f);

        void OnBounced(Vector3 p, int bounce, bool returning)
        {
            if (returning && bounce == 1) Toast(Ar.Now, 0.4f);
        }

        void OnWind(float w) => wind = w;

        void OnGameOver(int score, int best, int medals) { endTitle = Ar.GameOver; endScore = score; endBest = best; endMedals = medals; }
        void OnVictory(int score, int best, int medals) { endTitle = Ar.Victory; endScore = score; endBest = best; endMedals = medals; }

        void Toast(string text, float seconds)
        {
            toast = text;
            toastUntil = Time.time + seconds;
        }

        // ------------------------------------------------------------------ camera

        void ApplyFov()
        {
            if (cam == null) return;
            float v = Camera.HorizontalToVerticalFieldOfView(cameraHorizontalFov, Mathf.Max(0.5f, cam.aspect));
            cam.fieldOfView = Mathf.Max(cameraMinVerticalFov, v);
        }

        void LateUpdate()
        {
            ApplyFov(); // the aspect is only known once the screen exists, and changes with rotation
            if (!cameraFollowsPlayer || cam == null || foot == null) return;
            var p = cam.transform.position;
            // POV: the eyes are over the feet, so follow the strike spot fully (smoothed).
            float x = Mathf.Lerp(p.x, foot.X, 1f - Mathf.Exp(-10f * Time.unscaledDeltaTime));
            cam.transform.position = new Vector3(x, cameraOffset.y, cameraOffset.z) + (shake != null ? shake.Offset : Vector3.zero);
        }

        // ------------------------------------------------------------------ HUD (spoken Levantine Arabic, see Ar.cs)

        void OnGUI()
        {
            float u = Mathf.Max(1f, Screen.height / 900f);
            var font = Ar.Font;
            var label = new GUIStyle(GUI.skin.label) { fontSize = Mathf.RoundToInt(24 * u), normal = { textColor = Color.white }, font = font };
            var big = new GUIStyle(label) { fontSize = Mathf.RoundToInt(42 * u), alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            var button = new GUIStyle(GUI.skin.button) { fontSize = Mathf.RoundToInt(30 * u), font = font };
            float pad = 16f * u;
            var rules = game != null ? game.Rules : null;
            var right = new Rect(pad, 0f, Screen.width - pad * 2f, 0f);

            if (rules != null && !showStart && endTitle == null)
            {
                Shadowed(new Rect(right.x, pad + SafeTop(), right.width, 52 * u), Ar.T(Ar.Hud1(rules.Score)), big, TextAnchor.UpperRight);
                Shadowed(new Rect(right.x, pad + SafeTop() + 58 * u, right.width, 40 * u),
                    Ar.T(Ar.Hud2(rules.Lives, game.config.lives, rules.HouseIndex + 1, rules.HouseCount, rules.WindowCount - rules.WindowsLeft, rules.WindowCount)), label, TextAnchor.UpperRight);
                Shadowed(new Rect(right.x, pad + SafeTop() + 94 * u, right.width, 40 * u), Ar.T(Ar.Hud3(wind, rules.Medals)), label, TextAnchor.UpperRight);
                if (Time.time < toastUntil)
                    Shadowed(new Rect(0, Screen.height * 0.42f, Screen.width, 60 * u), Ar.T(toast), big, TextAnchor.MiddleCenter);
                if (rules.Phase == RallyPhase.Serve)
                    Shadowed(new Rect(0, Screen.height * 0.55f, Screen.width, 40 * u), Ar.T(Ar.ServeHint),
                        new GUIStyle(label) { alignment = TextAnchor.MiddleCenter }, TextAnchor.MiddleCenter);
            }

            if (showStart)
            {
                Panel();
                GUILayout.BeginArea(new Rect(pad * 2, Screen.height * 0.12f, Screen.width - pad * 4, Screen.height * 0.8f));
                GUILayout.Label(Ar.T(Ar.Title), big);
                // IMGUI wraps left-to-right; a wrapped Arabic paragraph would break in the wrong places, so it is split by sentence.
                foreach (var line in Ar.Intro.Split('.'))
                    if (line.Trim().Length > 0) GUILayout.Label(Ar.T(line.Trim() + "."), new GUIStyle(label) { alignment = TextAnchor.MiddleRight });
                GUILayout.Space(20 * u);
                if (GUILayout.Button(Ar.T(Ar.Play), button, GUILayout.Height(80 * u))) { showStart = false; endTitle = null; touch?.IgnoreFor(0.5f); game.NewGame(); }
                GUILayout.EndArea();
            }
            else if (endTitle != null)
            {
                Panel();
                GUILayout.BeginArea(new Rect(pad * 2, Screen.height * 0.3f, Screen.width - pad * 4, Screen.height * 0.5f));
                GUILayout.Label(Ar.T(endTitle), big);
                GUILayout.Label(Ar.T(Ar.End(endScore, endBest, endMedals)), new GUIStyle(label) { alignment = TextAnchor.MiddleCenter });
                GUILayout.Space(20 * u);
                if (GUILayout.Button(Ar.T(Ar.PlayAgain), button, GUILayout.Height(80 * u))) { endTitle = null; touch?.IgnoreFor(0.5f); game.NewGame(); }
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

        /// <summary>The classic black-and-white panel ball ("توپ چهل‌تیکه"; user 2026-10-28: the striped plastic
        /// one read as too weak to break glass). Dark pentagon-ish patches on an off-white sphere map, slightly scuffed.</summary>
        static Texture2D FootballTexture()
        {
            const int n = 256, m = 128;
            var tex = new Texture2D(n, m, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Repeat };
            var px = new Color32[n * m];
            var white = new Color32(236, 232, 224, 255);
            var black = new Color32(28, 28, 30, 255);
            var seam = new Color32(170, 165, 155, 255);
            var rng = new System.Random(5);
            var centres = new System.Collections.Generic.List<Vector2>();
            for (int row = 0; row < 4; row++)
                for (int col = 0; col < 6; col++)
                    centres.Add(new Vector2((col + (row % 2) * 0.5f) / 6f * n, (row + 0.5f) / 4f * m));
            for (int y = 0; y < m; y++)
                for (int x = 0; x < n; x++)
                {
                    Color32 c = white;
                    float best = float.MaxValue;
                    foreach (var cc in centres)
                    {
                        float dx = Mathf.Abs(x - cc.x); dx = Mathf.Min(dx, n - dx);
                        float dy = Mathf.Abs(y - cc.y);
                        float d = Mathf.Max(Mathf.Sqrt(dx * dx + dy * dy), (dx + dy * 1.1f) * 0.78f);
                        best = Mathf.Min(best, d);
                    }
                    if (best < 11f) c = black;
                    else if (best < 12.5f) c = seam;
                    if (rng.NextDouble() < 0.02) c = new Color32((byte)(c.r * 0.85f), (byte)(c.g * 0.85f), (byte)(c.b * 0.85f), 255);
                    px[y * n + x] = c;
                }
            tex.SetPixels32(px);
            tex.Apply();
            return tex;
        }
    }
}
