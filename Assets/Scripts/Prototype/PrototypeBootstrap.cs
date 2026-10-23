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

        [Tooltip("Third-person: the camera sits behind and above the player and follows them sideways along the sidewalk.")]
        public bool cameraFollowsPlayer = true;
        // User, 2026-10-23, after the first phone test: "we should see the player from behind" and the
        // game is landscape. Behind-and-above keeps the player, the ball's bounce and the whole facade
        // in one wide frame; the lower FOV suits a landscape screen (75 was for portrait).
        public Vector3 cameraOffset = new Vector3(0f, 2.6f, -4.2f);
        public float cameraPitchDeg = 14f;
        public float cameraFov = 60f;

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
        Transform facadeRoot, oldFacadeRoot, rightLegPivot;
        Renderer[] windowRenderers = new Renderer[0];
        Coroutine kickAnim;
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
            Box(root, new Vector3(0f, -0.06f, 16.5f), new Vector3(80f, 0.1f, 27f), asphalt);         // street and beyond
            for (int i = -6; i <= 6; i++)
                Box(root, new Vector3(i * 4f, -0.005f, 6.2f), new Vector3(2f, 0.01f, 0.15f), shoe);   // centre line
            Box(root, new Vector3(0f, -0.04f, 9f), new Vector3(80f, 0.1f, 2f), sidewalk);            // far sidewalk
            Box(root, new Vector3(-3.4f, 3f, 2.4f), new Vector3(0.12f, 6f, 0.12f), metal);           // lamp post
            Box(root, new Vector3(-2.9f, 5.95f, 2.4f), new Vector3(1.0f, 0.1f, 0.12f), metal);
            for (int i = 0; i < 2; i++)
                Box(root, new Vector3(2.6f + i * 1.6f, 0.45f, 2.6f), new Vector3(0.1f, 0.9f, 0.1f), metal); // bollards
        }

        void BuildGameObjects()
        {
            var config = ScriptableObject.CreateInstance<GameConfig>();
            var houses = new[] { House1(), House2(), House3() };

            // Player: foot position + window mapping, the placeholder body as children.
            var player = new GameObject("Player");
            foot = player.AddComponent<PlayerFoot>();
            var mapper = player.AddComponent<BackdropWindowMapper>();
            mapper.cam = cam;
            BuildLegs(player.transform);
            BuildBody(player.transform);

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
            var overlay = gameGo.AddComponent<SensorDebugOverlay>();
            overlay.motion = motion;
            overlay.game = game;
            gameGo.SetActive(true);
        }

        void BuildLegs(Transform player)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                var pivot = new GameObject(side < 0 ? "Left leg" : "Right leg").transform;
                pivot.SetParent(player, false);
                pivot.localPosition = new Vector3(side * 0.13f, 0.86f, 0.05f);
                var thigh = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                Strip(thigh);
                thigh.transform.SetParent(pivot, false);
                thigh.transform.localPosition = new Vector3(0f, -0.42f, 0f);
                thigh.transform.localScale = new Vector3(0.15f, 0.42f, 0.15f);
                thigh.GetComponent<Renderer>().sharedMaterial = jeans;
                var sneaker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Strip(sneaker);
                sneaker.transform.SetParent(pivot, false);
                sneaker.transform.localPosition = new Vector3(0f, -0.82f, 0.07f);
                sneaker.transform.localScale = new Vector3(0.12f, 0.09f, 0.28f);
                sneaker.GetComponent<Renderer>().sharedMaterial = shoe;
                if (side > 0) rightLegPivot = pivot;
            }
        }

        /// <summary>Torso, arms and head above the legs, so the player reads as a person from behind.</summary>
        void BuildBody(Transform player)
        {
            var torso = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            Strip(torso);
            torso.name = "Torso";
            torso.transform.SetParent(player, false);
            torso.transform.localPosition = new Vector3(0f, 1.18f, 0.02f);
            torso.transform.localScale = new Vector3(0.36f, 0.3f, 0.22f);
            torso.GetComponent<Renderer>().sharedMaterial = awning; // a plain shirt colour
            for (int side = -1; side <= 1; side += 2)
            {
                var arm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                Strip(arm);
                arm.name = side < 0 ? "Left arm" : "Right arm";
                arm.transform.SetParent(player, false);
                arm.transform.localPosition = new Vector3(side * 0.26f, 1.1f, 0.02f);
                arm.transform.localRotation = Quaternion.Euler(0f, 0f, side * -8f);
                arm.transform.localScale = new Vector3(0.09f, 0.28f, 0.09f);
                arm.GetComponent<Renderer>().sharedMaterial = awning;
            }
            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Strip(head);
            head.name = "Head";
            head.transform.SetParent(player, false);
            head.transform.localPosition = new Vector3(0f, 1.62f, 0.02f);
            head.transform.localScale = Vector3.one * 0.22f;
            head.GetComponent<Renderer>().sharedMaterial = sidewalk; // skin-ish placeholder
        }

        // ------------------------------------------------------------------ houses (placeholder data)

        HouseDefinition House1() => MakeHouse("House 1", new[] { 2.6f, 4.3f }, new[] { -2.4f, 0f, 2.4f }, 1.1f, 1.3f, 6.0f, 0f, 1.6f, 0.6f, 1f);
        HouseDefinition House2() => MakeHouse("House 2", new[] { 2.6f, 4.3f, 6.0f }, new[] { -2.4f, 0f, 2.4f }, 1.1f, 1.3f, 7.2f, 0.4f, 1.45f, 1.0f, 0.95f);
        HouseDefinition House3() => MakeHouse("House 3", new[] { 2.6f, 4.3f, 6.0f }, new[] { -3f, -1f, 1f, 3f }, 1.0f, 1.2f, 7.2f, 0.8f, 1.3f, 1.4f, 0.9f);

        static HouseDefinition MakeHouse(string name, float[] rows, float[] cols, float w, float h, float roof,
                                         float wind, float returnTime, float spread, float hitScale)
        {
            var house = ScriptableObject.CreateInstance<HouseDefinition>();
            house.name = name;
            house.displayName = name;
            house.facadeDistance = 10f;
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

            var body = Box(root, new Vector3(0f, roof * 0.5f, 0.25f), new Vector3(w, roof, 0.5f), brick);
            body.GetComponent<Renderer>().material.color = BrickTints[index % BrickTints.Length];
            Box(root, new Vector3(0f, roof + 0.1f, 0.2f), new Vector3(w + 0.3f, 0.2f, 0.7f), frame);           // cornice
            Box(root, new Vector3(-(w * 0.5f + 5f), (roof + 1.5f) * 0.5f, 0.6f), new Vector3(10f, roof + 1.5f, 0.5f), neighbour);
            Box(root, new Vector3(w * 0.5f + 5f, (roof - 1f) * 0.5f, 0.6f), new Vector3(10f, roof - 1f, 0.5f), neighbour);
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

        void OnWindowBroken(int i, Vector3 point)
        {
            if (i < 0 || i >= windowRenderers.Length) return;
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
            if (kickAnim != null) StopCoroutine(kickAnim);
            kickAnim = StartCoroutine(KickSwing(Mathf.Lerp(45f, 85f, g.Strength01)));
            if (quality > 0.85f) Toast("Clean strike!", 0.7f);
        }

        IEnumerator KickSwing(float angle)
        {
            if (rightLegPivot == null) yield break;
            const float up = 0.09f, down = 0.25f;
            for (float t = 0f; t < up; t += Time.deltaTime)
            {
                rightLegPivot.localRotation = Quaternion.Euler(-angle * (t / up), 0f, 0f);
                yield return null;
            }
            for (float t = 0f; t < down; t += Time.deltaTime)
            {
                rightLegPivot.localRotation = Quaternion.Euler(-angle * (1f - t / down), 0f, 0f);
                yield return null;
            }
            rightLegPivot.localRotation = Quaternion.identity;
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
            // Follow the player sideways, but only partly: the camera staying a little behind the
            // player's lateral position keeps the facade from sliding around and shows which way they ran.
            float x = Mathf.Lerp(p.x, foot.X * 0.7f, 1f - Mathf.Exp(-8f * Time.unscaledDeltaTime));
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
                        router != null && router.UsingMotion ? "Tap the screen to kick (hold = harder); tilt the phone to move" : "Tap to kick (hold = harder), finger position moves you",
                        new GUIStyle(label) { alignment = TextAnchor.MiddleCenter }, TextAnchor.MiddleCenter);
            }

            if (showStart)
            {
                Panel();
                GUILayout.BeginArea(new Rect(pad * 2, Screen.height * 0.2f, Screen.width - pad * 4, Screen.height * 0.7f));
                GUILayout.Label("SHEYTOONAK - prototype", big);
                GUILayout.Label("Placeholder graphics. Tilt or move the phone left/right to reach the ball. " +
                                "Tap anywhere on the screen to kick; hold a moment before lifting for a harder, higher kick. " +
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
