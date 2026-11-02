using System.Collections;
using Sheytoonak.Core;
using UnityEngine;

namespace Sheytoonak
{
    /// <summary>
    /// Wires input, foot, ball, backdrop mapping and the pure <see cref="RallyRules"/> together.
    /// It turns physics contacts into rule events and rule events into <see cref="GameEvents"/>.
    /// Scene setup it needs: a Camera matched to the backdrop, a PlayerFoot at world z = 0,
    /// a Ball (BallController prefab), a BackdropWindowMapper on the PlayerFoot's parent at z = 0,
    /// and a KickInputRouter. Ground, facade and kill-zone colliders are created at runtime if missing.
    /// </summary>
    public class GameManager : MonoBehaviour, IBallListener
    {
        public GameConfig config;
        [Tooltip("The street, in order. Design: 8 houses.")]
        public HouseDefinition[] houses;
        public BallController ball;
        public PlayerFoot foot;
        public BackdropWindowMapper mapper;
        [Tooltip("Any component implementing IKickInput, normally KickInputRouter.")]
        public MonoBehaviour inputSource;
        [Tooltip("Start a game as soon as the scene loads (turn off when a menu starts it).")]
        public bool autoStart = false;
        [Tooltip("Fixed seed for repeatable tests; 0 = random.")]
        public int randomSeed = 0;

        const string PrefBest = "sheytoonak.best";

        public RallyRules Rules { get; private set; }
        public HouseDefinition CurrentHouse => Rules != null ? houses[Rules.HouseIndex] : null;
        public float Wind { get; private set; }
        public int Best { get; private set; }

        IKickInput input;
        System.Random rng;
        Coroutine pending;
        int lastWindow = -1;

        void Awake()
        {
            Best = PlayerPrefs.GetInt(PrefBest, 0);
            input = inputSource as IKickInput;
            if (input == null) Debug.LogError("GameManager.inputSource must implement IKickInput (use KickInputRouter).");
            ball.Init(this, config);
            foot.Init(config, input);
            EnsureWorldColliders();
        }

        void OnEnable() { if (input != null) input.Kicked += OnKickGesture; }
        void OnDisable() { if (input != null) input.Kicked -= OnKickGesture; }

        void Start() { if (autoStart) NewGame(); }

        void Update()
        {
            // While serving, the ball sits at the feet and moves with the player.
            if (Rules != null && Rules.Phase == RallyPhase.Serve && ball.gameObject.activeSelf && !ball.InPlay)
                ball.transform.position = foot.StrikeSpot + Vector3.up * ball.Radius;
        }

        // ------------------------------------------------------------------ flow

        public void NewGame()
        {
            if (houses == null || houses.Length == 0) { Debug.LogError("GameManager: no houses assigned."); return; }
            if (pending != null) StopCoroutine(pending);
            rng = randomSeed != 0 ? new System.Random(randomSeed) : new System.Random();
            Rules = new RallyRules(config.lives, houses.Length, houses[0].windows.Length, config.maxBouncesBeforeWall);
            Rules.PhaseChanged += OnPhaseChanged;
            Rules.ScoreChanged += GameEvents.RaiseScoreChanged;
            Rules.LifeLost += GameEvents.RaiseLifeLost;
            Rules.MedalEarned += m => GameEvents.RaiseMedal(m, houses[Rules.HouseIndex]);
            input?.Recenter();
            foot.ResetToCentre();
            LoadHouse(0);
            GameEvents.RaiseScoreChanged(0);
            Serve();
        }

        void LoadHouse(int index)
        {
            mapper.Map(houses[index]);
            GameEvents.RaiseHouseLoaded(houses[index], index);
        }

        void Serve()
        {
            var h = CurrentHouse;
            Wind = Mathf.Lerp(h.windRange.x, h.windRange.y, (float)rng.NextDouble());
            GameEvents.RaiseWindChanged(Wind);
            Vector3 spot = foot.StrikeSpot + Vector3.up * ball.Radius;
            ball.PlaceAtRest(spot);
            lastWindow = -1;
            GameEvents.RaiseServed(spot);
        }

        void OnPhaseChanged(RallyPhase p)
        {
            GameEvents.RaisePhaseChanged(p);
            switch (p)
            {
                case RallyPhase.Resolving: Schedule(ResolveThenServe()); break;
                case RallyPhase.HouseCleared: Schedule(WalkToNextHouse()); break;
                case RallyPhase.GameOver: Finish(false); break;
                case RallyPhase.Victory: Finish(true); break;
            }
        }

