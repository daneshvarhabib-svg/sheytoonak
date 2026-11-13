using System.Collections;
using System.Collections.Generic;
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
        public float duelCameraPitchDeg = 3f; // run 108: at 5 a 3:1 painting's top row did not fit under 86 % of the screen; 3 shows ~0.7 m more wall
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
        BackdropWindowMapper windowMapper;
        string toast = "";
        int lootShards; string[] lootCards; bool lootShown; float lootAt; // the last opened bag, shown over the home screen until tapped
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

        // Splash (user 2026-11-07: «بذار رو بازی، نوشته بیاد بالای اسپلش»): the painted alley (Art/UI/splash) with the tin
        // sign logo (Art/UI/logo) hanging over it for a few seconds after boot; a tap skips it.

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
            GameEvents.RoundStarted += OnRoundStarted;
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
            GameEvents.RoundStarted -= OnRoundStarted;
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
            // the duel camera stands further back: 1.2 cm read as no shake at all (user 2026-11-05: «لرزش ریز برخورد توپ با
            // دیوار … حذف شده»); still well under the 7 cm "earthquake" of 2026-10-28.
            if (duel) { shake.wallAmplitude = 0.03f; shake.wallSeconds = 0.2f; }

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
            // User (2026-11-05): «پس زمینه خونه ها رو عوض کن و فعلا فقط از این ۶ تا استفاده کن تا تصویر جدید بیاد … اون اولیه که
            // تصویر شیشه شکسته‌ش رو نداریم رو بی‌خیال شو». Only houses painted twice (intact + every pane smashed) are dealt;
            // House1..House6 stay in the code but out of the pool.
            // User (2026-11-05): «به نظرم دوباره تصویر خونه رو بگیریم بهتره» — the pool is the wide (2.3:1) paintings only; the 3:2
            // houses 7–12 stay in code until their wide versions arrive.
            // User (2026-11-05): the 3:1 paintings (2172x724) replace the 2.3:1 ones; houses 13–22 stay in code until their 3:1
            // versions arrive («فعلا این سه تا رو داشته باش تا باقی برسن»).
            // test (user, 2026-11-05): one 3:1 painting with the top row 8 % below the edge, alone, to see if it fills the frame
            var houses = new[] { House36(), House37(), House38(), House39(), House40() }; // 15, 13, 21, 11, 17 windows: at least 11, always odd
            allHouses = houses;

            // Player: foot position + window mapping. No body: the game is first-person (POV).
            var player = new GameObject("Player");
            foot = player.AddComponent<PlayerFoot>();
            var mapper = player.AddComponent<BackdropWindowMapper>();
            windowMapper = mapper;
            mapper.cam = cam;

            // Football: a sphere with a procedural black-and-white texture.
            var ballGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ballGo.name = "Ball";
            ballGo.transform.localScale = Vector3.one * (config.ballRadius * 2f);
            var r = ballGo.GetComponent<Renderer>();
            r.sharedMaterial = ball;
            // User (2026-11-06): «توپی که باهاش شیشه میارن پایین بهتره توپ بچه‌های کوچه باشه نه این توپ تر و تمیز»: a painted
            // street ball (Art/Props/ball_street, an equirectangular wrap) when it exists, the clean procedural one until then.
            var street = Resources.Load<Texture2D>("Art/Props/ball_street");
            r.material.mainTexture = street != null ? street : FootballTexture();
            if (street != null) r.material.SetFloat("_Glossiness", 0.08f);
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
                duelGame.house = houses[0];
                duelGame.housePool = houses; // a different house every duel
                // CI screenshots always show the newest house, so each new painting is actually seen before the user plays it.
                if (!string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("SHEYTOONAK_SHOTS")))
                    duelGame.housePool = houses.Length >= 2 ? new[] { houses[houses.Length - 2], houses[houses.Length - 1] } : houses;
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
                duelGame.BagOpened += (slot, shards, cards) => { lootShards = shards; lootCards = cards ?? new string[0]; lootShown = true; lootAt = Time.time; };
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
            gameGo.AddComponent<MusicPlayer>();
            gameGo.AddComponent<SplashOverlay>();
            gameGo.AddComponent<UpgradeCelebration>();
            var arc = gameGo.AddComponent<AimArc>(); arc.duel = duelGame; arc.touch = touch;
            var feel = gameGo.AddComponent<KickFeel>(); feel.duel = duelGame; feel.shake = shake;
            gameGo.AddComponent<HeroVoice>();
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

        // Beirut sandstone, green shutters, 4 x 3 windows (user, 2026-11-05). Every pot is cut out of the painting with its
        // real pixel rect, so the pot the ball shook loose is the very pot of that window (user: «گلدون جلوی پنجره جدا شه
        // از قاب و بیفته و برسه زمین بشکنه»). Glass rects are between the shutters.
        HouseDefinition House6() => MakePaintedHouse("البيت ٦", "Art/house_06", 1536f, 1024f, 13f,
            new[]
            {
                R(235f, 90f, 355f, 230f), R(538f, 90f, 675f, 230f), R(855f, 90f, 993f, 230f), R(1177f, 90f, 1306f, 230f),
                R(235f, 333f, 357f, 470f), R(540f, 333f, 675f, 470f), R(855f, 333f, 995f, 470f), R(1180f, 333f, 1310f, 470f),
                R(233f, 565f, 360f, 705f), R(538f, 565f, 675f, 705f), R(855f, 565f, 990f, 705f), R(1180f, 565f, 1310f, 705f),
            },
            0.25f, 1.95f, 0f, 1.1f,
            new[]
            {
                Pot(0, 240f, 175f, 345f, 272f), Pot(1, 618f, 168f, 678f, 232f), Pot(2, 850f, 145f, 922f, 232f), Pot(3, 1238f, 158f, 1310f, 232f),
                Pot(4, 272f, 398f, 342f, 474f), Pot(5, 573f, 386f, 672f, 474f), Pot(7, 1228f, 403f, 1316f, 474f),
                Pot(8, 233f, 646f, 300f, 710f), Pot(9, 553f, 643f, 647f, 772f), Pot(10, 896f, 638f, 992f, 710f), Pot(11, 1236f, 633f, 1312f, 710f),
                new WindowDetail(6, DetailKind.Laundry) { pixel = R(820f, 413f, 1032f, 502f) },
            });

        const float Wide3W = 13f * 2172f / 1536f; // a 3:1 painting at the same pixel density as the old 13 m 3:2 houses
        // Houses 7–27 (3:2 and 2.3:1 paintings) were removed on 2026-11-07 (design review §2.8): the pool is the 3:1 gritty houses only.

        // First house of the odd-count rule (user 2026-11-07: «حداقل ۱۱ و حداکثر ۲۷، فرد»): 5 + 5 + (4 barred + shop) = 15.
        HouseDefinition House36() => MakePaintedHouse("البيت ٣٦", "Art/house_36", 2172f, 724f, Wide3W, // Beirut, peeling plaster, three rows, 17 windows (cartoon repaint 2026-11-12)
            new[]
            {
                // 2026-11-12: cartoon repaint (user: «دیزاین خونه ها هم با بازیکنا نمیخوره … فانتزی‌تر»); panes measured on the new
                // picture, and the broken twin rebuilt pane by pane from the AI's broken picture at each pane's own offset
                R(163f, 27f, 280f, 147f), R(508f, 27f, 628f, 147f), R(858f, 27f, 975f, 147f), R(1203f, 27f, 1321f, 147f), R(1551f, 27f, 1670f, 147f), R(1898f, 27f, 2016f, 147f), R(163f, 223f, 288f, 380f),
                R(505f, 223f, 630f, 380f), R(853f, 223f, 977f, 380f), R(1200f, 223f, 1325f, 380f), R(1548f, 223f, 1672f, 380f), R(1895f, 223f, 2020f, 380f), R(152f, 478f, 295f, 608f), R(494f, 478f, 637f, 608f),
                R(755f, 478f, 1075f, 652f), R(1537f, 478f, 1687f, 608f), R(1884f, 478f, 2031f, 608f),
            },
            0.25f, 1.95f, 0f, 1.2f, null, "Art/house_36_broken", R(684f, 163f, 825f, 212f));

        // Damascus ablaq spice shop, arched windows: 5 + 5 + (2 barred + shop) = 13. ⚠️ the broken twin came 2110x745 and was
        // resized to 2172x724 so the cut-out rectangles line up.
        HouseDefinition House37() => MakePaintedHouse("البيت ٣٧", "Art/house_37", 2172f, 724f, Wide3W,
            new[]
            {
                // 2026-11-12: cartoon repaint (user: «دیزاین خونه ها هم با بازیکنا نمیخوره … فانتزی‌تر»); panes measured on the new
                // picture, and the broken twin rebuilt pane by pane from the AI's broken picture at each pane's own offset
                R(306f, 49f, 421f, 195f), R(673f, 49f, 791f, 195f), R(1048f, 49f, 1162f, 195f), R(1420f, 49f, 1537f, 195f), R(1790f, 49f, 1906f, 195f), R(306f, 255f, 421f, 396f), R(673f, 255f, 791f, 396f),
                R(1048f, 255f, 1162f, 396f), R(1420f, 255f, 1537f, 396f), R(1790f, 255f, 1906f, 396f), R(299f, 494f, 440f, 635f), R(877f, 543f, 1531f, 695f), R(1761f, 494f, 1903f, 635f),
            },
            // ⚠️ 2026-11-07 the twin (2110x745) was dropped as "a different generation"; that was wrong -- blended over the
            // intact 2172x724 it lines up window for window once resized, so it is back (resized to the intact size). The
            // mismatched panes the user saw were the generic Art/Broken panes that stood in for it.
            0.25f, 1.95f, 0f, 1.2f, null, "Art/house_37_broken", R(1662f, 467f, 1738f, 586f));

        // Beirut 1970s concrete block, pink, bullet-pocked: 7 + 7 + 6 barred + the glass of the stairwell door = 21 (review 2026-11-07).
        HouseDefinition House38() => MakePaintedHouse("البيت ٣٨", "Art/house_38", 2172f, 724f, Wide3W,
            new[]
            {
                // 2026-11-12: cartoon repaint (user: «دیزاین خونه ها هم با بازیکنا نمیخوره … فانتزی‌تر»); panes measured on the new
                // picture, and the broken twin rebuilt pane by pane from the AI's broken picture at each pane's own offset
                R(230f, 71f, 361f, 161f), R(502f, 71f, 632f, 161f), R(760f, 71f, 891f, 161f), R(1021f, 71f, 1151f, 161f), R(1288f, 71f, 1418f, 161f), R(1553f, 71f, 1683f, 161f), R(1816f, 71f, 1946f, 161f),
                R(230f, 269f, 361f, 391f), R(502f, 269f, 632f, 391f), R(760f, 269f, 891f, 391f), R(1021f, 269f, 1151f, 391f), R(1288f, 269f, 1418f, 391f), R(1553f, 269f, 1683f, 391f), R(1816f, 269f, 1946f, 391f),
                R(230f, 500f, 361f, 597f), R(502f, 500f, 632f, 597f), R(760f, 500f, 891f, 597f), R(1021f, 497f, 1153f, 695f), R(1288f, 500f, 1418f, 597f), R(1553f, 500f, 1683f, 597f), R(1816f, 500f, 1946f, 597f),
            },
            0.25f, 1.95f, 0f, 1.2f, null, "Art/house_38_broken", R(1195f, 174f, 1271f, 250f));

        // Alexandria corniche, lime wash and green wood, café with neon: 4 + 4 + (kiosk + café + barred) = 11.
        HouseDefinition House39() => MakePaintedHouse("البيت ٣٩", "Art/house_39", 2172f, 724f, Wide3W,
            new[]
            {
                // 2026-11-12: cartoon repaint (user: «دیزاین خونه ها هم با بازیکنا نمیخوره … فانتزی‌تر»); panes measured on the new
                // picture, and the broken twin rebuilt pane by pane from the AI's broken picture at each pane's own offset
                R(326f, 33f, 497f, 152f), R(812f, 33f, 977f, 152f), R(1214f, 33f, 1381f, 152f), R(1700f, 33f, 1866f, 152f), R(324f, 217f, 497f, 304f), R(804f, 217f, 977f, 304f), R(1208f, 217f, 1381f, 304f),
                R(1694f, 217f, 1870f, 304f), R(233f, 527f, 543f, 646f), R(733f, 494f, 1341f, 662f), R(1466f, 478f, 1642f, 635f),
            },
            0.25f, 1.95f, 0f, 1.2f, null, "Art/house_39_broken", R(1499f, 195f, 1586f, 282f));

        // Tunis medina, yellowed plaster and cobalt blue, bakery: 6 + 6 + (4 barred + bakery) = 17.
        HouseDefinition House40() => MakePaintedHouse("البيت ٤٠", "Art/house_40", 2172f, 724f, Wide3W,
            new[]
            {
                // 2026-11-12: the cartoon repaint (user: «دیزاین خونه ها هم با بازیکنا نمیخوره … فانتزی‌تر») kept the layout
                // but its right half drifted up to 45 px, so the panes were measured again on the new picture
                R(307f, 33f, 400f, 128f), R(665f, 33f, 755f, 128f), R(932f, 33f, 1021f, 128f), R(1222f, 33f, 1314f, 128f), R(1513f, 33f, 1605f, 128f), R(1833f, 33f, 1922f, 128f),
                R(280f, 206f, 432f, 331f), R(633f, 206f, 785f, 331f), R(896f, 206f, 1048f, 331f), R(1187f, 206f, 1341f, 331f), R(1491f, 206f, 1642f, 331f), R(1808f, 206f, 1960f, 331f),
                R(185f, 445f, 350f, 628f), R(529f, 445f, 695f, 628f), R(787f, 440f, 1119f, 630f), R(1504f, 445f, 1662f, 628f), R(1873f, 445f, 2029f, 628f),
            },
            0.25f, 1.95f, 0f, 1.2f, null, "Art/house_40_broken", R(1250f, 420f, 1450f, 600f));

        // Houses 28–35 (3:1 but 5–9 windows) left the pool on 2026-11-07 (user: «حداقل ۱۱ تا پنجره قابل شکستن … باید بمونن»).


        static Rect R(float x0, float y0, float x1, float y1) => Rect.MinMaxRect(x0, y0, x1, y1);
        static WindowDetail Pot(int w, float x0, float y0, float x1, float y1) => new WindowDetail(w, DetailKind.Pot) { pixel = R(x0, y0, x1, y1) };

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
        /// <summary>
        /// Width the facade painting needs to fill the phone from edge to edge: the duel camera (z -6, 64 degrees across) sees
        /// 2 * 13 * tan(32) = 16.2 m at the facade 13 m away; a little more so the edge never shows on wide phones.
        /// User (2026-11-05): «کلا ما تو قاب فقط همون خونه رو ببینیم». Windows scale with it (they are given in image pixels).
        /// </summary>
        const float PaintedFacadeWidth = 17.2f;

        /// <summary>
        /// ⚠️ run 88: 17.2 m still left sky on both sides. The horizontal field of view is not fixed: with the minimum vertical
        /// FOV of 48 degrees a 2.2:1 phone sees ~25 m at the facade. So the painting is stretched to the camera's real view
        /// at build time, and the window rectangles (stored for a PaintedFacadeWidth-wide quad) are scaled with it.
        /// </summary>
        float FacadeWidthForCamera(float facadeZ)
        {
            if (cam == null) return PaintedFacadeWidth;
            float dist = facadeZ - cam.transform.position.z;
            float vFov = Mathf.Max(cameraMinVerticalFov, Camera.HorizontalToVerticalFieldOfView(cameraHorizontalFov, Mathf.Max(0.5f, cam.aspect)));
            float hFov = Camera.VerticalToHorizontalFieldOfView(vFov, Mathf.Max(0.5f, cam.aspect));
            return Mathf.Max(PaintedFacadeWidth, 2f * dist * Mathf.Tan(hFov * 0.5f * Mathf.Deg2Rad) * 1.04f);
        }

        /// <summary>
        /// World height at the facade that sits at the given screen fraction (0 = bottom edge, 1 = top edge), from the
        /// camera's pitch and its real vertical field of view.
        /// </summary>
        float HeightAtScreenFraction(float facadeZ, float fraction)
        {
            if (cam == null) return 1e9f;
            float dist = facadeZ - cam.transform.position.z;
            float vFov = Mathf.Max(cameraMinVerticalFov, Camera.HorizontalToVerticalFieldOfView(cameraHorizontalFov, Mathf.Max(0.5f, cam.aspect)));
            float up = -cameraPitchDeg + (fraction - 0.5f) * vFov; // Euler X is positive when the camera looks down
            return cam.transform.position.y + dist * Mathf.Tan(up * Mathf.Deg2Rad);
        }

        static HouseDefinition MakePaintedHouse(string name, string texture, float imgW, float imgH, float w, Rect[] pixelWindows,
                                                float wind, float returnTime, float spread, float hitScale, WindowDetail[] details = null,
                                                string broken = null, Rect wallPatch = default)
        {
            w = PaintedFacadeWidth * w / 13f; // the 13 m houses were tuned at the old width; a cropped one keeps its ratio
            float h = w * imgH / imgW;
            var house = ScriptableObject.CreateInstance<HouseDefinition>();
            house.name = name;
            house.displayName = name;
            house.paintedTexture = texture;
            house.brokenTexture = broken ?? "";
            house.wallPatch = wallPatch; // pixels of the painting, chosen by eye: no window, door, lamp, plant or awning
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
            if (!string.IsNullOrEmpty(h.paintedTexture))
            {
                float want = FacadeWidthForCamera(z);
                // User (2026-11-05, run 115): «یه کاری کنیم همه قاب پنجره بیفته ولی همسایه هم ندیم … از بالا آسمون بدیم یکمی هم از
                // راست و چپ بکشیمش». The painting always spans the camera's width (no neighbours). Its height follows its own
                // aspect, but if the top window row would sit above 88 % of the screen the height is reduced until it fits:
                // the picture is then a little wider than its aspect (stretched sideways), and the room above it is sky.
                viewHalfWidth = want * 0.5f;
                float kx = want / w, ky = kx;
                float topLimit = HeightAtScreenFraction(z, 0.76f), topWindow = 0f; // user (2026-11-06): «آسمون … تیکه آبی داشته باشه اون بالا» — room for sky above the roof line
                foreach (var r0 in h.worldWindows) topWindow = Mathf.Max(topWindow, r0.yMax);
                if (topWindow > 0.01f && topWindow * ky > topLimit) ky = Mathf.Max(kx * 0.6f, topLimit / topWindow);
                if (Mathf.Abs(kx - 1f) > 0.001f || Mathf.Abs(ky - 1f) > 0.001f)
                {
                    // the definition is scaled in place (windows are world rects derived from pixels at the old width) and
                    // mapped again, so the rules, the hit test and the drawing all agree on the new size
                    var wr = h.worldWindows;
                    for (int i = 0; i < wr.Length; i++)
                        wr[i] = Rect.MinMaxRect(wr[i].xMin * kx, wr[i].yMin * ky, wr[i].xMax * kx, wr[i].yMax * ky);
                    h.worldWindows = wr;
                    h.worldFacadeWidth = want; h.worldRoofHeight = roof * ky;
                    w = want; roof = roof * ky;
                    if (windowMapper != null) windowMapper.Map(h);
                }
                return BuildPaintedFacade(h, root, w, roof);
            }

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
        float viewHalfWidth; // half of what the camera sees at the facade, set in BuildFacade
        HouseDefinition[] allHouses; // the pool: the neighbours beside the duel house are faded copies of other paintings

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
            // User (2026-11-05): «عکس خونه باید ابعادش بزرگتر شه و کل تصویر رو بگیره … کلا ما تو قاب فقط همون خونه رو ببینیم».
            // ⚠️ run 89 filled the width and lost the upper window rows; run 90 mirrored the house beside itself and the user
            // said «خونه رو تکرار کرده فقط». A 3:2 painting cannot fill a 2.2:1 screen above the road without either, so the
            // house is sized to its windows and the strip left on each side is the painting's own outer 3 % stretched
            // sideways: the wall, the floor lines and the awning band run on, nothing repeats. (A wider painting fixes it.)
            if (viewHalfWidth > w * 0.5f + 0.05f)
            {
                // User (2026-11-05): «کنارش خونه‌های دیگه رو بذار ولی فیدش کن». The neighbours are two other paintings from
                // the pool at the same pixel scale, set a little back and dimmed to half brightness with the colour drained,
                // so the eye stays on the lit house in the middle. With no other painting the plain-wall patch tiles instead.
                var others = new System.Collections.Generic.List<HouseDefinition>();
                if (allHouses != null) foreach (var o in allHouses) if (o != h && !string.IsNullOrEmpty(o.paintedTexture)) others.Add(o);
                float side = viewHalfWidth - w * 0.5f + 0.5f;
                int pick = Random.Range(0, Mathf.Max(1, others.Count));
                for (int dir = -1, n = 0; dir <= 1; dir += 2, n++)
                {
                    var q = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    Strip(q);
                    q.name = "Neighbour " + dir;
                    q.transform.SetParent(root, false);
                    Texture2D nt = others.Count > 0 ? Resources.Load<Texture2D>(others[(pick + n) % others.Count].paintedTexture) : null;
                    if (nt != null)
                    {
                        float nw = w * nt.width / (float)tex.width * (tex.height / (float)nt.height); // same height as this painting
                        var nm = new Material(mat) { mainTexture = nt, color = new Color(0.5f, 0.5f, 0.52f) };
                        nm.SetFloat("_Glossiness", 0f);
                        // the far edge of the neighbour meets the duel house: the left one shows its right end
                        float u0 = dir < 0 ? Mathf.Max(0f, 1f - side / nw) : 0f, uw = Mathf.Min(1f, side / nw);
                        nm.mainTextureScale = new Vector2(uw, 1f); nm.mainTextureOffset = new Vector2(u0, 0f);
                        q.GetComponent<Renderer>().sharedMaterial = nm;
                        q.transform.localPosition = new Vector3(dir * (w * 0.5f + Mathf.Min(side, nw) * 0.5f), paintedHeight * 0.5f, 0.2f);
                        q.transform.localScale = new Vector3(Mathf.Min(side, nw), paintedHeight, 1f);
                        if (nw < side) // a third, dimmer wall patch covers whatever is left
                        {
                            var rest = GameObject.CreatePrimitive(PrimitiveType.Quad); Strip(rest); rest.transform.SetParent(root, false);
                            rest.transform.localPosition = new Vector3(dir * (w * 0.5f + nw + (side - nw) * 0.5f), paintedHeight * 0.5f, 0.25f);
                            rest.transform.localScale = new Vector3(side - nw, paintedHeight, 1f);
                            var pm = WallPatch(mat, tex, h.wallPatch, w, paintedHeight, side - nw); pm.color = new Color(0.45f, 0.45f, 0.47f);
                            rest.GetComponent<Renderer>().sharedMaterial = pm;
                        }
                    }
                    else
                    {
                        q.transform.localPosition = new Vector3(dir * (w * 0.5f + side * 0.5f), paintedHeight * 0.5f, 0.05f);
                        q.transform.localScale = new Vector3(side, paintedHeight, 1f);
                        q.GetComponent<Renderer>().sharedMaterial = WallPatch(mat, tex, h.wallPatch, w, paintedHeight, side);
                    }
                }
            }

            // The wall keeps going above the painting (no roof): a box in the painting's own brick colour.
            Color top = AverageColour(tex, 0.02f, 0.06f);
            // ⚠️ run 73/75: a flat average colour above the painting read as a grey/olive bar across the top of the screen.
            // The painting's own top rows stretched upward continue its sky (or wall) instead.
            // User (2026-11-05): sky above the painting instead of its stretched top rows.
            var skyMat = SkyGradient(mat);
            // the sky quad reaches just past the top of the screen and shows the painted sky's upper 70 % (the blue and
            // the power lines), not its haze: user (2026-11-06) «تیکه آبی داشته باشه اون بالا»
            float skyTop = Mathf.Max(paintedHeight + 1.5f, HeightAtScreenFraction(root.position.z, 1.08f));
            float above = skyTop - paintedHeight;
            var sky = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Strip(sky); sky.name = "Sky";
            sky.transform.SetParent(root, false);
            sky.transform.localPosition = new Vector3(0f, paintedHeight + above * 0.5f, 0.6f);
            sky.transform.localScale = new Vector3(Mathf.Max(w, viewHalfWidth * 2f + 1f) * 1.2f, above, 1f);
            // ⚠️ The painted sky is blue only in its top ~40 % of rows (user 2026-11-06: «تیکه آبی داشته باشه اون بالا»); the
            // quad runs from the roof to just past the screen top, so v 0.45..1.0 puts the blue under the HUD and the haze at the roof.
            // ⚠️ user (2026-11-12, phone): «آسمون زیاد کشیده شده و پرنده های آسمون خیلی بزرگن». The whole picture was spread over
            // the ~40 m quad while only ~3 m of its height showed: 3-4x wider than tall. Now the same rows keep the picture's
            // own proportions and it repeats sideways mirrored (seamless: each copy meets its own mirror image).
            var skyTex = skyMat.mainTexture;
            float tilesX = 1f;
            if (skyTex != null && skyTex.height > 0)
            {
                skyTex.wrapModeU = TextureWrapMode.Mirror;
                float tileW = above * skyTex.width / (skyTex.height * 0.55f);
                tilesX = sky.transform.localScale.x / Mathf.Max(0.5f, tileW);
            }
            skyMat.mainTextureScale = new Vector2(tilesX, 0.55f); skyMat.mainTextureOffset = new Vector2(0.5f - tilesX * 0.5f, 0.45f);
            sky.GetComponent<Renderer>().sharedMaterial = skyMat;
            // Neighbours: painted blank walls (Art/wall_left, Art/wall_right: houses without visible windows,
            // user 2026-10-27) when they exist, otherwise a darker shade of this painting's brick.
            // the neighbour walls (Art/wall_left/right) are gone with the full-frame painting (user, 2026-11-05)

            var rects = h.worldWindows;
            windowRenderers = new Renderer[rects.Length];
            // User (2026-11-05): «دو تا خروجی گرفتم یکی با فرض شکستن همه شیشه‌های پنجره و یکی معمولی … هر پنجره که شیشه‌ش
            // شکست پنجره‌اش رو از تصویر پنجره شکسته کراپ می‌کنی». The broken painting has the same framing, so the pane
            // is exactly the window's rectangle of that picture, laid over the intact one: the room, the torn curtain and the
            // glass left in the frame are the painter's, not a generic pane.
            var brokenTex = string.IsNullOrEmpty(h.brokenTexture) ? null : Resources.Load<Texture2D>(h.brokenTexture);
            paintedBroken = brokenTex != null;
            for (int i = 0; paintedBroken && i < rects.Length; i++)
            {
                Rect r = rects[i];
                var m = new Material(mat) { mainTexture = brokenTex };
                float u0 = r.xMin / w + 0.5f, u1 = r.xMax / w + 0.5f, v0 = r.yMin / paintedHeight, v1 = r.yMax / paintedHeight;
                m.mainTextureScale = new Vector2(u1 - u0, v1 - v0);
                m.mainTextureOffset = new Vector2(u0, v0);
                // ⚠️ run 83 (user): «وقتی شیشه می‌شکنه عکس پنجره ۱۸۰ درجه می‌چرخه و گلدون می‌ره بالا». A cube's face toward the
                // camera (-Z) maps its texture mirrored, so the smashed pane came out upside down. A quad facing the camera
                // maps it like the painting itself does.
                var pane = GameObject.CreatePrimitive(PrimitiveType.Quad);
                Strip(pane);
                pane.transform.SetParent(root, false);
                pane.transform.localPosition = new Vector3(r.center.x, r.center.y, -0.01f);
                pane.transform.localScale = new Vector3(r.width, r.height, 1f);
                pane.GetComponent<Renderer>().sharedMaterial = m;
                pane.name = "Broken pane " + i;
                windowRenderers[i] = pane.GetComponent<Renderer>();
                windowRenderers[i].enabled = false;
            }
            // User (2026-11-08, house 37 which has no smashed twin): «شیشه‌های شکسته داره رندوم می‌افته روی خونه ها، این
            // پنجره‌هاش این شکلی نبوده که». The generic painted panes (Art/Broken) are another house's window and never
            // match. Without a twin the pane is now cut from THIS painting's own window and smashed at break time
            // (OwnHoleWhereHit): same frame, same curtain, a dark jagged hole and cracks where the ball hit.
            ownPanes = !paintedBroken && tex.isReadable;
            for (int i = 0; ownPanes && i < rects.Length; i++)
            {
                Rect r = rects[i];
                var pane = GameObject.CreatePrimitive(PrimitiveType.Quad);
                Strip(pane);
                pane.transform.SetParent(root, false);
                pane.transform.localPosition = new Vector3(r.center.x, r.center.y, -0.01f);
                pane.transform.localScale = new Vector3(r.width, r.height, 1f);
                pane.GetComponent<Renderer>().sharedMaterial = mat; // replaced by the smashed crop when it breaks
                pane.name = "Broken pane " + i;
                windowRenderers[i] = pane.GetComponent<Renderer>();
                windowRenderers[i].enabled = false;
            }
            for (int i = 0; !paintedBroken && !ownPanes && i < rects.Length; i++)
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
                else if (d.kind == DetailKind.Tree) PaintedTree(d);
            return root;
        }

        Texture2D paintedTex; float paintedW, paintedH; Transform detailRoot;
        bool paintedBroken; // the house has a smashed twin painting (BuildPaintedFacade)
        bool ownPanes;      // no twin: smashed panes are cut from the intact painting itself at break time

        /// <summary>
        /// The window's own pixels from the intact painting with a jagged dark hole centred where the ball hit and a
        /// handful of cracks running to the frame. Nothing from another picture.
        /// </summary>
        void OwnHoleWhereHit(int window, Vector3 point)
        {
            var r = windowRenderers[window];
            if (r == null || paintedTex == null) return;
            Vector3 c = r.transform.position, sz = r.transform.lossyScale;
            // the quad's rect in painting pixels (the painting is paintedW x paintedH world units, centred in x, bottom at y=0)
            float u0 = (c.x - sz.x * 0.5f - detailRoot.position.x) / paintedW + 0.5f, u1 = u0 + sz.x / paintedW;
            float v0 = (c.y - sz.y * 0.5f - detailRoot.position.y) / paintedH, v1 = v0 + sz.y / paintedH;
            int x = Mathf.Clamp(Mathf.RoundToInt(u0 * paintedTex.width), 0, paintedTex.width - 2);
            int y = Mathf.Clamp(Mathf.RoundToInt(v0 * paintedTex.height), 0, paintedTex.height - 2);
            int cw = Mathf.Clamp(Mathf.RoundToInt((u1 - u0) * paintedTex.width), 2, paintedTex.width - x);
            int ch = Mathf.Clamp(Mathf.RoundToInt((v1 - v0) * paintedTex.height), 2, paintedTex.height - y);
            var px = paintedTex.GetPixels(x, y, cw, ch);
            float hx = Mathf.Clamp((point.x - c.x) / Mathf.Max(0.01f, sz.x) + 0.5f, 0.2f, 0.8f) * cw;
            float hy = Mathf.Clamp((point.y - c.y) / Mathf.Max(0.01f, sz.y) + 0.5f, 0.2f, 0.8f) * ch;
            var rnd = new System.Random(window * 31 + 7);
            // the hole: a wobbly radius round the hit, about a third of the window; cracks: 7-9 rays to the edge
            float baseR = Mathf.Min(cw, ch) * 0.30f;
            var wob = new float[16]; for (int k = 0; k < wob.Length; k++) wob[k] = 0.65f + (float)rnd.NextDouble() * 0.7f;
            int rays = 7 + rnd.Next(3);
            var rayA = new float[rays]; for (int k = 0; k < rays; k++) rayA[k] = (float)rnd.NextDouble() * Mathf.PI * 2f;
            var room = new Color(0.07f, 0.06f, 0.06f);
            for (int j = 0; j < ch; j++)
                for (int i = 0; i < cw; i++)
                {
                    float dx = i - hx, dy = j - hy, d = Mathf.Sqrt(dx * dx + dy * dy), a = Mathf.Atan2(dy, dx);
                    float t = (a + Mathf.PI) / (2f * Mathf.PI) * wob.Length; int k0 = (int)t % wob.Length; int k1 = (k0 + 1) % wob.Length;
                    float rr = baseR * Mathf.Lerp(wob[k0], wob[k1], t - Mathf.Floor(t));
                    int idx = j * cw + i;
                    if (d < rr) { px[idx] = Color.Lerp(room, px[idx], 0.12f); continue; }
                    if (d < rr + 2f) { px[idx] = Color.Lerp(px[idx], Color.white, 0.55f); continue; } // the glass edge catches light
                    for (int k = 0; k < rays; k++)
                    {
                        float da = Mathf.DeltaAngle(a * Mathf.Rad2Deg, rayA[k] * Mathf.Rad2Deg) * Mathf.Deg2Rad;
                        float across = Mathf.Abs(da) * d; // distance from the ray line
                        if (across < 0.9f + 0.006f * d) { px[idx] = Color.Lerp(px[idx], new Color(0.92f, 0.95f, 1f), 0.6f); break; }
                    }
                }
            var tex = new Texture2D(cw, ch, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            tex.SetPixels(px); tex.Apply();
            var m = new Material(r.sharedMaterial) { mainTexture = tex, mainTextureScale = Vector2.one, mainTextureOffset = Vector2.zero };
            m.SetFloat("_Glossiness", 0.3f);
            r.material = m;
        }

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
            var px = paintedTex.GetPixels(x, y, cw, ch);
            if (d.kind == DetailKind.Laundry && game != null)
            {
                // User (2026-11-05): «پنجره پشت لباس آویزان یه بخشی از کادر پنجره هم کراپ شده و با باد می‌لرزه». The clothes line
                // crosses windows; those pixels (window rect plus a 10 px frame) are cleared from the cutout so only cloth sways.
                var house = game.houses[Mathf.Clamp(game.Rules != null ? game.Rules.HouseIndex : 0, 0, game.houses.Length - 1)];
                float sx = paintedTex.width / paintedW, sy = paintedTex.height / paintedH;
                foreach (var wr in house.worldWindows)
                {
                    int wx0 = Mathf.RoundToInt((wr.xMin / paintedW + 0.5f) * paintedTex.width) - 10, wx1 = Mathf.RoundToInt((wr.xMax / paintedW + 0.5f) * paintedTex.width) + 10;
                    int wy0 = Mathf.RoundToInt(wr.yMin * sy) - 10, wy1 = Mathf.RoundToInt(wr.yMax * sy) + 10; // bottom-up, like GetPixels
                    for (int yy = Mathf.Max(0, wy0 - y); yy < Mathf.Min(ch, wy1 - y); yy++)
                        for (int xx = Mathf.Max(0, wx0 - x); xx < Mathf.Min(cw, wx1 - x); xx++)
                            px[yy * cw + xx].a = 0f;
                }
            }
            tex.SetPixels(px);
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

        /// <summary>
        /// User (2026-11-05): «درختا رو باید بندازی روی همون درختایی که تو تصاویر هست و یکمی تکونش بدی». The tree is cut out
        /// of the painting at its own pixels, pivoted at the trunk's foot, and leans gently with the wind (a slight lean and
        /// a slower sway, like branches, not a board rocking). The painting underneath keeps the tree too, so a lean only
        /// uncovers more leaves, never wall.
        /// </summary>
        void PaintedTree(WindowDetail d)
        {
            var sr = Cutout(d, -0.02f, out Rect pix);
            var t = sr.transform;
            // move the pivot to the bottom centre: wrap in a parent at the foot
            var foot = new GameObject("Tree foot").transform;
            foot.SetParent(detailRoot, false);
            float wx = (pix.center.x / paintedTex.width - 0.5f) * paintedW, wy0 = (1f - pix.yMax / paintedTex.height) * paintedH;
            foot.localPosition = new Vector3(wx, wy0, -0.02f);
            t.SetParent(foot, true);
            StartCoroutine(TreeSway(foot, Random.Range(0f, 6f)));
        }

        IEnumerator TreeSway(Transform t, float phase)
        {
            while (t != null)
            {
                float w = wind;
                float a = Mathf.Sin(Time.time * 0.9f + phase) * (0.8f + Mathf.Abs(w) * 3f) + Mathf.Sin(Time.time * 2.3f + phase * 1.7f) * 0.35f - w * 2.5f;
                t.localRotation = Quaternion.Euler(0f, 0f, a);
                yield return null;
            }
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

        // ⚠️ run 78: stretching the painting's top row upward drew vertical streaks (every pixel became a stripe). Instead
        // the top band (4 %) is averaged into 8 columns: a soft left-to-right gradient of its own sky/wall colours.
        /// <summary>A material that mirror-tiles the painting's plain-wall patch (pixels) over a strip `side` wide and
        /// `paintedHeight` high, at the painting's own pixel scale. No patch set: the painting's edge column, stretched.</summary>
        static Material WallPatch(Material painting, Texture2D tex, Rect patch, float paintedWidth, float paintedHeight, float side)
        {
            var m = new Material(painting);
            try
            {
                if (patch.width < 8f || patch.height < 8f) throw new UnityException("no wall patch");
                int x0 = Mathf.Clamp((int)patch.xMin, 0, tex.width - 8), y0 = Mathf.Clamp(tex.height - (int)patch.yMax, 0, tex.height - 8);
                int pw = Mathf.Min((int)patch.width, tex.width - x0), ph = Mathf.Min((int)patch.height, tex.height - y0);
                var t = new Texture2D(pw, ph, TextureFormat.RGBA32, true) { wrapMode = TextureWrapMode.Mirror, filterMode = FilterMode.Bilinear };
                t.SetPixels(tex.GetPixels(x0, y0, pw, ph)); t.Apply();
                float worldPerPx = paintedWidth / tex.width;
                m.mainTexture = t;
                m.mainTextureScale = new Vector2(side / (pw * worldPerPx), paintedHeight / (ph * worldPerPx));
                m.mainTextureOffset = Vector2.zero;
            }
            catch (UnityException)
            {
                m.mainTextureScale = new Vector2(0.001f, 1f); m.mainTextureOffset = new Vector2(0.5f, 0f);
            }
            return m;
        }

        /// <summary>
        /// A warm late-afternoon sky with soft clouds (user, 2026-11-06: «آسمون بالای ساختمون حس آسمون بده … یه پترن مثل
        /// آسفالت کف»): a 512x256 texture, haze at the roof line to blue above, with two octaves of Perlin clouds that thin
        /// out near the horizon. If Art/sky.png exists in Resources it is used instead.
        /// </summary>
        static Material SkyGradient(Material painting)
        {
            var m = new Material(painting) { color = Color.white };
            var painted = Resources.Load<Texture2D>("Art/sky");
            Texture2D g = painted;
            if (g == null)
            {
                const int W = 512, H = 256;
                g = new Texture2D(W, H, TextureFormat.RGBA32, true) { wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Bilinear };
                var low = new Color(0.95f, 0.87f, 0.76f); var high = new Color(0.45f, 0.66f, 0.90f); var cloud = new Color(1f, 0.97f, 0.93f);
                var px = new Color[W * H];
                for (int y = 0; y < H; y++)
                    for (int x = 0; x < W; x++)
                    {
                        float v = y / (float)(H - 1);
                        var c = Color.Lerp(low, high, Mathf.Pow(v, 0.7f));
                        float n = Mathf.PerlinNoise(x * 0.012f, y * 0.02f) * 0.65f + Mathf.PerlinNoise(x * 0.04f + 7f, y * 0.06f + 3f) * 0.35f;
                        float a = Mathf.SmoothStep(0.5f, 0.78f, n) * Mathf.Clamp01((v - 0.08f) * 2.5f) * 0.85f;
                        px[y * W + x] = Color.Lerp(c, cloud, a);
                    }
                g.SetPixels(px); g.Apply(true, false);
            }
            m.mainTexture = g; m.mainTextureScale = new Vector2(2f, 0.25f); m.mainTextureOffset = Vector2.zero; // the 60 m wall shows the lowest quarter
            m.SetFloat("_Glossiness", 0f);
            return m;
        }

        static Material TopRows(Material painting, Texture2D tex)
        {
            var m = new Material(painting);
            try
            {
                const int cols = 8;
                int band = Mathf.Max(1, tex.height / 25), y0 = tex.height - band;
                var px = tex.GetPixels(0, y0, tex.width, band);
                var g = new Texture2D(cols, 1, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
                for (int c = 0; c < cols; c++)
                {
                    Color sum = Color.black; int n = 0;
                    int x0 = c * tex.width / cols, x1 = (c + 1) * tex.width / cols;
                    for (int y = 0; y < band; y++) for (int x = x0; x < x1; x++) { sum += px[y * tex.width + x]; n++; }
                    var col = sum / Mathf.Max(1, n); col.a = 1f;
                    g.SetPixel(c, 0, col);
                }
                g.Apply();
                m.mainTexture = g; m.mainTextureScale = Vector2.one; m.mainTextureOffset = Vector2.zero;
            }
            catch (UnityException)
            {
                m.mainTexture = null; m.color = AverageColour(tex, 0.02f, 0.06f);
            }
            return m;
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
            var above = TopRows(mat, tex);
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

        // User (2026-11-04): «توپ وقتی می‌خوره به شیشه اون بخشی که توپ خورده نمی‌شکنه، یه بخش دیگه‌ش می‌شکنه». The painted
        // panes have their hole in a fixed place, so at the moment of the break the painting is picked and moved so that
        // its hole lands where the ball hit: low hits get the low hole (pane_1, mirrored for the right half), others the
        // central one (pane_0) slid up or down; mirroring moves the hole sideways.
        static readonly Vector2[] PaneHoleUV = { new Vector2(0.45f, 0.5f), new Vector2(0.3f, 0.24f), new Vector2(0.5f, 0.5f) };
        void HoleWhereHit(int window, Vector3 point)
        {
            var r = windowRenderers[window];
            var p0 = Resources.Load<Texture2D>("Art/Broken/pane_0");
            if (r == null || p0 == null) return;
            Vector3 c = r.transform.position, sz = r.transform.lossyScale;
            float hx = Mathf.Clamp01((point.x - c.x) / Mathf.Max(0.01f, sz.x) + 0.5f);
            float hy = Mathf.Clamp01((point.y - c.y) / Mathf.Max(0.01f, sz.y) + 0.5f);
            int v = hy < 0.38f ? 1 : (window % 2 == 0 ? 0 : 2);
            var tex = Resources.Load<Texture2D>("Art/Broken/pane_" + v);
            if (tex == null) return;
            var m = new Material(r.sharedMaterial) { mainTexture = tex };
            float aspect = sz.x / Mathf.Max(0.01f, sz.y), ta = tex.width / (float)tex.height;
            float sx = 1f, sy = 1f;
            if (aspect > ta) sy = ta / aspect; else sx = aspect / ta;
            // mirror when the hit is on the other side of the hole
            bool flip = (hx - 0.5f) * (PaneHoleUV[v].x - 0.5f) < 0f && Mathf.Abs(hx - 0.5f) > 0.08f;
            float holeU = flip ? 1f - PaneHoleUV[v].x : PaneHoleUV[v].x;
            float offX = Mathf.Clamp(holeU - hx * sx, 0f, 1f - sx);
            float offY = Mathf.Clamp(PaneHoleUV[v].y - hy * sy, 0f, 1f - sy);
            if (flip) { m.mainTextureScale = new Vector2(-sx, sy); m.mainTextureOffset = new Vector2(1f - offX, offY); }
            else { m.mainTextureScale = new Vector2(sx, sy); m.mainTextureOffset = new Vector2(offX, offY); }
            r.material = m;
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
            StartCoroutine(PaneFlash(windowRenderers[i].transform.position, windowRenderers[i].transform.lossyScale));
            windowRenderers[i].enabled = true; // the pane shows the dark room with a web of cracks (its material is set at build time)
            if (ownPanes) OwnHoleWhereHit(i, point);
            else if (duel && !paintedBroken) HoleWhereHit(i, point);
            // User (2026-11-05): «با توجه به این که دیگه خود پنجره حالت شکسته‌ش هست اون پرده و گلدون افتادن و گربه و
            // پیرزن حذف شه، فقط و فقط پنجره‌ای که دو امتیازی هست … پیرزن … سرش رو از پنجره بیاره بیرون فحش بده». A house
            // painted twice shows the painter's own smashed window and nothing else moves; the 2-point window gets the granny.
            if (paintedBroken)
            {
                // User (2026-11-08): «با شکستن هر چندتا پنجره بیاد بیرون و فحش بده. با شکستن تک تک پنجره‌ها فحش رو نداشته
                // باشیم». A resident leans out every 2-4 broken windows of the round (the first after 2), never on each one;
                // the curse voice is tied to the same moment (GameEvents.ResidentShouts).
                brokenThisRound++;
                if (brokenThisRound >= nextResidentAt)
                {
                    nextResidentAt = brokenThisRound + Random.Range(2, 5);
                    var who = PickResident();
                    CurseVoice.ResidentIsFemale = who != null && FemaleResidents.Contains(who.name); // voice matches the face
                    GameEvents.RaiseResidentShouts(i, point);
                    StartCoroutine(GrannyCurses(windowRenderers[i].transform.position, windowRenderers[i].transform.lossyScale, who));
                }
            }
            else if (game != null) WakeDetails(i);
            else WindowLife(i, windowRenderers[i].transform.position, windowRenderers[i].transform.lossyScale);
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
        // ⚠️ until 2026-11-04 any window could throw out an orange cat. User: «پنجره‌ای که پشتش گربه نیست می‌خوره زمین گربه
        // ازش … پشت پنجره گربه خاکستریه ولی گربه‌ای که می‌افته زمین نارنجیه». Now only what the painting shows behind
        // that window comes out (the house's details: the grey cat of window 8, the pots on the sills), else nothing.
        void WindowLife(int window, Vector3 at, Vector3 size)
        {
            bool cat = false, pot = false;
            var house = duelGame != null ? duelGame.house : null;
            if (house != null && house.details != null)
                foreach (var d in house.details)
                    if (d.window == window) { if (d.kind == DetailKind.Cat) cat = true; if (d.kind == DetailKind.Pot) pot = true; }
            // a pot cut out of the painting itself (house 6 onwards): that exact pot falls
            if (house != null && house.details != null)
                foreach (var d in house.details)
                    if (d.window == window && d.kind == DetailKind.Pot && d.pixel.width > 0f && paintedTex != null)
                    {
                        var sr = Cutout(d, -0.08f, out Rect pix);
                        PatchBehind(pix);
                        StartCoroutine(PotFalls(at, size, sr.transform));
                        return;
                    }
            // User (2026-11-07): «پرده‌ی سفیدی که جدا طراحی کرده بودیم … اصلاً زیبا نیست. کلا برش داریم». No curtain any more:
            // a window with nothing painted behind it just breaks.
            if (cat) StartCoroutine(CatLeap(at, size));
            else if (pot && Random.value < 0.6f) StartCoroutine(PotFalls(at, size));
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

        /// <summary>
        /// The neighbour cut out of her painting (Art/Poison/granny: only the woman, no frame, so the window itself is not
        /// changed) rises in the smashed window, shakes her fist, a curse bubble pops over her head, then she ducks back.
        /// </summary>
        /// <summary>Research proposal 18: a white flash on the pane for ~120 ms of real time (the hit stop slows game time).</summary>
        IEnumerator PaneFlash(Vector3 at, Vector3 size)
        {
            var q = GameObject.CreatePrimitive(PrimitiveType.Quad); Strip(q);
            q.transform.position = at + new Vector3(0f, 0f, -0.05f);
            q.transform.localScale = new Vector3(size.x * 1.08f, size.y * 1.08f, 1f);
            var m = new Material(Shader.Find("Sprites/Default")); q.GetComponent<Renderer>().sharedMaterial = m;
            for (float t = 0f; t < 0.12f; t += Time.unscaledDeltaTime) { m.color = new Color(1f, 1f, 0.95f, 0.85f * (1f - t / 0.12f)); yield return null; }
            Destroy(q);
        }

        int brokenThisRound, nextResidentAt = 2, lastResident = -1;
        void OnRoundStarted() { brokenThisRound = 0; nextResidentAt = 2; }

        /// <summary>The residents who lean out: Art/Poison/resident_*.png (the granny is granny2); never the same one twice running.</summary>
        Texture2D PickResident()
        {
            var all = new List<Texture2D>(Resources.LoadAll<Texture2D>("Art/Poison"));
            all.RemoveAll(t => !(t.name.StartsWith("resident_") || t.name == "granny2"));
            if (all.Count == 0) return null;
            int k = all.Count == 1 ? 0 : Random.Range(0, all.Count - 1);
            if (all.Count > 1 && k >= lastResident) k++;
            lastResident = k;
            return all[k];
        }

        // granny2 (red scarf), resident_03 (towel and face mask), resident_05 (headscarf, hands on cheeks) are women; the rest are
        // men and the boy (cartoon set 2026-11-13: 01 boy, 02 beads, 04 cane, 06 slipper)
        static readonly HashSet<string> FemaleResidents = new HashSet<string> { "granny2", "resident_03", "resident_05" };

        IEnumerator GrannyCurses(Vector3 at, Vector3 size, Texture2D chosen = null)
        {
            // User (2026-11-08): «این پیرزنه اصلاً به آرت فعلی نمی‌خوره». The old cartoon cut-out (Art/Poison/granny) is
            // retired; residents painted in the facades' style live in Art/Poison (granny2, resident_*; prompts in
            // docs/ART_SPEC.fa.md). Until one exists nothing is drawn.
            var tex = chosen != null ? chosen : PickResident();
            if (tex == null) yield break;
            yield return new WaitForSeconds(0.35f);
            var go = new GameObject("Granny");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0f), 100f);
            float h = size.y * 0.8f;
            float k = h / sr.sprite.bounds.size.y;
            if (sr.sprite.bounds.size.x * k > size.x * 0.9f) k = size.x * 0.9f / sr.sprite.bounds.size.x; // never wider than the window
            Vector3 bottom = new Vector3(at.x, at.y - size.y * 0.5f, at.z - 0.03f);
            go.transform.position = bottom;
            Transform bubble = null;
            var curse = Resources.Load<Texture2D>("Art/Icons/curse");
            // rise: grows upward from the sill
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.35f)
            {
                float e = 1f - (1f - t) * (1f - t);
                go.transform.localScale = new Vector3(k, k * e, k);
                yield return null;
            }
            go.transform.localScale = Vector3.one * k;
            if (curse != null)
            {
                var b = new GameObject("Curse").AddComponent<SpriteRenderer>();
                b.sprite = Sprite.Create(curse, new Rect(0, 0, curse.width, curse.height), new Vector2(0.5f, 0f), 100f);
                bubble = b.transform;
                float bh = size.y * 0.45f;
                bubble.localScale = Vector3.one * (bh / b.sprite.bounds.size.y);
                bubble.position = new Vector3(at.x + size.x * 0.35f, at.y + size.y * 0.35f, at.z - 0.05f);
            }
            // fist shaking and scolding: a quick wobble for two seconds
            for (float t = 0f; t < 2.0f; t += Time.deltaTime)
            {
                go.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 18f) * 4f);
                if (bubble != null) bubble.localScale *= 1f + Mathf.Sin(t * 12f) * 0.004f;
                yield return null;
            }
            if (bubble != null) Destroy(bubble.gameObject);
            go.transform.rotation = Quaternion.identity;
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.3f)
            {
                go.transform.localScale = new Vector3(k, k * (1f - t), k);
                yield return null;
            }
            Destroy(go);
        }

        IEnumerator CatLeap(Vector3 at, Vector3 size)
        {
            yield return new WaitForSeconds(0.25f);
            var t = FxSprite("fx_cat_grey", 0.45f) ?? FxSprite("fx_cat", 0.45f); // the cat in the painting is grey
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

        /// <summary>
        /// Where a pot was cut out of the painting, the painting still shows it: cover that spot with each row's colour
        /// taken just left and right of the pot (the sill and the wall are flat there), blended across. The upper part,
        /// over the glass, is hidden by the broken pane anyway.
        /// </summary>
        void PatchBehind(Rect pix)
        {
            int x0 = Mathf.Clamp(Mathf.RoundToInt(pix.xMin), 3, paintedTex.width - 4), x1 = Mathf.Clamp(Mathf.RoundToInt(pix.xMax), x0 + 1, paintedTex.width - 4);
            int yb = Mathf.Clamp(Mathf.RoundToInt(paintedTex.height - pix.yMax), 0, paintedTex.height - 2);
            int h = Mathf.Clamp(Mathf.RoundToInt(pix.height), 1, paintedTex.height - yb), w = x1 - x0;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            var px = new Color[w * h];
            for (int y = 0; y < h; y++)
            {
                Color l = paintedTex.GetPixel(x0 - 3, yb + y), r = paintedTex.GetPixel(x1 + 3, yb + y);
                for (int x = 0; x < w; x++) px[y * w + x] = Color.Lerp(l, r, (x + 0.5f) / w);
            }
            tex.SetPixels(px); tex.Apply();
            var go = new GameObject("Patch");
            go.transform.SetParent(detailRoot, false);
            float wx = (pix.center.x / paintedTex.width - 0.5f) * paintedW, wy = (1f - pix.center.y / paintedTex.height) * paintedH;
            go.transform.localPosition = new Vector3(wx, wy, -0.005f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), paintedTex.width / paintedW);
        }

        IEnumerator PotFalls(Vector3 at, Vector3 size, Transform painted = null)
        {
            var t = painted != null ? painted : FxSprite("fx_pot", 0.35f);
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
            Vector3 p = painted != null ? painted.position : at + new Vector3(side * size.x * 0.22f, -size.y * 0.5f + 0.17f, -0.12f);
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
            if (cam == null) return;
            // ⚠️ the duel camera does not follow the player, and this early return used to skip the shake too: the small
            // flinch on a wall hit was gone (user 2026-11-05: «لرزش ریز برخورد توپ با دیوار هم به اشتباه حذف شده»).
            if (!cameraFollowsPlayer || foot == null)
            {
                cam.transform.position = cameraOffset + (shake != null ? shake.Offset : Vector3.zero);
                return;
            }
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

            if (showStart && duel && showHeroes)
            {
                HeroesScreen(u, big);
            }
            else if (showStart && duel && showShop)
            {
                ShopScreen(u, big);
            }
            else if (showStart && duel)
            {
                HomeScreen(u, big); // its own full-screen backdrop: the street behind it is not shown
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
        // User (2026-11-05, with a screenshot of the reference game's lobby): «کاور خود بازی قبل از شروعه نباید بک گراندش
        // این باشه، باید مثل پسرخوانده باشه، یسری دکمه که کارهای مختلف توش میکنی». The lobby is its own screen now: a deep
        // painting over the whole street (see HomeBackdrop), a top bar of painted planks (shards, curses), the heroes card
        // (the kids in battle, on a ribbon with the game's name), the two big painted buttons, the bag rack
        // in a panel, and side buttons for what comes next (missions, heroes, shop, clan) marked «soon» instead of hidden.
        void HomeScreen(float u, GUIStyle big)
        {
            HomeBackdrop();
            // the loot of the bag just opened covers the lobby until tapped; nothing under it may take the tap
            if (lootShown) { LootReveal(u); return; }
            float pad = 14 * u, top = pad + SafeTop();
            var num = new GUIStyle(big) { fontSize = Mathf.RoundToInt(28 * u), alignment = TextAnchor.MiddleCenter };
            var small = new GUIStyle(num) { fontSize = Mathf.RoundToInt(20 * u) };

            // top bar: shards on the right (first for a right-to-left reader), curses next to them, the title on the left
            float barH = 58 * u, barW = Mathf.Min(250 * u, Screen.width * 0.2f);
            var shardsR = new Rect(Screen.width - pad - barW, top, barW, barH);
            var cursesR = new Rect(shardsR.x - 12 * u - barW, top, barW, barH);
            Plank(shardsR); Plank(cursesR);
            DuelHud.DrawIcon(new Rect(shardsR.xMax - barH * 0.95f, top + barH * 0.12f, barH * 0.76f, barH * 0.76f), "shards", Color.white);
            GUI.Label(new Rect(shardsR.x + 10 * u, top, shardsR.width - barH, barH), Ar.N(duelGame.ShardBank), num);
            DuelHud.DrawIcon(new Rect(cursesR.xMax - barH * 0.95f, top + barH * 0.12f, barH * 0.76f, barH * 0.76f), "curse", Color.white);
            GUI.Label(new Rect(cursesR.x + 10 * u, top, cursesR.width - barH, barH), Ar.N(duelGame.Curses[0]), num);
            var titleR = new Rect(pad, top, Mathf.Min(330 * u, Screen.width * 0.3f), barH);
            Plank(titleR);
            GUI.Label(titleR, Ar.T(Ar.Title), num);
            // research proposal 18: the buzz on a broken window can be switched off
            var vibR = new Rect(titleR.xMax + 12 * u, top, Mathf.Min(170 * u, Screen.width * 0.14f), barH);
            Plank(vibR);
            if (GUI.Button(vibR, Ar.T(KickFeel.VibrateOn ? Ar.VibrateOn : Ar.VibrateOff), small)) { KickFeel.VibrateOn = !KickFeel.VibrateOn; touch?.IgnoreFor(0.4f); }
            // research proposal 25: «كيسك جاهز!» on the phone while the game is closed; switched off here
            var notR = new Rect(vibR.xMax + 12 * u, top, vibR.width, barH);
            Plank(notR);
            if (GUI.Button(notR, Ar.T(Sheytoonak.Gameplay.Store.BagNotifier.On ? Ar.NotifyOn : Ar.NotifyOff), small)) { Sheytoonak.Gameplay.Store.BagNotifier.On = !Sheytoonak.Gameplay.Store.BagNotifier.On; touch?.IgnoreFor(0.4f); }

            // arena card in the middle
            float cw = Mathf.Min(430 * u, Screen.width * 0.34f), ch = cw * 0.56f;
            var card = new Rect((Screen.width - cw) * 0.5f, top + barH + 16 * u, cw, ch);
            var kitPanel = DuelHud.Kit("panel");
            if (kitPanel != null) GUI.DrawTexture(card, kitPanel, ScaleMode.StretchToFill);
            // User (2026-11-07): «عکسی هم که هست عکس خونه نباشه. عکس قهرمان‌ها باشه در حال نبرد» — Art/UI/heroes_battle.png
            // when painted, the splash painting (the kids with the ball) until then. The house is never shown here.
            if (!heroesCardLooked) { heroesCardLooked = true; heroesCardTex = Resources.Load<Texture2D>("Art/UI/heroes_battle") ?? Resources.Load<Texture2D>("Art/UI/splash"); }
            if (heroesCardTex != null)
            {
                var inner = new Rect(card.x + cw * 0.05f, card.y + ch * 0.08f, cw * 0.9f, ch * 0.84f);
                float ta = heroesCardTex.width / (float)heroesCardTex.height, ia = inner.width / inner.height;
                var tc = ia > ta ? new Rect(0f, (1f - ta / ia) * 0.5f, 1f, ta / ia) : new Rect((1f - ia / ta) * 0.5f, 0f, ia / ta, 1f);
                GUI.DrawTextureWithTexCoords(inner, heroesCardTex, tc);
            }
            var ribbon = DuelHud.Kit("ribbon");
            var ribR = new Rect(card.center.x - cw * 0.42f, card.yMax - ch * 0.2f, cw * 0.84f, cw * 0.84f * 246f / 969f);
            if (ribbon != null) GUI.DrawTexture(ribR, ribbon, ScaleMode.StretchToFill);
            var ink = new GUIStyle(small) { fontSize = Mathf.RoundToInt(22 * u) }; ink.normal.textColor = new Color(0.35f, 0.18f, 0.08f);
            GUI.Label(new Rect(ribR.x, ribR.y + ribR.height * 0.05f, ribR.width, ribR.height * 0.6f), Ar.T(Ar.Title), ink);

            // the two big buttons under the card: duel (orange) on the right, the 12/3 challenge (blue) on the left
            float bw = Mathf.Min(230 * u, Screen.width * 0.18f), bh = bw * 0.62f, by = ribR.yMax + 18 * u;
            var duelR = new Rect(Screen.width * 0.5f + 10 * u, by, bw, bh);
            var chalR = new Rect(Screen.width * 0.5f - 10 * u - bw, by, bw, bh);
            if (BigButton(duelR, "btn_orange", "duel", "مبارزة", true, u)) StartGameFromScript();
            bool can = duelGame.CanEnterChallenge;
            if (BigButton(chalR, "btn_blue", "trophy", "تحدّي", can, u))
            {
                showStart = false; endTitle = null; touch?.IgnoreFor(0.5f); duelGame.PlayChallenge();
            }
            float sy = by + bh + 6 * u;
            var run = duelGame.Challenge;
            if (duelGame.ChallengeActive)
            {
                DuelHud.ChallengeStrip(new Rect(chalR.x - 20 * u, sy, chalR.width + 40 * u, 34 * u), run, u);
                var left = run.EndsUtc - System.DateTime.UtcNow;
                DuelHud.DrawIcon(new Rect(chalR.x - 150 * u, sy + 3 * u, 28 * u, 28 * u), "clock", Color.white);
                GUI.Label(new Rect(chalR.x - 120 * u, sy, 100 * u, 34 * u), Ar.N(Mathf.Max(0, (int)left.TotalHours)) + ":" + Ar.N(Mathf.Max(0, left.Minutes)).PadLeft(2, '٠'), small);
            }
            else
            {
                DuelHud.DrawIcon(new Rect(chalR.center.x - 36 * u, sy, 30 * u, 30 * u), "shards", can ? Color.white : new Color(1f, 0.4f, 0.4f));
                GUI.Label(new Rect(chalR.center.x - 4 * u, sy, 90 * u, 30 * u), Ar.N(duelGame.ChallengeFree ? 0 : Sheytoonak.Core.Challenge.ChallengeRun.DefaultEntry), new GUIStyle(small) { alignment = TextAnchor.MiddleLeft });
            }

            // side buttons: what is planned (docs/RESEARCH.fa.md queue), honest about not being there yet
            float sbw = Mathf.Min(200 * u, Screen.width * 0.15f), sbh = sbw * 0.62f;
            SideSoon(new Rect(pad, top + barH + 30 * u, sbw, sbh), "btn_green", "pick", "مهمّات", u);
            // the heroes screen: levels of every poison and antidote (SCENARIO §8). A dot when something can be upgraded now.
            var heroesR = new Rect(Screen.width - pad - sbw, top + barH + 30 * u, sbw, sbh);
            if (BigButton(heroesR, "btn_red", "duel", Ar.Heroes, true, u)) { showHeroes = true; heroesScroll = Vector2.zero; touch?.IgnoreFor(0.4f); }
            if (AnyUpgradeReady())
            {
                var o = GUI.color; GUI.color = new Color(0.95f, 0.2f, 0.15f);
                GUI.DrawTexture(new Rect(heroesR.xMax - 26 * u, heroesR.y - 6 * u, 26 * u, 26 * u), DotTex()); GUI.color = o;
            }
            // the shop (2026-11-11): a red dot while today's free pile is still there
            var shopR = new Rect(pad, Screen.height - sbh - pad, sbw, sbh);
            if (BigButton(shopR, "btn_orange", "shards", "دكّان", true, u)) { showShop = true; shopScroll = Vector2.zero; touch?.IgnoreFor(0.4f); }
            if (!duelGame.Shop.Bought(DuelManager.ShopDay, 0))
            {
                var o = GUI.color; GUI.color = new Color(0.95f, 0.2f, 0.15f);
                GUI.DrawTexture(new Rect(shopR.xMax - 26 * u, shopR.y - 6 * u, 26 * u, 26 * u), DotTex()); GUI.color = o;
            }
            SideSoon(new Rect(Screen.width - pad - sbw, Screen.height - sbh - pad, sbw, sbh), "btn_blue", "trophy", "الحارة", u);

            // the bag rack sits in a dark panel along the bottom
            float sw = Mathf.Min(105 * u, (Screen.width - 80 * u) / 4f), gap = 14 * u, total = sw * 4 + gap * 3;
            var rackBg = new Rect((Screen.width - total) * 0.5f - 18 * u, Screen.height - sw - 44 * u - 14 * u, total + 36 * u, sw + 54 * u);
            if (kitPanel != null) GUI.DrawTexture(rackBg, kitPanel, ScaleMode.StretchToFill);
            BagRack(u);
        }

        static Texture2D homeBg, heroesCardTex; static bool homeBgLooked, heroesCardLooked;

        // ------------------------------------------------------------------ heroes: levels (SCENARIO §8)

        bool showHeroes; Vector2 heroesScroll;
        bool showShop; Vector2 shopScroll;
        public void ShowShopFromScript() { showStart = true; showHeroes = false; showShop = true; }
        public void ScrollShopForShot(float y) { shopScroll = new Vector2(0f, y); }
        /// <summary>CI only: the loot reveal waits for a tap that never comes in the screenshot run.</summary>
        public void DismissLootForShot() { lootShown = false; }

        /// <summary>
        /// The shop (user 2026-11-11, from a recording of the reference game: «بخش دکان رو از روی ویدیو الهام بگیری»). Two
        /// panels, each a painted frame with a title, like the reference: «عروض اليوم» -- six piles of one tool's cards
        /// (×N, the tool's icon, its progress toward the next level, the price in shards, the first one free), a «sold»
        /// ribbon once bought and the time until they change at midnight -- and «أكياس» -- bags for shards that open at
        /// once. Buying shows the same card reveal as a bag. No real money and no ads (SCENARIO §13).
        /// </summary>
        void ShopScreen(float u, GUIStyle big)
        {
            HomeBackdrop();
            if (lootShown) { LootReveal(u); return; }
            if (!shopNoteHooked) { shopNoteHooked = true; duelGame.ShopNote += m => { shopNote = m; shopNoteUntil = Time.time + 1.8f; }; }
            float pad = 14 * u, top = pad + SafeTop(), barH = 58 * u;
            var num = new GUIStyle(big) { fontSize = Mathf.RoundToInt(26 * u), alignment = TextAnchor.MiddleCenter };
            var small = new GUIStyle(num) { fontSize = Mathf.RoundToInt(18 * u), fontStyle = FontStyle.Normal };
            var backR = new Rect(pad, top, Mathf.Min(170 * u, Screen.width * 0.15f), barH);
            Plank(backR);
            if (GUI.Button(backR, Ar.T("رجوع"), num)) { showShop = false; touch?.IgnoreFor(0.4f); return; }
            // shards and lira on the right, like the reference's coins and gems
            float barW = Mathf.Min(230 * u, Screen.width * 0.18f);
            var shardsR = new Rect(Screen.width - pad - barW, top, barW, barH);
            var liraR = new Rect(shardsR.x - 10 * u - barW, top, barW, barH);
            Plank(shardsR); Plank(liraR);
            DuelHud.DrawIcon(new Rect(shardsR.xMax - barH * 0.95f, top + barH * 0.12f, barH * 0.76f, barH * 0.76f), "shards", Color.white);
            GUI.Label(new Rect(shardsR.x + 10 * u, top, shardsR.width - barH, barH), Ar.N(duelGame.ShardBank), num);
            LiraCoin(new Rect(liraR.xMax - barH * 0.9f, top + barH * 0.15f, barH * 0.7f, barH * 0.7f), u);
            GUI.Label(new Rect(liraR.x + 10 * u, top, liraR.width - barH, barH), Ar.N(duelGame.LiraBank), num);
            var titleR = new Rect(backR.xMax + 12 * u, top, Mathf.Min(260 * u, liraR.x - backR.xMax - 24 * u), barH);
            Plank(titleR); GUI.Label(titleR, Ar.T("دكّان"), num);

            float areaY = top + barH + 12 * u, areaH = Screen.height - areaY - pad, w = Screen.width - pad * 2 - 4;
            float hSpecial = 230 * u, hOffers = Mathf.Max(areaH * 0.62f, 330 * u), hRow = 250 * u, hCode = 150 * u, g = 14 * u;
            float total = hSpecial + hOffers + hRow * 3 + hCode + g * 5;
            shopScroll = GUI.BeginScrollView(new Rect(pad, areaY, w + 4, areaH), shopScroll, new Rect(0, 0, w, total), GUIStyle.none, GUIStyle.none);
            var title = new GUIStyle(num) { fontSize = Mathf.RoundToInt(28 * u) }; title.normal.textColor = new Color(1f, 0.85f, 0.4f);
            float y = 0f;

            // --- 1. the special offer (half price, three days), like «پیشنهاد پدرخوانده»
            var sr = Panel(new Rect(0, y, w, hSpecial), Ar.T("عرض خاص"), title, u);
            var sp = Sheytoonak.Core.Bags.ShopCatalog.SpecialNow(System.DateTime.UtcNow);
            Countdown(new Rect(sr.x + 24 * u, sr.y + 16 * u, 200 * u, 30 * u), sp.EndsUtc - System.DateTime.UtcNow, small, u);
            float sy = sr.y + 64 * u, sh = sr.height - 80 * u;
            var bagTex = Resources.Load<Texture2D>("Art/Bags/bag_" + (int)sp.Bag);
            var bagR = new Rect(sr.xMax - 40 * u - sh, sy, sh, sh);
            if (bagTex != null) GUI.DrawTexture(bagR, bagTex, ScaleMode.ScaleToFit); else DuelHud.DrawIcon(bagR, "bag", BagTint[(int)sp.Bag]);
            DuelHud.DrawIcon(new Rect(bagR.x - 20 * u - sh * 0.7f, sy + sh * 0.1f, sh * 0.6f, sh * 0.6f), "shards", Color.white);
            GUI.Label(new Rect(bagR.x - 30 * u - sh * 0.8f, sy + sh * 0.72f, sh * 0.8f, 30 * u), Ar.N(sp.Shards), small);
            var tag = new GUIStyle(title) { fontSize = Mathf.RoundToInt(30 * u) }; tag.normal.textColor = new Color(1f, 0.35f, 0.25f);
            GUI.Label(new Rect(sr.x + 30 * u, sy, 300 * u, 44 * u), Ar.T("تخفيض ٥٠٪"), tag);
            var strike = new GUIStyle(small); strike.normal.textColor = new Color(1f, 1f, 1f, 0.6f);
            GUI.Label(new Rect(sr.x + 30 * u, sy + 46 * u, 300 * u, 28 * u), Sheytoonak.Gameplay.Store.StorePayments.PriceText(sp.Sku + "_full", sp.FullCents), strike);
            var old = GUI.color; GUI.color = new Color(1f, 1f, 1f, 0.6f); GUI.DrawTexture(new Rect(sr.x + 110 * u, sy + 60 * u, 140 * u, 2 * u), Texture2D.whiteTexture); GUI.color = old;
            bool spDone = duelGame.SpecialBought(sp);
            var spBtn = new Rect(sr.x + 30 * u, sy + 84 * u, 300 * u, 50 * u);
            MoneyButton(spBtn, spDone ? Ar.T("انشرى") : Sheytoonak.Gameplay.Store.StorePayments.PriceText(sp.Sku, sp.PriceCents), !spDone, u, small);
            if (!spDone && GUI.Button(spBtn, GUIContent.none, GUIStyle.none)) { touch?.IgnoreFor(0.3f); duelGame.BuySpecial(); }
            y += hSpecial + g;

            // --- 2. today's card offers (shards; the first one is the «watch and win» pile)
            var or = Panel(new Rect(0, y, w, hOffers), Ar.T("عروض اليوم"), title, u);
            Countdown(new Rect(or.x + 24 * u, or.y + 16 * u, 200 * u, 30 * u), DuelManager.ShopDay.AddDays(1) - System.DateTime.UtcNow.AddHours(3.5), small, u);
            var offers = duelGame.TodaysOffers;
            int n = offers.Length; float gap = 12 * u, cw = (or.width - 50 * u - gap * (n - 1)) / n, ch = or.height - 90 * u;
            for (int i = 0; i < n; i++)
            {
                var o = offers[i];
                var r = new Rect(or.xMax - 25 * u - (i + 1) * cw - i * gap, or.y + 64 * u, cw, ch);
                bool sold = duelGame.Shop.Bought(DuelManager.ShopDay, i);
                OfferCard(r, o, sold, u, small);
                if (!sold && GUI.Button(r, GUIContent.none, GUIStyle.none))
                {
                    touch?.IgnoreFor(0.3f);
                    int slot = i;
                    if (o.Price == 0) Sheytoonak.Gameplay.Store.RewardedAds.Show(ok => { if (ok) duelGame.BuyOffer(slot); });
                    else if (duelGame.BuyOffer(i) == Sheytoonak.Core.Bags.ShopRules.Reject.NotEnoughShards) { shopNote = "ما معك زجاج كفاية"; shopNoteUntil = Time.time + 1.6f; }
                }
            }
            y += hOffers + g;

            // --- 3. bags: medium and large for shards, huge for lira (open at once)
            var br = Panel(new Rect(0, y, w, hRow), Ar.T("أكياس"), title, u);
            var sizes = new[] { Sheytoonak.Core.Bags.BagSize.Medium, Sheytoonak.Core.Bags.BagSize.Large, Sheytoonak.Core.Bags.BagSize.Huge };
            Row(br, sizes.Length, u, (k, r) =>
            {
                var size = sizes[k];
                var bt = Resources.Load<Texture2D>("Art/Bags/bag_" + (int)size);
                var ir = new Rect(r.center.x - r.height * 0.28f, r.y + 4 * u, r.height * 0.56f, r.height * 0.56f);
                if (bt != null) GUI.DrawTexture(ir, bt, ScaleMode.ScaleToFit); else DuelHud.DrawIcon(ir, "bag", BagTint[(int)size]);
                string nm = size == Sheytoonak.Core.Bags.BagSize.Huge ? "كيس ضخم" : size == Sheytoonak.Core.Bags.BagSize.Large ? "كيس كبير" : "كيس وسط";
                GUI.Label(new Rect(r.x, r.y + r.height * 0.6f, r.width, 26 * u), Ar.T(nm) + "  ×" + Ar.N(Sheytoonak.Core.Bags.BagRules.CardsIn(size)), small);
                // the odds, said up front: N cards of tools picked at random (every tool the same chance) + a shard range
                var odds = new GUIStyle(small) { fontSize = Mathf.RoundToInt(13 * u) }; odds.normal.textColor = new Color(1f, 1f, 1f, 0.75f);
                GUI.Label(new Rect(r.x, r.y + r.height * 0.6f + 24 * u, r.width, 22 * u), Ar.T("كروت عشوائية") + "  " + Ar.N(Sheytoonak.Core.Bags.BagRules.ShardsMinIn(size)) + "–" + Ar.N(Sheytoonak.Core.Bags.BagRules.ShardsMaxIn(size)), odds);
                var pr = new Rect(r.x + 10 * u, r.yMax - 40 * u, r.width - 20 * u, 34 * u);
                if (size == Sheytoonak.Core.Bags.BagSize.Huge)
                {
                    int lp = Sheytoonak.Core.Bags.ShopCatalog.BagLira(size);
                    LiraButton(pr, lp, duelGame.LiraBank >= lp, u, small);
                    if (GUI.Button(r, GUIContent.none, GUIStyle.none)) { touch?.IgnoreFor(0.3f); duelGame.BuyBagWithLira(size); }
                }
                else
                {
                    int price = Sheytoonak.Core.Bags.ShopRules.BagPrice(size);
                    PriceButton(pr, price, duelGame.ShardBank >= price, u, small);
                    if (GUI.Button(r, GUIContent.none, GUIStyle.none)) { touch?.IgnoreFor(0.3f); if (!duelGame.BuyBag(size)) { shopNote = "ما معك زجاج كفاية"; shopNoteUntil = Time.time + 1.6f; } }
                }
            });
            y += hRow + g;

            // --- 4. lira for money (through the store; «soon» until one is connected)
            var lr = Panel(new Rect(0, y, w, hRow), Ar.T("ليرات"), title, u);
            var packs = Sheytoonak.Core.Bags.ShopCatalog.LiraPacks;
            Row(lr, packs.Length, u, (k, r) =>
            {
                var pk = packs[k];
                GUI.Label(new Rect(r.x, r.y + 2 * u, r.width, 26 * u), Ar.T(pk.NameAr), small);
                float cs = r.height * 0.36f;
                for (int c = 0; c < Mathf.Min(1 + k, 5); c++) LiraCoin(new Rect(r.center.x - cs * 0.5f + (c - k * 0.5f) * cs * 0.3f, r.y + 32 * u + (c % 2) * 6 * u, cs, cs), u);
                var big2 = new GUIStyle(small) { fontSize = Mathf.RoundToInt(22 * u), fontStyle = FontStyle.Bold }; big2.normal.textColor = new Color(1f, 0.85f, 0.35f);
                GUI.Label(new Rect(r.x, r.y + 40 * u + cs, r.width, 30 * u), Ar.N(pk.Lira), big2);
                var pr = new Rect(r.x + 8 * u, r.yMax - 40 * u, r.width - 16 * u, 34 * u);
                MoneyButton(pr, Sheytoonak.Gameplay.Store.StorePayments.PriceText(pk.Sku, pk.PriceCents), true, u, small);
                if (GUI.Button(r, GUIContent.none, GUIStyle.none)) { touch?.IgnoreFor(0.3f); duelGame.BuyLira(k); }
            });
            y += hRow + g;

            // --- 5. shards for lira
            var zr = Panel(new Rect(0, y, w, hRow), Ar.T("زجاج"), title, u);
            var zp = Sheytoonak.Core.Bags.ShopCatalog.ShardPacks;
            Row(zr, zp.Length, u, (k, r) =>
            {
                var pk = zp[k];
                GUI.Label(new Rect(r.x, r.y + 2 * u, r.width, 26 * u), Ar.T(pk.NameAr), small);
                float cs = r.height * 0.4f;
                DuelHud.DrawIcon(new Rect(r.center.x - cs * 0.5f, r.y + 30 * u, cs, cs), "shards", Color.white);
                GUI.Label(new Rect(r.x, r.y + 36 * u + cs, r.width, 30 * u), Ar.N(pk.Shards), small);
                var pr = new Rect(r.x + 10 * u, r.yMax - 40 * u, r.width - 20 * u, 34 * u);
                LiraButton(pr, pk.Lira, duelGame.LiraBank >= pk.Lira, u, small);
                if (GUI.Button(r, GUIContent.none, GUIStyle.none)) { touch?.IgnoreFor(0.3f); duelGame.BuyShardPack(k); }
            });
            y += hRow + g;

            // --- 6. prize code
            var cr = Panel(new Rect(0, y, w, hCode), Ar.T("كود جائزة"), title, u);
            var field = new GUIStyle(GUI.skin.textField) { fontSize = Mathf.RoundToInt(24 * u), alignment = TextAnchor.MiddleCenter };
            var fr = new Rect(cr.center.x - 60 * u, cr.y + 62 * u, Mathf.Min(360 * u, cr.width * 0.4f), 52 * u);
            prizeCode = GUI.TextField(fr, prizeCode ?? "", 20, field);
            var okR = new Rect(fr.x - 180 * u, fr.y, 160 * u, 52 * u);
            MoneyButton(okR, Ar.T("استعمل"), (prizeCode ?? "").Trim().Length > 0, u, small);
            if (GUI.Button(okR, GUIContent.none, GUIStyle.none) && (prizeCode ?? "").Trim().Length > 0) { touch?.IgnoreFor(0.3f); if (duelGame.RedeemCode(prizeCode)) prizeCode = ""; }
            GUI.EndScrollView();

            if (Time.time < shopNoteUntil && !string.IsNullOrEmpty(shopNote))
            {
                var nr = new Rect(Screen.width * 0.5f - 220 * u, Screen.height * 0.45f, 440 * u, 60 * u);
                old = GUI.color; GUI.color = new Color(0f, 0f, 0f, 0.8f); GUI.DrawTexture(nr, Texture2D.whiteTexture); GUI.color = old;
                GUI.Label(nr, Ar.T(shopNote), num);
            }
        }
        string shopNote, prizeCode; float shopNoteUntil; bool shopNoteHooked;

        /// <summary>A painted frame with a title; returns its rect.</summary>
        static Rect Panel(Rect r, string titleText, GUIStyle title, float u)
        {
            var panel = DuelHud.Kit("panel");
            if (panel != null) GUI.DrawTexture(r, panel, ScaleMode.StretchToFill);
            else { var o = GUI.color; GUI.color = new Color(0f, 0f, 0f, 0.55f); GUI.DrawTexture(r, Texture2D.whiteTexture); GUI.color = o; }
            GUI.Label(new Rect(r.x, r.y + 12 * u, r.width, 40 * u), titleText, title);
            return r;
        }

        /// <summary>Cells of a panel's row, right to left, each with a dark backing.</summary>
        static void Row(Rect panel, int n, float u, System.Action<int, Rect> cell)
        {
            float gap = 14 * u, cw = Mathf.Min(240 * u, (panel.width - 60 * u - gap * (n - 1)) / n), ch = panel.height - 80 * u;
            float x0 = panel.x + (panel.width - (n * cw + (n - 1) * gap)) * 0.5f;
            for (int k = 0; k < n; k++)
            {
                var r = new Rect(x0 + (n - 1 - k) * (cw + gap), panel.y + 60 * u, cw, ch);
                var o = GUI.color; GUI.color = new Color(0f, 0f, 0f, 0.35f); GUI.DrawTexture(r, Texture2D.whiteTexture); GUI.color = o;
                cell(k, r);
            }
        }

        static void Countdown(Rect r, System.TimeSpan left, GUIStyle small, float u)
        {
            DuelHud.DrawIcon(new Rect(r.x, r.y + 2 * u, 26 * u, 26 * u), "clock", Color.white);
            var st = new GUIStyle(small) { alignment = TextAnchor.MiddleLeft };
            string t = left.TotalHours >= 24 ? Ar.T(Ar.N((int)left.TotalDays) + " يوم " + Ar.N(left.Hours) + " ساعة") : Ar.N(Mathf.Max(0, (int)left.TotalHours)) + ":" + Ar.N(Mathf.Max(0, left.Minutes)).PadLeft(2, '٠');
            GUI.Label(new Rect(r.x + 32 * u, r.y, r.width - 32 * u, r.height), t, st);
        }

        /// <summary>The lira coin: Art/Icons/lira when painted, else a gold disc with «ل».</summary>
        static void LiraCoin(Rect r, float u)
        {
            var t = DuelHud.Icon("lira");
            if (t != null) { GUI.DrawTexture(r, t, ScaleMode.ScaleToFit); return; }
            var o = GUI.color; GUI.color = new Color(0.95f, 0.75f, 0.2f); GUI.DrawTexture(r, DotTex()); GUI.color = o;
            var st = new GUIStyle(GUI.skin.label) { font = Ar.Font, fontSize = Mathf.RoundToInt(r.height * 0.6f), alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            st.normal.textColor = new Color(0.45f, 0.25f, 0.05f);
            GUI.Label(r, Ar.T("ل"), st);
        }

        static void LiraButton(Rect r, int price, bool can, float u, GUIStyle small)
        {
            var kit = DuelHud.Kit("btn_green");
            var old = GUI.color; GUI.color = can ? Color.white : new Color(0.6f, 0.6f, 0.6f, 0.9f);
            if (kit != null) GUI.DrawTexture(r, kit, ScaleMode.StretchToFill); else { GUI.color = new Color(0.2f, 0.6f, 0.3f); GUI.DrawTexture(r, Texture2D.whiteTexture); }
            GUI.color = old;
            LiraCoin(new Rect(r.xMax - r.height * 0.95f, r.y + r.height * 0.1f, r.height * 0.8f, r.height * 0.8f), u);
            var st = new GUIStyle(small) { fontSize = Mathf.RoundToInt(16 * u), fontStyle = FontStyle.Bold }; st.normal.textColor = Color.white;
            GUI.Label(new Rect(r.x, r.y, r.width - r.height * 0.8f, r.height), Ar.N(price), st);
        }

        static void MoneyButton(Rect r, string text, bool can, float u, GUIStyle small)
        {
            var kit = DuelHud.Kit("btn_orange");
            var old = GUI.color; GUI.color = can ? Color.white : new Color(0.6f, 0.6f, 0.6f, 0.9f);
            if (kit != null) GUI.DrawTexture(r, kit, ScaleMode.StretchToFill); else { GUI.color = new Color(0.95f, 0.6f, 0.15f); GUI.DrawTexture(r, Texture2D.whiteTexture); }
            GUI.color = old;
            var st = new GUIStyle(small) { fontSize = Mathf.RoundToInt(17 * u), fontStyle = FontStyle.Bold }; st.normal.textColor = Color.white;
            GUI.Label(r, text, st);
        }

        float shopShortUntil;

        void OfferCard(Rect r, Sheytoonak.Core.Bags.DailyOffer o, bool sold, float u, GUIStyle small)
        {
            var old = GUI.color;
            GUI.color = new Color(0.98f, 0.93f, 0.85f, sold ? 0.45f : 1f); GUI.DrawTexture(r, Texture2D.whiteTexture); GUI.color = old;
            bool poison = false; string name = o.ToolId;
            foreach (var h in Sheytoonak.Core.Duel.StarterHeroes.All)
            {
                if (h.Poison.Id == o.ToolId) { poison = true; name = h.Poison.NameAr; }
                else if (h.Antidote.Id == o.ToolId) name = h.Antidote.NameAr;
            }
            float s = Mathf.Min(r.width * 0.75f, r.height * 0.45f);
            DraftScreen.Badge(new Rect(r.center.x - s * 0.5f, r.y + 8 * u, s, s), o.ToolId, poison ? DuelHud.PoisonRed : DuelHud.AntidoteGreen);
            var cnt = new GUIStyle(small) { fontSize = Mathf.RoundToInt(22 * u), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleRight };
            cnt.normal.textColor = new Color(0.2f, 0.75f, 0.25f);
            GUI.Label(new Rect(r.x, r.y + 4 * u, r.width - 8 * u, 28 * u), "×" + Ar.N(o.Count), cnt);
            var ink = new GUIStyle(small) { fontSize = Mathf.RoundToInt(15 * u) }; ink.normal.textColor = new Color(0.3f, 0.16f, 0.08f);
            GUI.Label(new Rect(r.x + 2 * u, r.y + 12 * u + s, r.width - 4 * u, 24 * u), Ar.T(name), ink);
            // progress toward the next level, like the reference's card
            var lv = duelGame.Levels; int need = lv.CardsNeeded(o.ToolId), have = lv.Cards(o.ToolId);
            var bar = new Rect(r.x + 8 * u, r.y + 40 * u + s, r.width - 16 * u, 18 * u);
            GUI.color = new Color(0.1f, 0.12f, 0.2f); GUI.DrawTexture(bar, Texture2D.whiteTexture);
            GUI.color = need > 0 && have + o.Count >= need ? new Color(0.3f, 0.85f, 0.35f) : new Color(0.25f, 0.55f, 0.95f);
            GUI.DrawTexture(new Rect(bar.x, bar.y, need > 0 ? bar.width * Mathf.Clamp01(have / (float)need) : bar.width, bar.height), Texture2D.whiteTexture);
            GUI.color = old;
            GUI.Label(bar, need > 0 ? Ar.N(have) + " / " + Ar.N(need) : Ar.T(Ar.MaxLevel), new GUIStyle(small) { fontSize = Mathf.RoundToInt(13 * u) });
            var pr = new Rect(r.x + 6 * u, r.yMax - 38 * u, r.width - 12 * u, 32 * u);
            if (o.Price == 0)
            {
                var free = new GUIStyle(small) { fontStyle = FontStyle.Bold }; free.normal.textColor = new Color(0.15f, 0.6f, 0.2f);
                GUI.Label(pr, Ar.T(Sheytoonak.Gameplay.Store.RewardedAds.Available ? "شوف واربح" : "ببلاش!"), free);
            }
            else PriceButton(pr, o.Price, duelGame.ShardBank >= o.Price, u, small);
            if (sold)
            {
                // the red «sold» ribbon across the card
                var m = GUI.matrix;
                GUIUtility.RotateAroundPivot(-20f, r.center);
                GUI.color = new Color(0.8f, 0.15f, 0.12f, 0.95f);
                var rib = new Rect(r.x - 10 * u, r.center.y - 18 * u, r.width + 20 * u, 36 * u);
                GUI.DrawTexture(rib, Texture2D.whiteTexture); GUI.color = old;
                var w = new GUIStyle(small) { fontStyle = FontStyle.Bold }; w.normal.textColor = Color.white;
                GUI.Label(rib, Ar.T("انباع"), w);
                GUI.matrix = m;
            }
        }

        static void PriceButton(Rect r, int price, bool can, float u, GUIStyle small)
        {
            var kit = DuelHud.Kit("btn_orange");
            var old = GUI.color; GUI.color = can ? Color.white : new Color(0.6f, 0.6f, 0.6f, 0.9f);
            if (kit != null) GUI.DrawTexture(r, kit, ScaleMode.StretchToFill);
            else { GUI.color = can ? new Color(0.95f, 0.6f, 0.15f) : new Color(0.4f, 0.4f, 0.4f); GUI.DrawTexture(r, Texture2D.whiteTexture); }
            GUI.color = old;
            DuelHud.DrawIcon(new Rect(r.xMax - r.height * 0.95f, r.y + r.height * 0.1f, r.height * 0.8f, r.height * 0.8f), "shards", can ? Color.white : new Color(1f, 0.5f, 0.5f));
            var st = new GUIStyle(small) { fontSize = Mathf.RoundToInt(16 * u), fontStyle = FontStyle.Bold }; st.normal.textColor = Color.white;
            GUI.Label(new Rect(r.x, r.y, r.width - r.height * 0.8f, r.height), Ar.N(price), st);
        }
        public void ShowHeroesFromScript() { showStart = true; showHeroes = true; }

        bool AnyUpgradeReady()
        {
            var lv = duelGame.Levels;
            foreach (var h in Sheytoonak.Core.Duel.StarterHeroes.All)
                if (lv.CanUpgrade(h.Poison.Id, duelGame.ShardBank) == Sheytoonak.Core.Bags.ToolLevels.Reject.None
                    || lv.CanUpgrade(h.Antidote.Id, duelGame.ShardBank) == Sheytoonak.Core.Bags.ToolLevels.Reject.None) return true;
            return false;
        }

        static Texture2D dotTex;
        static Texture2D DotTex()
        {
            if (dotTex != null) return dotTex;
            const int n = 32; dotTex = new Texture2D(n, n, TextureFormat.RGBA32, false);
            for (int y = 0; y < n; y++) for (int x = 0; x < n; x++)
            { float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(n / 2f, n / 2f)); dotTex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(n / 2f - d))); }
            dotTex.Apply(); return dotTex;
        }

        /// <summary>
        /// User (2026-11-09): «از توی کیسه یسری شانس بالقوه برای قوی‌تر کردن هر سم یا پادسم … برای بالفعل کردنش باید خرده
        /// شیشه خرج کنه». One card per hero: the portrait, then its poison (red) and antidote (green), each with its level,
        /// a bar of the cards in hand against the cards the next level needs, and the upgrade button with its shard price
        /// (grey while either is short). No paragraphs: numbers and pictures.
        /// </summary>
        void HeroesScreen(float u, GUIStyle big)
        {
            HomeBackdrop();
            float pad = 14 * u, top = pad + SafeTop(), barH = 58 * u;
            var num = new GUIStyle(big) { fontSize = Mathf.RoundToInt(26 * u), alignment = TextAnchor.MiddleCenter };
            var small = new GUIStyle(num) { fontSize = Mathf.RoundToInt(18 * u), fontStyle = FontStyle.Normal };
            // top bar: back on the left, title in the middle, shards on the right
            var backR = new Rect(pad, top, Mathf.Min(170 * u, Screen.width * 0.15f), barH);
            Plank(backR);
            if (GUI.Button(backR, Ar.T("رجوع"), num)) { showHeroes = false; touch?.IgnoreFor(0.4f); return; }
            float barW = Mathf.Min(250 * u, Screen.width * 0.2f);
            var shardsR = new Rect(Screen.width - pad - barW, top, barW, barH);
            Plank(shardsR);
            DuelHud.DrawIcon(new Rect(shardsR.xMax - barH * 0.95f, top + barH * 0.12f, barH * 0.76f, barH * 0.76f), "shards", Color.white);
            GUI.Label(new Rect(shardsR.x + 10 * u, top, shardsR.width - barH, barH), Ar.N(duelGame.ShardBank), num);
            var titleR = new Rect((Screen.width - 300 * u) * 0.5f, top, 300 * u, barH);
            Plank(titleR); GUI.Label(titleR, Ar.T(Ar.Heroes), num);

            var heroes = Sheytoonak.Core.Duel.StarterHeroes.All;
            int cols = 4, rowsN = (heroes.Length + cols - 1) / cols;
            float areaY = top + barH + 12 * u, areaH = Screen.height - areaY - pad;
            float gap = 12 * u, cw = (Screen.width - pad * 2 - gap * (cols - 1)) / cols, chh = Mathf.Max(areaH / rowsN - gap, 300 * u);
            var view = new Rect(0, 0, Screen.width - pad * 2 - 4, rowsN * (chh + gap));
            heroesScroll = GUI.BeginScrollView(new Rect(pad, areaY, Screen.width - pad * 2, areaH), heroesScroll, view, GUIStyle.none, GUIStyle.none);
            var panel = DuelHud.Kit("panel");
            for (int i = 0; i < heroes.Length; i++)
            {
                // right-to-left: the first hero top right
                int c = cols - 1 - i % cols, r = i / cols;
                var card = new Rect(c * (cw + gap), r * (chh + gap), cw, chh);
                if (panel != null) GUI.DrawTexture(card, panel, ScaleMode.StretchToFill);
                else { var o = GUI.color; GUI.color = new Color(0f, 0f, 0f, 0.55f); GUI.DrawTexture(card, Texture2D.whiteTexture); GUI.color = o; }
                var h = heroes[i];
                float ph = chh * 0.34f;
                DraftScreen.Portrait(new Rect(card.center.x - ph * 0.5f, card.y + 10 * u, ph, ph), h.Id);
                GUI.Label(new Rect(card.x, card.y + 12 * u + ph, card.width, 30 * u), Ar.T(h.NameAr), small);
                float ry = card.y + 46 * u + ph, rh = (card.yMax - ry - 10 * u) / 2f;
                ToolRow(new Rect(card.x + 10 * u, ry, card.width - 20 * u, rh - 6 * u), h.Poison, DuelHud.PoisonRed, u, small);
                ToolRow(new Rect(card.x + 10 * u, ry + rh, card.width - 20 * u, rh - 6 * u), h.Antidote, DuelHud.AntidoteGreen, u, small);
            }
            GUI.EndScrollView();
        }

        void ToolRow(Rect r, Sheytoonak.Core.Duel.Tool t, Color tint, float u, GUIStyle small)
        {
            var lv = duelGame.Levels;
            int level = lv.Level(t.Id), have = lv.Cards(t.Id), need = lv.CardsNeeded(t.Id), price = lv.ShardsNeeded(t.Id);
            float s = Mathf.Min(r.height, 64 * u);
            DraftScreen.Badge(new Rect(r.xMax - s, r.y, s, s), t.Id, tint);
            // level pips under the icon
            for (int k = 0; k < Sheytoonak.Core.Bags.ToolLevels.MaxLevel; k++)
            {
                var o = GUI.color; GUI.color = k < level ? new Color(1f, 0.82f, 0.2f) : new Color(1f, 1f, 1f, 0.25f);
                float d = s / 6f; GUI.DrawTexture(new Rect(r.xMax - s + k * (s / 5f) + (s / 5f - d) * 0.5f, r.y + s + 2 * u, d, d), DotTex()); GUI.color = o;
            }
            float tx = r.x, tw = r.width - s - 8 * u;
            var lab = new GUIStyle(small) { alignment = TextAnchor.MiddleRight };
            GUI.Label(new Rect(tx, r.y, tw, s * 0.45f), Ar.T(t.NameAr + "  " + Ar.Lv(level)), lab);
            if (need == 0) { GUI.Label(new Rect(tx, r.y + s * 0.45f, tw, s * 0.55f), Ar.T(Ar.MaxLevel), lab); return; }
            // the cards bar: latent upgrades in hand / needed for the next level
            var bar = new Rect(tx, r.y + s * 0.5f, tw * 0.55f, s * 0.3f);
            var old = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.55f); GUI.DrawTexture(bar, Texture2D.whiteTexture);
            GUI.color = have >= need ? new Color(0.3f, 0.85f, 0.35f) : new Color(0.35f, 0.6f, 1f);
            GUI.DrawTexture(new Rect(bar.x, bar.y, bar.width * Mathf.Clamp01(have / (float)need), bar.height), Texture2D.whiteTexture);
            GUI.color = old;
            GUI.Label(bar, Ar.N(Mathf.Min(have, need)) + " / " + Ar.N(need), new GUIStyle(small) { fontSize = Mathf.RoundToInt(15 * u) });
            // the upgrade button with its price
            bool can = lv.CanUpgrade(t.Id, duelGame.ShardBank) == Sheytoonak.Core.Bags.ToolLevels.Reject.None;
            var br = new Rect(bar.xMax + 6 * u, r.y + s * 0.4f, tw - bar.width - 6 * u, s * 0.55f);
            var kit = DuelHud.Kit(can ? "btn_green" : "btn_blue");
            GUI.color = can ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.9f);
            if (kit != null) GUI.DrawTexture(br, kit, ScaleMode.StretchToFill);
            else { GUI.color = can ? new Color(0.2f, 0.6f, 0.3f) : new Color(0.3f, 0.3f, 0.3f); GUI.DrawTexture(br, Texture2D.whiteTexture); }
            GUI.color = old;
            DuelHud.DrawIcon(new Rect(br.xMax - br.height * 0.9f, br.y + br.height * 0.1f, br.height * 0.8f, br.height * 0.8f), "shards", duelGame.ShardBank >= price ? Color.white : new Color(1f, 0.45f, 0.45f));
            GUI.Label(new Rect(br.x, br.y, br.width - br.height * 0.8f, br.height), Ar.N(price), new GUIStyle(small) { fontSize = Mathf.RoundToInt(16 * u) });
            if (can && GUI.Button(br, GUIContent.none, GUIStyle.none))
            {
                if (duelGame.UpgradeTool(t.Id) == Sheytoonak.Core.Bags.ToolLevels.Reject.None)
                    UpgradeCelebration.Instance?.Play(t.Id, t.NameAr, lv.Level(t.Id));
                touch?.IgnoreFor(0.3f);
            }
        }
        /// <summary>
        /// User (2026-11-07): «صفحه بک گراند اولیه خیلی زشته و شبیه لوگوی شبکه ۴ه … عوضش کنیم». The generated blue
        /// star carpet is gone. The lobby now sits on a painting: Art/UI/home.png when it exists (prompt in
        /// docs/ART_SPEC.fa.md, «قهرمان‌ها در حال نبرد»), until then the splash painting, darkened so the planks and
        /// buttons stay readable on top.
        /// </summary>
        static void HomeBackdrop()
        {
            if (!homeBgLooked)
            {
                homeBgLooked = true;
                homeBg = Resources.Load<Texture2D>("Art/UI/home") ?? Resources.Load<Texture2D>("Art/UI/splash");
            }
            var all = new Rect(0, 0, Screen.width, Screen.height);
            if (homeBg != null)
            {
                float k = Mathf.Max(Screen.width / (float)homeBg.width, Screen.height / (float)homeBg.height);
                float w = homeBg.width * k, h = homeBg.height * k;
                GUI.DrawTexture(new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h), homeBg, ScaleMode.StretchToFill);
            }
            else { var c = GUI.color; GUI.color = new Color(0.08f, 0.1f, 0.18f); GUI.DrawTexture(all, Texture2D.whiteTexture); GUI.color = c; }
            // darken so the UI reads; the middle a touch lighter than the edges
            var old = GUI.color;
            GUI.color = new Color(0f, 0f, 0.04f, 0.42f); GUI.DrawTexture(all, Texture2D.whiteTexture);
            GUI.color = new Color(0f, 0f, 0f, 0.25f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height * 0.18f), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(0, Screen.height * 0.78f, Screen.width, Screen.height * 0.22f), Texture2D.whiteTexture);
            GUI.color = old;
        }

        static void Plank(Rect r)
        {
            var bar = DuelHud.Kit("bar");
            if (bar != null) GUI.DrawTexture(r, bar, ScaleMode.StretchToFill);
            else { var old = GUI.color; GUI.color = new Color(0f, 0f, 0f, 0.6f); GUI.DrawTexture(r, Texture2D.whiteTexture); GUI.color = old; }
        }

        static bool BigButton(Rect r, string kitName, string icon, string label, bool enabled, float u)
        {
            var tex = DuelHud.Kit(kitName);
            var old = GUI.color;
            GUI.color = enabled ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.9f);
            if (tex != null) GUI.DrawTexture(r, tex, ScaleMode.StretchToFill);
            GUI.color = old;
            float isz = r.height * 0.5f;
            DuelHud.DrawIcon(new Rect(r.center.x - isz * 0.5f, r.y + r.height * 0.1f, isz, isz), icon, enabled ? Color.white : new Color(0.8f, 0.8f, 0.8f));
            var st = new GUIStyle(GUI.skin.label) { font = Ar.Font, fontSize = Mathf.RoundToInt(r.height * 0.22f), alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            var shadow = new GUIStyle(st); shadow.normal.textColor = new Color(0f, 0f, 0f, 0.55f);
            st.normal.textColor = Color.white;
            var lr = new Rect(r.x, r.y + r.height * 0.58f, r.width, r.height * 0.3f);
            GUI.Label(new Rect(lr.x + 2, lr.y + 2, lr.width, lr.height), Ar.T(label), shadow);
            GUI.Label(lr, Ar.T(label), st);
            return enabled && GUI.Button(r, GUIContent.none, GUIStyle.none);
        }

        float soonUntil; Rect soonAt;
        void SideSoon(Rect r, string kitName, string icon, string label, float u)
        {
            if (BigButton(r, kitName, icon, label, true, u)) { soonUntil = Time.time + 1.5f; soonAt = r; touch?.IgnoreFor(0.3f); }
            var tag = new GUIStyle(GUI.skin.label) { font = Ar.Font, fontSize = Mathf.RoundToInt(16 * u), alignment = TextAnchor.MiddleCenter };
            tag.normal.textColor = new Color(1f, 0.92f, 0.6f);
            bool flash = Time.time < soonUntil && soonAt == r;
            var old = GUI.color; GUI.color = new Color(0f, 0f, 0f, flash ? 0.85f : 0.55f);
            var tr = new Rect(r.center.x - 40 * u, r.y - 10 * u, 80 * u, 24 * u);
            GUI.DrawTexture(tr, Texture2D.whiteTexture); GUI.color = old;
            GUI.Label(tr, Ar.T("قريباً"), tag);
        }

        /// <summary>
        /// User (2026-11-10): «کیسه رو که باز می‌کنی باید آیکون‌هایی که بردی رو نشونت بده، در قالب یه کارتی چیزی. الآن من نمی‌فهمم
        /// چی بردم». Each card turns over one after another: the shards first, then one card per tool with its icon, its
        /// name, how many came, and how far that tool now is toward its next level. It stays until tapped.
        /// </summary>
        void LootReveal(float u)
        {
            var all = new Rect(0, 0, Screen.width, Screen.height);
            var old = GUI.color; GUI.color = new Color(0f, 0f, 0f, 0.82f); GUI.DrawTexture(all, Texture2D.whiteTexture); GUI.color = old;
            var title = new GUIStyle(GUI.skin.label) { font = Ar.Font, fontSize = Mathf.RoundToInt(34 * u), alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            title.normal.textColor = new Color(1f, 0.85f, 0.4f);
            var lab = new GUIStyle(title) { fontSize = Mathf.RoundToInt(20 * u), fontStyle = FontStyle.Normal }; lab.normal.textColor = Color.white;
            var big = new GUIStyle(title) { fontSize = Mathf.RoundToInt(30 * u) }; big.normal.textColor = Color.white;
            GUI.Label(new Rect(0, Screen.height * 0.1f, Screen.width, 50 * u), Ar.T("شو طلع من الكيس"), title);

            // group the cards by tool, in the order they came
            var ids = new System.Collections.Generic.List<string>(); var counts = new System.Collections.Generic.Dictionary<string, int>();
            foreach (var id in lootCards) { if (!counts.ContainsKey(id)) { counts[id] = 0; ids.Add(id); } counts[id]++; }
            int shardCards = lootShards > 0 ? 1 : 0; // a shop pile brings no shards: no empty shards card
            int n = ids.Count + shardCards;
            float cw = Mathf.Min(190 * u, (Screen.width - 60 * u) / Mathf.Max(1, n) - 14 * u), ch = cw * 1.45f, gap = 14 * u;
            float x0 = (Screen.width - (n * cw + (n - 1) * gap)) * 0.5f, y = Screen.height * 0.5f - ch * 0.5f;
            var panel = DuelHud.Kit("panel");
            for (int k = 0; k < n; k++)
            {
                // flips in one by one (0.25 s apart), right to left
                float t = Mathf.Clamp01((Time.time - lootAt - k * 0.25f) / 0.3f);
                if (t <= 0f) continue;
                var r = new Rect(x0 + (n - 1 - k) * (cw + gap), y, cw, ch);
                var rr = new Rect(r.center.x - r.width * 0.5f * t, r.y, r.width * t, r.height);
                if (panel != null) GUI.DrawTexture(rr, panel, ScaleMode.StretchToFill);
                else { GUI.color = new Color(0.2f, 0.18f, 0.16f); GUI.DrawTexture(rr, Texture2D.whiteTexture); GUI.color = old; }
                if (t < 1f) continue;
                float s = cw * 0.55f;
                if (k == 0 && shardCards == 1)
                {
                    DuelHud.DrawIcon(new Rect(r.center.x - s * 0.5f, r.y + ch * 0.14f, s, s), "shards", Color.white);
                    GUI.Label(new Rect(r.x, r.y + ch * 0.6f, r.width, 40 * u), "+" + Ar.N(lootShards), big);
                    GUI.Label(new Rect(r.x, r.y + ch * 0.75f, r.width, 30 * u), Ar.T("زجاج"), lab);
                    continue;
                }
                string tid = ids[k - shardCards];
                bool poison = false; string name = tid;
                foreach (var h in Sheytoonak.Core.Duel.StarterHeroes.All)
                {
                    if (h.Poison.Id == tid) { poison = true; name = h.Poison.NameAr; }
                    else if (h.Antidote.Id == tid) name = h.Antidote.NameAr;
                }
                DraftScreen.Badge(new Rect(r.center.x - s * 0.5f, r.y + ch * 0.1f, s, s), tid, poison ? DuelHud.PoisonRed : DuelHud.AntidoteGreen);
                GUI.Label(new Rect(r.x, r.y + ch * 0.1f + s + 4 * u, r.width, 34 * u), "×" + Ar.N(counts[tid]), big);
                GUI.Label(new Rect(r.x + 6 * u, r.y + ch * 0.1f + s + 40 * u, r.width - 12 * u, 28 * u), Ar.T(name), lab);
                // how close the tool is now to its next level
                var lv = duelGame.Levels; int need = lv.CardsNeeded(tid), have = lv.Cards(tid);
                var bar = new Rect(r.x + 14 * u, r.yMax - 34 * u, r.width - 28 * u, 16 * u);
                GUI.color = new Color(0f, 0f, 0f, 0.6f); GUI.DrawTexture(bar, Texture2D.whiteTexture);
                GUI.color = need > 0 && have >= need ? new Color(0.3f, 0.85f, 0.35f) : new Color(0.35f, 0.6f, 1f);
                GUI.DrawTexture(new Rect(bar.x, bar.y, need > 0 ? bar.width * Mathf.Clamp01(have / (float)need) : bar.width, bar.height), Texture2D.whiteTexture);
                GUI.color = old;
                GUI.Label(new Rect(bar.x, bar.y - 24 * u, bar.width, 22 * u), need > 0 ? Ar.N(Mathf.Min(have, need)) + " / " + Ar.N(need) : Ar.T(Ar.MaxLevel), new GUIStyle(lab) { fontSize = Mathf.RoundToInt(15 * u) });
            }
            if (Time.time - lootAt > n * 0.25f + 0.4f)
                GUI.Label(new Rect(0, Screen.height * 0.84f, Screen.width, 40 * u), Ar.T("دوس لتكمّل"), lab);
            // tap anywhere to close (not in the first half second: the tap that opened the bag must not close it)
            if (Time.time - lootAt > 0.5f && GUI.Button(all, GUIContent.none, GUIStyle.none)) { lootShown = false; touch?.IgnoreFor(0.3f); }
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
            float sw = Mathf.Min(105 * u, (Screen.width - 80 * u) / 4f), gap = 14 * u;
            float total = sw * 4 + gap * 3, x0 = (Screen.width - total) * 0.5f, y = Screen.height - sw - 44 * u;
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
                var bagTex = Resources.Load<Texture2D>("Art/Bags/bag_" + (int)b.Size); // painted bags (2026-11-04), glyph otherwise
                if (bagTex != null) GUI.DrawTexture(ir, bagTex, ScaleMode.ScaleToFit);
                else DuelHud.DrawIcon(ir, "bag", BagTint[(int)b.Size]);
                if (GUI.Button(r, GUIContent.none, GUIStyle.none)) { touch?.IgnoreFor(0.5f); duelGame.TapBag(i); }
                var lr = new Rect(r.x, r.yMax + 4 * u, r.width, 30 * u);
                if (ready) DuelHud.DrawIcon(new Rect(lr.center.x - 15 * u, lr.y, 30 * u, 30 * u), "pick", new Color(0.4f, 1f, 0.5f));
                else
                {
                    // the time it takes (closed) or what is left (opening): user 2026-11-10, at least an hour
                    var span = b.Unlocking ? rack.TimeLeft(i, now) : rack.UnlockTime(b.Size, now);
                    int mins = Mathf.Max(1, Mathf.CeilToInt((float)span.TotalMinutes));
                    string t = mins >= 60 ? Ar.N(mins / 60) + ":" + Ar.N(mins % 60).PadLeft(2, '٠') : Ar.N(mins) + "'";
                    var st = new GUIStyle(num) { fontSize = Mathf.RoundToInt(19 * u), alignment = TextAnchor.MiddleLeft };
                    if (b.Unlocking) st.normal.textColor = new Color(1f, 0.85f, 0.4f);
                    DuelHud.DrawIcon(new Rect(lr.center.x - 38 * u, lr.y + 2 * u, 26 * u, 26 * u), "clock", b.Unlocking ? new Color(1f, 0.85f, 0.4f) : Color.white);
                    GUI.Label(new Rect(lr.center.x - 10 * u, lr.y, 60 * u, 30 * u), t, st);
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
