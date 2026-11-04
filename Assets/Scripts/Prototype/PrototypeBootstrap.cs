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
        [Tooltip("Duel mode (docs/SCENARIO.fa.md): shared stage seen from behind, three heroes a side, turns. Off = the old rally.")]
        public bool duel = true;
        // Seen from behind with the heroes in frame: higher, further back, a little pitched down, wider vertically.
        public Vector3 duelCameraOffset = new Vector3(0f, 3.0f, -6.0f);
        public float duelCameraPitchDeg = 5f;
        public float duelMinVerticalFov = 48f;
        public float duelHorizontalFov = 64f;
        DuelManager duelGame;
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
        int lootShards, lootCards; float lootUntil; // the last opened bag, shown over the home screen
        float toastUntil;
        string endTitle;
        int endScore, endBest, endMedals;
        bool showStart = true;
        string calibrationText = "";

        /// <summary>Same as pressing the start button; used by AutoScreenshot in CI.</summary>
        public void StartChallengeFromScript() { showStart = false; endTitle = null; duelGame?.PlayChallenge(); }
        public void GoHomeFromScript() { duelGame?.ToHome(); showStart = true; }
        public void StartGameFromScript() { showStart = false; endTitle = null; touch?.IgnoreFor(0.5f); if (duel) duelGame.NewDuel(); else game.NewGame(); }
        public DuelManager Duel => duelGame;
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
            if (duel) { cameraOffset = duelCameraOffset; cameraPitchDeg = duelCameraPitchDeg; cameraMinVerticalFov = duelMinVerticalFov; cameraHorizontalFov = duelHorizontalFov; }
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
            if (!duel)
            {
                Box(root, new Vector3(-3.4f, 3f, 2.4f), new Vector3(0.12f, 6f, 0.12f), metal);           // lamp post
                Box(root, new Vector3(-2.9f, 5.95f, 2.4f), new Vector3(1.0f, 0.1f, 0.12f), metal);
                for (int i = 0; i < 2; i++)
                    Box(root, new Vector3(2.6f + i * 1.6f, 0.45f, 2.6f), new Vector3(0.1f, 0.9f, 0.1f), metal); // bollards
            }
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
            if (duel)
            {
                duelGame = gameGo.AddComponent<DuelManager>();
                duelGame.config = config;
                duelGame.house = houses[1]; // 3x3 windows: a clear 1 / 2 split
                duelGame.ball = ballCtl;
                duelGame.foot = foot;
                duelGame.mapper = mapper;
                duelGame.inputSource = router;
                var stage = gameGo.AddComponent<DuelStage>();
                stage.duel = duelGame; stage.mapper = mapper; stage.foot = foot;
                stage.leftMat = Mat(new Color(0.20f, 0.45f, 0.85f));
                stage.rightMat = Mat(new Color(0.85f, 0.30f, 0.25f));
                stage.propMat = Mat(new Color(0.35f, 0.38f, 0.42f));
                stage.shutterMat = Mat(new Color(0.45f, 0.30f, 0.15f));
                stage.puddleMat = Mat(new Color(0.25f, 0.4f, 0.6f), 0.8f);
                var hud = gameGo.AddComponent<DuelHud>();
                hud.duel = duelGame; hud.stage = stage; hud.mapper = mapper; hud.cam = cam; hud.touch = touch;
                hud.onPlayAgain = () => { if (duelGame.InChallenge) duelGame.PlayChallenge(); else duelGame.NewDuel(); };
                hud.onHome = () => { duelGame.ToHome(); showStart = true; };
                var draft = gameGo.AddComponent<DraftScreen>();
                draft.duel = duelGame; draft.touch = touch;
                duelGame.BagOpened += (slot, shards, cards) => { lootShards = shards; lootCards = cards; lootUntil = Time.time + 2.8f; };
                duelGame.Toast += (t, sec) => { }; // the hud shows toasts; the bootstrap keeps its own for the rally only
            }
            else
            {
                game = gameGo.AddComponent<GameManager>();
                game.config = config;
                game.houses = houses;
                game.ball = ballCtl;
                game.foot = foot;
                game.mapper = mapper;
                game.inputSource = router;
                game.autoStart = false;
            }
            gameGo.AddComponent<ProceduralSfx>();
            gameGo.AddComponent<CurseVoice>();
            gameGo.AddComponent<StreetLife>();
            if (!duel)
            {
                var overlay = gameGo.AddComponent<SensorDebugOverlay>();
                overlay.motion = motion;
                overlay.game = game;
            }
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
                var pane = Box(root, new Vector3(r.center.x, r.center.y, -0.02f), new Vector3(r.width * 0.9f, r.height * 0.9f, 0.02f), BrokenGlassMat(i, r.width / Mathf.Max(0.01f, r.height)));
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

        readonly System.Collections.Generic.Dictionary<int, Material> brokenMats = new System.Collections.Generic.Dictionary<int, Material>();
        const int BrokenVariants = 4;

        /// <summary>
        /// What a smashed pane looks like from the street. User (2026-11-02): «شیشه‌ها بد می‌شکنن … باید چند مدل
        /// پنجره شکسته هم داشته باشیم». Painted panes (Art/Broken/pane_0..3) win when they exist; otherwise four
        /// procedural ones: the hole sits in a different place each time, glass is left hanging round the frame.
        /// </summary>
        // painted panes are tall (a door-like portrait) and the windows are wide: stretching made a squashed room and
        // turned the painted curtain of pane_3 into a brown "boat" (user 2026-11-04: افتادن گلدون و پرده … طبیعی نیست).
        // So the painting is cropped to the window's shape around its hole, and pane_3 (curtain painted in) is not used:
        // the curtain is the moving sprite.
        static readonly float[] PaneHoleV = { 0.5f, 0.24f, 0.5f };
        Material BrokenGlassMat(int window, float aspect)
        {
            var painted0 = Resources.Load<Texture2D>("Art/Broken/pane_0");
            int variants = painted0 != null ? PaneHoleV.Length : BrokenVariants;
            int v = (window * 7 + 3) % variants;
            int key = v * 100 + Mathf.RoundToInt(aspect * 10f);
            if (brokenMats.TryGetValue(key, out var m) && m != null) return m;
            var painted = Resources.Load<Texture2D>("Art/Broken/pane_" + v);
            var tex = painted != null ? painted : ProceduralPane(v);
            m = new Material(Shader.Find("Standard")) { mainTexture = tex, color = Color.white };
            m.SetFloat("_Glossiness", 0.55f);
            if (painted != null)
            {
                float ta = tex.width / (float)tex.height;
                if (aspect > ta)
                {
                    float sy = ta / aspect;
                    m.mainTextureScale = new Vector2(1f, sy);
                    m.mainTextureOffset = new Vector2(0f, Mathf.Clamp(PaneHoleV[v] - sy * 0.5f, 0f, 1f - sy));
                }
                else
                {
                    float sx = aspect / ta;
                    m.mainTextureScale = new Vector2(sx, 1f);
                    m.mainTextureOffset = new Vector2((1f - sx) * 0.5f, 0f);
                }
            }
            brokenMats[key] = m;
            return m;
        }

        static Texture2D ProceduralPane(int variant)
        {
            const int n = 256;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            var px = new Color32[n * n];
            var rng = new System.Random(17 + variant * 31);
            // hole centre: middle / lower-left / upper-right / almost everything gone
            Vector2[] centres = { new Vector2(0.5f, 0.5f), new Vector2(0.32f, 0.3f), new Vector2(0.7f, 0.68f), new Vector2(0.5f, 0.45f) };
            float[] radii = { 0.38f, 0.4f, 0.36f, 0.55f }; // run 48: smaller holes read as a black splat on intact glass
            Vector2 c = centres[variant] * n; float rad = radii[variant] * n;
            // jagged hole outline: radius per angle with spikes
            int spokes = 9 + variant * 2;
            var spike = new float[spokes];
            for (int k = 0; k < spokes; k++) spike[k] = 0.55f + (float)rng.NextDouble() * 0.75f;
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float dx = x - c.x, dy = y - c.y, d = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = (Mathf.Atan2(dy, dx) / (Mathf.PI * 2f) + 1f) % 1f * spokes;
                    int k0 = (int)a % spokes, k1 = (k0 + 1) % spokes; float f = a - (int)a;
                    float edge = rad * Mathf.Lerp(spike[k0], spike[k1], f);
                    bool hole = d < edge;
                    // dark room in the hole, a faint warm lamp glow; remaining glass is a cool reflective grey-blue
                    float shade = 1f - Mathf.Clamp01(d / (rad * 1.6f));
                    px[y * n + x] = hole
                        ? new Color32((byte)(18 + 22 * shade), (byte)(15 + 14 * shade), (byte)(14 + 8 * shade), 255)
                        : new Color32((byte)(72 + y / 9), (byte)(88 + y / 9), (byte)(104 + y / 10), 255); // left-over glass: darker than the painted intact pane
                }
            // cracks running from the hole out to the frame
            var crack = new Color32(232, 238, 245, 255);
            for (int k = 0; k < spokes + 3; k++)
            {
                float ang = (float)rng.NextDouble() * Mathf.PI * 2f, wob = (float)rng.NextDouble() * 6f;
                for (float d = rad * 0.6f; d < n; d += 0.7f)
                {
                    float w = Mathf.Sin(d * 0.07f + k) * wob;
                    int x = Mathf.RoundToInt(c.x + Mathf.Cos(ang) * d - Mathf.Sin(ang) * w), y = Mathf.RoundToInt(c.y + Mathf.Sin(ang) * d + Mathf.Cos(ang) * w);
                    if (x < 0 || x >= n || y < 0 || y >= n) break;
                    if (px[y * n + x].r > 60) px[y * n + x] = crack;
                }
            }
            tex.SetPixels32(px);
            tex.Apply();
            return tex;
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
            if (game != null) WakeDetails(i);
            else WindowLife(windowRenderers[i].transform.position, windowRenderers[i].transform.lossyScale);
            for (int k = 0; k < 18; k++)
            {
                var shard = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Strip(shard);
                shard.transform.position = point + Random.insideUnitSphere * 0.3f + Vector3.back * 0.1f;
                shard.transform.localScale = new Vector3(Random.Range(0.05f, 0.18f), Random.Range(0.05f, 0.22f), 0.01f);
                shard.transform.rotation = Random.rotation;
                shard.GetComponent<Renderer>().sharedMaterial = glass;
                var rb = shard.AddComponent<Rigidbody>();
                rb.linearVelocity = new Vector3(Random.Range(-1.5f, 1.5f), Random.Range(0f, 1.5f), Random.Range(-3f, -1f));
                rb.angularVelocity = Random.insideUnitSphere * 10f;
                Destroy(shard, 2.5f);
            }
            if (!duel) Toast(Ar.Smashed, 1.2f); // the duel shows the value on the window, no sentence
        }

        /// <summary>
        /// Something happens behind every smashed pane (user, 2026-11-02: «گربه بیفته، پرده تکون بخوره و از این
        /// چیزها»): a cat leaps out, a curtain billows out through the hole, or a flower pot topples off the sill.
        /// Painted sprites Art/Fx/fx_cat, fx_curtain, fx_pot are used when present, primitives otherwise.
        /// </summary>
        void WindowLife(Vector3 at, Vector3 size)
        {
            float roll = Random.value;
            if (roll < 0.34f) StartCoroutine(CatLeap(at, size));
            else if (roll < 0.67f) StartCoroutine(CurtainOut(at, size));
            else StartCoroutine(PotFalls(at, size));
        }

        Transform FxSprite(string name, float worldHeight) => FxSprite(name, worldHeight, new Vector2(0.5f, 0.5f));

        Transform FxSprite(string name, float worldHeight, Vector2 pivot)
        {
            var tex = Resources.Load<Texture2D>("Art/Fx/" + name);
            if (tex == null) return null;
            var go = new GameObject(name);
            var r = go.AddComponent<SpriteRenderer>();
            r.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), pivot, 100f);
            go.transform.localScale = Vector3.one * (worldHeight / Mathf.Max(0.01f, r.sprite.bounds.size.y));
            return go.transform;
        }

        IEnumerator CatLeap(Vector3 at, Vector3 size)
        {
            yield return new WaitForSeconds(0.25f);
            var t = FxSprite("fx_cat", 0.45f);
            if (t == null)
            {
                t = new GameObject("Cat").transform;
                var fur = Mat(new Color(0.85f, 0.55f, 0.25f));
                var b = Box(t, Vector3.zero, new Vector3(0.42f, 0.18f, 0.18f), fur);
                Box(t, new Vector3(0.24f, 0.1f, 0f), new Vector3(0.16f, 0.15f, 0.15f), fur);
                Box(t, new Vector3(-0.28f, 0.1f, 0f), new Vector3(0.22f, 0.04f, 0.04f), fur);
            }
            float dir = Random.value < 0.5f ? -1f : 1f;
            if (t.GetComponent<SpriteRenderer>() is SpriteRenderer sr) sr.flipX = dir < 0f;
            // it pops out of the hole (grows from the dark), leaps down to the pavement under the house and runs off:
            // the nose follows the arc (up on the way out, down on the way down), a squash on landing.
            Vector3 full = t.localScale;
            Vector3 from = at + new Vector3(0f, -size.y * 0.25f, -0.1f), to = new Vector3(at.x + dir * 1.6f, 0.22f, at.z - 1.2f);
            for (float k = 0f; k < 1f && t != null; k += Time.deltaTime / 0.25f) { t.position = from; t.localScale = full * Mathf.SmoothStep(0.3f, 1f, k); yield return null; }
            float hop = 0.6f, T = 0.9f;
            Vector3 last = from;
            for (float k = 0f; k < 1f && t != null; k += Time.deltaTime / T)
            {
                Vector3 pos = Vector3.Lerp(from, to, k) + Vector3.up * (4f * hop * k * (1f - k));
                Vector3 v = pos - last; last = pos;
                if (v.sqrMagnitude > 1e-6f) t.localRotation = Quaternion.Euler(0f, 0f, Mathf.Clamp(Mathf.Atan2(v.y, Mathf.Abs(v.x) + 0.001f) * Mathf.Rad2Deg, -60f, 40f) * dir);
                t.position = pos;
                yield return null;
            }
            if (t == null) yield break;
            t.localRotation = Quaternion.identity;
            for (float k = 0f; k < 1f && t != null; k += Time.deltaTime / 0.15f) { t.localScale = new Vector3(full.x * (1f + 0.15f * Mathf.Sin(k * Mathf.PI)), full.y * (1f - 0.2f * Mathf.Sin(k * Mathf.PI)), full.z); yield return null; }
            // runs off along the pavement, a little bound in every stride
            for (float k = 0f; k < 2f && t != null; k += Time.deltaTime)
            {
                Vector3 q = t.position; q.x += dir * 4.5f * Time.deltaTime; q.y = 0.22f + Mathf.Abs(Mathf.Sin(k * 14f)) * 0.06f;
                t.position = q;
                yield return null;
            }
            if (t != null) Destroy(t.gameObject);
        }

        /// <summary>The curtain hangs from its rod at the top of the hole, a gust blows it out through the broken glass,
        /// then it keeps breathing in the wind and finally drops out of the frame. Pivot at the rod, never bigger than the window.</summary>
        IEnumerator CurtainOut(Vector3 at, Vector3 size)
        {
            var t = FxSprite("fx_curtain", size.y * 0.8f, new Vector2(0.5f, 1f));
            if (t == null) yield break;
            var sr = t.GetComponent<SpriteRenderer>();
            float fit = Mathf.Min(1f, size.x * 0.8f / Mathf.Max(0.01f, sr.bounds.size.x));
            Vector3 baseScale = t.localScale * fit;
            t.localScale = baseScale;
            t.position = at + new Vector3(0f, size.y * 0.42f, -0.06f);
            float dir = Random.value < 0.5f ? -1f : 1f;
            sr.flipX = dir < 0f;
            float start = Time.time, life = 5f;
            while (t != null && Time.time - start < life)
            {
                float e = Time.time - start;
                float gust = Mathf.Exp(-e * 1.4f);              // the first push, dying out
                float breath = 0.25f * (1f + Mathf.Sin(e * 2.3f)) * Mathf.Sin(e * 5.1f + 1f); // slow wind afterwards
                // out through the hole: the bottom swings toward the street (scale y shrinks a bit as it tilts at us)
                t.localRotation = Quaternion.Euler(0f, 0f, dir * (28f * gust + 9f * breath));
                t.localScale = new Vector3(baseScale.x * (1f + 0.08f * gust * Mathf.Sin(e * 14f)), baseScale.y * (1f - 0.18f * gust), baseScale.z);
                yield return null;
            }
            if (t == null) yield break;
            // the rod gives way: it falls to the pavement, crumples and is gone
            float vy = 0f, rot = dir * 40f;
            Vector3 p = t.position;
            while (t != null && p.y > 0.1f)
            {
                vy -= 9.8f * 0.35f * Time.deltaTime;         // cloth: slower than a stone
                p += new Vector3(dir * 0.4f * Time.deltaTime, vy * Time.deltaTime, -0.8f * Time.deltaTime);
                t.position = p; t.Rotate(0f, 0f, rot * Time.deltaTime);
                yield return null;
            }
            for (float k = 0f; k < 1f && t != null; k += Time.deltaTime * 2f) { t.localScale = new Vector3(baseScale.x * (1f + 0.3f * k), baseScale.y * (1f - k), baseScale.z); yield return null; }
            if (t != null) Destroy(t.gameObject);
        }

        IEnumerator PotFalls(Vector3 at, Vector3 size)
        {
            var t = FxSprite("fx_pot", 0.35f);
            if (t == null)
            {
                t = new GameObject("Pot").transform;
                Box(t, Vector3.zero, new Vector3(0.22f, 0.2f, 0.22f), Mat(new Color(0.7f, 0.35f, 0.2f)));
                var leaves = GameObject.CreatePrimitive(PrimitiveType.Sphere); Strip(leaves);
                leaves.transform.SetParent(t, false); leaves.transform.localPosition = new Vector3(0f, 0.18f, 0f); leaves.transform.localScale = Vector3.one * 0.28f;
                leaves.GetComponent<Renderer>().sharedMaterial = Mat(new Color(0.25f, 0.55f, 0.2f));
            }
            // it sits on the sill, wobbles from the blow, tips over the edge and falls with a spin
            float side = Random.value < 0.5f ? -1f : 1f;
            Vector3 p = at + new Vector3(side * size.x * 0.22f, -size.y * 0.5f + 0.17f, -0.12f);
            t.position = p;
            for (float k = 0f; k < 1f && t != null; k += Time.deltaTime / 0.7f)
            {
                t.localRotation = Quaternion.Euler(0f, 0f, side * (Mathf.Sin(k * 22f) * 8f * (1f - k) + 35f * k * k));
                t.position = p + new Vector3(0f, 0f, -0.12f * k * k);
                yield return null;
            }
            if (t == null) yield break;
            p = t.position;
            float vy = 0.6f, spin = side * Random.Range(120f, 240f);
            Vector3 drift = new Vector3(side * 0.3f, 0f, -0.9f);
            while (t != null && p.y > 0.15f)
            {
                vy -= 9.8f * Time.deltaTime; p += (drift + Vector3.up * vy) * Time.deltaTime;
                t.position = p; t.Rotate(0f, 0f, spin * Time.deltaTime);
                yield return null;
            }
            if (t == null) yield break;
            GameEvents.RaiseWallHit(p); // a thud
            // smashed: little bits of terracotta and soil
            for (int k = 0; k < 8; k++)
            {
                var bit = GameObject.CreatePrimitive(PrimitiveType.Cube); Strip(bit);
                bit.transform.position = p; bit.transform.localScale = Vector3.one * Random.Range(0.04f, 0.09f);
                bit.GetComponent<Renderer>().sharedMaterial = Mat(k % 2 == 0 ? new Color(0.7f, 0.35f, 0.2f) : new Color(0.3f, 0.2f, 0.12f));
                var rb = bit.AddComponent<Rigidbody>(); rb.linearVelocity = new Vector3(Random.Range(-1.5f, 1.5f), Random.Range(1f, 2.5f), Random.Range(-1.5f, 0.5f));
                Destroy(bit, 2f);
            }
            Destroy(t.gameObject);
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

            if (showStart && duel)
            {
                Panel();
                HomeScreen(u, big);
            }
            else if (showStart)
            {
                Panel();
                GUILayout.BeginArea(new Rect(pad * 2, Screen.height * 0.12f, Screen.width - pad * 4, Screen.height * 0.8f));
                GUILayout.Label(Ar.T(Ar.Title), big);
                // IMGUI wraps left-to-right; a wrapped Arabic paragraph would break in the wrong places, so it is split by sentence.
                foreach (var line in (duel ? Ar.DuelIntro : Ar.Intro).Split('.'))
                    if (line.Trim().Length > 0) GUILayout.Label(Ar.T(line.Trim() + "."), new GUIStyle(label) { alignment = TextAnchor.MiddleRight });
                GUILayout.Space(20 * u);
                if (GUILayout.Button(Ar.T(Ar.Play), button, GUILayout.Height(80 * u))) StartGameFromScript();
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

        /// <summary>
        /// Home: two big picture buttons, a duel and the 12/3 challenge (SCENARIO §10), shards and curses in the corners,
        /// the running challenge under its button. No paragraphs (user 2026-11-02: «کلا تو بازی نوشته زیاده»).
        /// </summary>
        void HomeScreen(float u, GUIStyle big)
        {
            float pad = 16 * u, isz = 44 * u;
            var num = new GUIStyle(big) { fontSize = Mathf.RoundToInt(32 * u), alignment = TextAnchor.MiddleLeft };
            GUI.Label(new Rect(0, pad + SafeTop(), Screen.width, 70 * u), Ar.T(Ar.Title), new GUIStyle(big) { alignment = TextAnchor.MiddleCenter });
            DuelHud.DrawIcon(new Rect(pad, pad + SafeTop(), isz, isz), "shards", Color.white);
            GUI.Label(new Rect(pad + isz + 8 * u, pad + SafeTop(), 200 * u, isz), Ar.N(duelGame.ShardBank), num);
            DuelHud.DrawIcon(new Rect(Screen.width - pad - isz, pad + SafeTop(), isz, isz), "curse", Color.white);
            GUI.Label(new Rect(Screen.width - pad - isz - 208 * u, pad + SafeTop(), 200 * u, isz), Ar.N(duelGame.Curses[0]), new GUIStyle(num) { alignment = TextAnchor.MiddleRight });

            float bw = Mathf.Min(260 * u, Screen.width * 0.3f), bh = bw * 0.75f, by = Screen.height * 0.3f;
            var duelR = new Rect(Screen.width * 0.5f + 20 * u, by, bw, bh);       // right = first for a right-to-left reader
            var chalR = new Rect(Screen.width * 0.5f - 20 * u - bw, by, bw, bh);
            if (DuelHud.IconButton(duelR, "duel", new Color(0.16f, 0.5f, 0.32f), true, null)) StartGameFromScript();
            bool can = duelGame.CanEnterChallenge;
            if (DuelHud.IconButton(chalR, "trophy", new Color(0.85f, 0.5f, 0.12f), can, null))
            {
                showStart = false; endTitle = null; touch?.IgnoreFor(0.5f); duelGame.PlayChallenge();
            }
            float sy = by + bh + 16 * u;
            var run = duelGame.Challenge;
            if (duelGame.ChallengeActive)
            {
                DuelHud.ChallengeStrip(new Rect(chalR.x - 40 * u, sy, chalR.width + 80 * u, 40 * u), run, u);
                var left = run.EndsUtc - System.DateTime.UtcNow;
                DuelHud.DrawIcon(new Rect(chalR.x, sy + 90 * u, 30 * u, 30 * u), "clock", Color.white);
                GUI.Label(new Rect(chalR.x + 38 * u, sy + 90 * u, 200 * u, 30 * u), Ar.N(Mathf.Max(0, (int)left.TotalHours)) + ":" + Ar.N(Mathf.Max(0, left.Minutes)).PadLeft(2, '٠'),
                    new GUIStyle(num) { fontSize = Mathf.RoundToInt(24 * u) });
            }
            else
            {
                // the entry: shards price (0 = the free first run)
                DuelHud.DrawIcon(new Rect(chalR.x + chalR.width * 0.5f - 40 * u, sy, 34 * u, 34 * u), "shards", can ? Color.white : new Color(1f, 0.4f, 0.4f));
                GUI.Label(new Rect(chalR.x + chalR.width * 0.5f, sy, 200 * u, 34 * u), Ar.N(duelGame.ChallengeFree ? 0 : Sheytoonak.Core.Challenge.ChallengeRun.DefaultEntry),
                    new GUIStyle(num) { fontSize = Mathf.RoundToInt(26 * u) });
            }
            BagRack(u);
        }

        static readonly Color[] BagTint =
        {
            new Color(0.55f, 0.75f, 0.45f), new Color(0.40f, 0.65f, 1f), new Color(0.75f, 0.45f, 1f), new Color(1f, 0.8f, 0.25f)
        };

        /// <summary>
        /// The four garbage-bag slots along the bottom (SCENARIO §8). Tap a closed bag: its timer starts (one at a time);
        /// tap a ready one: it opens. Under each: its hours, the time left, or a pulsing «open». No words.
        /// </summary>
        void BagRack(float u)
        {
            var rack = duelGame.Bags;
            var now = System.DateTime.UtcNow;
            float sw = Mathf.Min(150 * u, (Screen.width - 80 * u) / 4f), gap = 14 * u;
            float total = sw * 4 + gap * 3, x0 = (Screen.width - total) * 0.5f, y = Screen.height - sw - 70 * u;
            var num = new GUIStyle(GUI.skin.label) { font = Ar.Font, fontSize = Mathf.RoundToInt(22 * u), alignment = TextAnchor.MiddleCenter };
            num.normal.textColor = Color.white;
            for (int i = 0; i < Sheytoonak.Core.Bags.BagRules.Slots; i++)
            {
                // right-to-left: slot 0 is on the right
                var r = new Rect(x0 + (3 - i) * (sw + gap), y, sw, sw);
                var old = GUI.color;
                GUI.color = new Color(0f, 0f, 0f, 0.35f); GUI.DrawTexture(r, Texture2D.whiteTexture); GUI.color = old;
                var b = rack.Rack[i];
                if (b == null) continue;
                bool ready = rack.IsReady(i, now);
                float pulse = ready ? 1f + 0.06f * Mathf.Sin(Time.time * 6f) : 1f;
                var ir = new Rect(r.center.x - r.width * 0.4f * pulse, r.center.y - r.height * 0.4f * pulse, r.width * 0.8f * pulse, r.height * 0.8f * pulse);
                DuelHud.DrawIcon(ir, "bag", BagTint[(int)b.Size]);
                if (GUI.Button(r, GUIContent.none, GUIStyle.none)) { touch?.IgnoreFor(0.5f); duelGame.TapBag(i); }
                var lr = new Rect(r.x, r.yMax + 4 * u, r.width, 30 * u);
                if (ready) DuelHud.DrawIcon(new Rect(lr.center.x - 15 * u, lr.y, 30 * u, 30 * u), "pick", new Color(0.4f, 1f, 0.5f));
                else if (b.Unlocking)
                {
                    var left = rack.TimeLeft(i, now);
                    GUI.Label(lr, Ar.N((int)left.TotalHours) + ":" + Ar.N(left.Minutes).PadLeft(2, '٠'), num);
                }
                else
                {
                    DuelHud.DrawIcon(new Rect(lr.center.x - 34 * u, lr.y + 2 * u, 26 * u, 26 * u), "clock", Color.white);
                    GUI.Label(new Rect(lr.center.x - 4 * u, lr.y, 50 * u, lr.height), Ar.N(Sheytoonak.Core.Bags.BagRules.Hours[(int)b.Size]), num);
                }
            }
            if (Time.time < lootUntil)
            {
                var big = new GUIStyle(num) { fontSize = Mathf.RoundToInt(44 * u) };
                float cy = Screen.height * 0.5f;
                var old = GUI.color; GUI.color = new Color(0f, 0f, 0f, 0.7f);
                GUI.DrawTexture(new Rect(Screen.width * 0.5f - 200 * u, cy - 70 * u, 400 * u, 140 * u), Texture2D.whiteTexture); GUI.color = old;
                DuelHud.DrawIcon(new Rect(Screen.width * 0.5f - 170 * u, cy - 30 * u, 60 * u, 60 * u), "shards", Color.white);
                GUI.Label(new Rect(Screen.width * 0.5f - 110 * u, cy - 30 * u, 110 * u, 60 * u), "+" + lootShards, big);
                if (lootCards > 0)
                {
                    DuelHud.DrawIcon(new Rect(Screen.width * 0.5f + 20 * u, cy - 30 * u, 60 * u, 60 * u), "card", new Color(1f, 0.85f, 0.4f));
                    GUI.Label(new Rect(Screen.width * 0.5f + 80 * u, cy - 30 * u, 100 * u, 60 * u), "+" + lootCards, big);
                }
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