        void Schedule(IEnumerator routine)
        {
            if (pending != null) StopCoroutine(pending);
            pending = StartCoroutine(routine);
        }

        IEnumerator ResolveThenServe()
        {
            yield return new WaitForSeconds(config.resolveDelay);
            pending = null;
            if (Rules.Phase != RallyPhase.Resolving) yield break;
            Rules.NextServe();
            Serve();
        }

        IEnumerator WalkToNextHouse()
        {
            yield return new WaitForSeconds(config.resolveDelay);
            var from = CurrentHouse;
            var to = houses[Rules.HouseIndex + 1];
            ball.Hide();
            GameEvents.RaiseWalkStarted(from, to, config.walkDuration);
            yield return new WaitForSeconds(config.walkDuration);
            pending = null;
            Rules.AdvanceHouse(to.windows.Length);
            LoadHouse(Rules.HouseIndex);
            Serve();
        }

        void Finish(bool won)
        {
            if (Rules.Score > Best)
            {
                Best = Rules.Score;
                PlayerPrefs.SetInt(PrefBest, Best);
                PlayerPrefs.Save();
            }
            if (won) GameEvents.RaiseVictory(Rules.Score, Best, Rules.Medals);
            else GameEvents.RaiseGameOver(Rules.Score, Best, Rules.Medals);
        }

        // ------------------------------------------------------------------ input

        void OnKickGesture(KickGesture g)
        {
            if (Rules == null || Rules.IsFinished) return;
            Vector3 b = ball.transform.position;
            bool reach = ball.gameObject.activeSelf && foot.BallInReach(b, ball.Radius);
            var why = Rules.CanKick(reach);
            if (why != KickRejectReason.None)
            {
                GameEvents.RaiseKickRejected(why);
                return;
            }
            float offset = foot.ContactOffset(b);
            float quality = KickSolver.ContactQuality(b.y, offset, config.kick);
            var res = KickSolver.SolveAtWall(g, offset, quality, config.kick, Noise(), Noise(),
                                             mapper.FacadeZ - b.z, b.y, mapper.MaxTargetHeight);
            var v = new Vector3(res.Velocity.X, res.Velocity.Y, res.Velocity.Z);
            ball.Launch(v, res.SpinY, Wind);
            Rules.OnKick();
            GameEvents.RaiseKicked(g, quality, v);
        }

        float Noise() => (float)(rng.NextDouble() * 2.0 - 1.0);

        // ------------------------------------------------------------------ physics reports (from BallController)

        public void OnBallGround(Vector3 point)
        {
            if (Rules == null) return;
            var phase = Rules.Phase;
            if (phase != RallyPhase.Outgoing && phase != RallyPhase.Returning) return;
            Rules.OnGroundBounce();
            GameEvents.RaiseBounced(point, phase == RallyPhase.Returning ? Rules.BouncesSinceWall : 0, phase == RallyPhase.Returning);
        }

        /// <summary>The ball stopped hopping and rolls: counts as a further bounce (it touches the ground again).</summary>
        public void OnBallRolling(Vector3 point)
        {
            if (Rules == null) return;
            if (Rules.Phase == RallyPhase.Returning && Rules.BouncesSinceWall >= 1) Rules.OnGroundBounce();
            else if (Rules.Phase == RallyPhase.Outgoing) Rules.OnBallLost(LossReason.ShortShot);
        }

        public void OnBallFacade(Vector3 point)
        {
            if (Rules == null) return;
            if (Rules.Phase != RallyPhase.Outgoing && Rules.Phase != RallyPhase.Returning) return;

            int w = mapper.FindWindow(point, ball.Radius);
            // A ball the owner just threw out must not count its own window on the way out.
            if (w >= 0 && !(w == lastWindow && Rules.Phase == RallyPhase.Returning))
            {
                HitWindow(w, point);
                return;
            }
            Rules.OnWallHit();
            ApplyReturnAssist();
            GameEvents.RaiseWallHit(point);
        }

        void HitWindow(int w, Vector3 point)
        {
            bool alreadyBroken = Rules.IsBroken(w);
            bool throwBack = alreadyBroken && rng.NextDouble() < config.ownerThrowBackChance;
            ball.Hide(); // the ball goes through the glass into the flat
            lastWindow = w;
            if (!alreadyBroken) GameEvents.RaiseWindowBroken(w, point);
            else GameEvents.RaiseBallInHouse(w, throwBack);
            var result = Rules.OnWindowHit(w, throwBack);
            if (result == WindowHitResult.ThrownBack) Schedule(OwnerThrowsBack(w));
        }

        IEnumerator OwnerThrowsBack(int w)
        {
            yield return new WaitForSeconds(config.ownerThrowDelay);
            pending = null;
            if (Rules.Phase != RallyPhase.Returning) yield break;
            Rect r = mapper.WindowRects[w];
            var from = new Vector3(r.center.x, r.center.y, mapper.FacadeZ - ball.Radius - 0.1f);
            ball.PlaceAtRest(from);
            ball.Launch(Vector3.zero, 0f, 0f);
            ball.SetVelocity(LandingVelocity(from, config.ownerThrowFlightTime));
            GameEvents.RaiseOwnerThrow(w);
        }

        public void OnBallKilled(LossReason reason)
        {
            Rules?.OnBallLost(reason);
        }

        /// <summary>
        /// Off the facade, blend the physical rebound with a designed arc whose first bounce lands
        /// somewhere the player can run to. This is what makes "move left/right to reach the ball" the game.
        /// </summary>
        void ApplyReturnAssist()
        {
            if (!ball.InPlay || config.returnAssist <= 0f) return;
            Vector3 real = ball.Body.linearVelocity;
            if (real.z > 0f) real.z = -real.z * 0.6f; // physics may report the pre-contact velocity on some frames
            Vector3 designed = LandingVelocity(ball.transform.position, CurrentHouse.returnFlightTime);
            ball.SetVelocity(Vector3.Lerp(real, designed, config.returnAssist));
        }

        Vector3 LandingVelocity(Vector3 from, float flightTime)
        {
            var h = CurrentHouse;
            float x = Mathf.Clamp((float)(rng.NextDouble() * 2.0 - 1.0) * h.returnSpreadX, -config.laneHalfWidth, config.laneHalfWidth);
            float z = foot.Z + Mathf.Lerp(config.returnLandingDepth.x, config.returnLandingDepth.y, (float)rng.NextDouble());
            var v = KickSolver.BallisticVelocity(new Vec3(from.x, from.y, from.z), new Vec3(x, ball.Radius, z), flightTime, -Physics.gravity.y);
            return new Vector3(v.X, v.Y, v.Z);
        }

        // ------------------------------------------------------------------ world

        void EnsureWorldColliders()
        {
            Transform root = transform;
            if (FindAnyObjectByType<GroundSurface>() == null)
            {
                var g = MakeBox("Ground (gameplay)", root, new Vector3(0f, -0.5f, 10f), new Vector3(120f, 1f, 80f), false);
                g.AddComponent<GroundSurface>();
            }
            if (mapper.facadeCollider == null)
            {
                // Parked far away until BackdropWindowMapper.Map() places it.
                var f = MakeBox("Facade (gameplay)", root, Parked, Vector3.one, false);
                f.AddComponent<FacadeSurface>();
                mapper.facadeCollider = f.GetComponent<BoxCollider>();
            }
            else if (mapper.facadeCollider.GetComponent<FacadeSurface>() == null)
            {
                mapper.facadeCollider.gameObject.AddComponent<FacadeSurface>();
            }
            if (mapper.roofKillZone == null)
            {
                var r = MakeBox("Over roof (gameplay)", root, Parked, Vector3.one, true);
                r.AddComponent<KillZone>().reason = LossReason.OverRoof;
                mapper.roofKillZone = r.GetComponent<BoxCollider>();
            }
            // Behind the player and far to the sides.
            MakeBox("Behind player (gameplay)", root, new Vector3(0f, 5f, foot.Z - 2.5f), new Vector3(60f, 20f, 2f), true)
                .AddComponent<KillZone>().reason = LossReason.PassedPlayer;
            MakeBox("Left of street (gameplay)", root, new Vector3(-30f, 5f, 10f), new Vector3(2f, 30f, 60f), true)
                .AddComponent<KillZone>().reason = LossReason.OutOfBounds;
            MakeBox("Right of street (gameplay)", root, new Vector3(30f, 5f, 10f), new Vector3(2f, 30f, 60f), true)
                .AddComponent<KillZone>().reason = LossReason.OutOfBounds;
        }

        static readonly Vector3 Parked = new Vector3(0f, -100f, 0f);

        static GameObject MakeBox(string name, Transform parent, Vector3 pos, Vector3 size, bool trigger)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var box = go.AddComponent<BoxCollider>();
            box.size = size;
            box.isTrigger = trigger;
            return go;
        }

        [ContextMenu("Reachability report")]
        void ReachabilityReport()
        {
            if (mapper.House == null && houses != null && houses.Length > 0) mapper.Map(houses[0]);
            mapper.ReachabilityReport(config.kick, ball != null ? ball.Radius : 0.11f);
        }
    }
}
