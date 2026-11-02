using System;
using System.Collections;
using Sheytoonak.Core;
using Sheytoonak.Core.Duel;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Sheytoonak
{
    /// <summary>
    /// Runs one duel on the shared stage (docs/SCENARIO.fa.md v1.3): both sides kick the same ball at the
    /// same facade from the centre spot, the rules live in <see cref="DuelRules"/>, the opponent is a
    /// <see cref="DuelBot"/> until there is a server. Poisons are turned into physics here: covered and
    /// shut windows bounce, the wind scatters, a pigeon may knock the ball down, a puddle weakens the kick.
    /// </summary>
    public class DuelManager : MonoBehaviour, IBallListener
    {
        public GameConfig config;
        public HouseDefinition house;
        public BallController ball;
        public PlayerFoot foot;
        public BackdropWindowMapper mapper;
        public MonoBehaviour inputSource;
        [Tooltip("Shards each window is worth before size scaling.")]
        public int shardsPerWindow = 40;
        [Tooltip("0..1: how often the bot's aim lands on the window it meant.")]
        public float botAccuracy = 0.65f;
        public float botThinkSeconds = 1.2f;
        public int randomSeed = 0;

        public DuelRules Rules { get; private set; }
        public DuelBot Bot { get; private set; }
        public Side Me => Side.Left;
        public float TurnTimeLeft { get; private set; }
        public int LastHitWindow { get; private set; } = -1;
        public bool PlayerMayKick => Rules != null && Rules.CanAct(Me) == ActionReject.None;
        public int[] Curses { get; } = { 0, 0 };
        public int ShardBank { get; private set; }

        public event Action<string, float> Toast;
        public event Action<Side, int> KickLaunched;        // side, target window or -1
        public event Action<Side> TurnChanged;
        public event Action<Side, bool> Finished;            // winner, I won
        public event Action DuelStarted;

        IKickInput input;
        Coroutine botRoutine, resolveRoutine;
        int hitWindow = -1;
        int groundBounces;
        bool kickActive;
        Side kicker;
        float kickStarted;
        const string PrefCurse = "sheytoonak.curses", PrefShards = "sheytoonak.shards";

        void Awake()
        {
            input = inputSource as IKickInput;
            ball.Init(this, config);
            foot.Init(config, null);
            EnsureWorldColliders();
            Curses[0] = PlayerPrefs.GetInt(PrefCurse, 0);
            ShardBank = PlayerPrefs.GetInt(PrefShards, 0);
        }

        void OnEnable() { if (input != null) input.Kicked += OnKickGesture; }
        void OnDisable() { if (input != null) input.Kicked -= OnKickGesture; }

        // ------------------------------------------------------------------ flow

        public DraftRules Draft { get; private set; }
        [Tooltip("CI screenshots: the player's draft steps are taken by the bot logic too.")]
        public bool autoDraftForMe;
        public event Action DraftStarted;
        System.Random draftRng;

        /// <summary>Starts the pre-duel draft (ban/pick); the duel itself begins when the draft is done.</summary>
        public void NewDuel()
        {
            StopAllCoroutines(); botRoutine = resolveRoutine = null;
            Rules = null;
            ball.Hide();
            mapper.Map(house);
            GameEvents.RaiseHouseLoaded(house, 0);
            draftRng = randomSeed != 0 ? new System.Random(randomSeed) : new System.Random();
            var mine = PoolIds();
            var theirs = PoolIds();
            Draft = new DraftRules(Random.value < 0.5f ? Side.Left : Side.Right, mine, theirs);
            Draft.Acted += (side, kind, hero) =>
            {
                if (kind == DraftRules.Kind.Ban) Toast?.Invoke((side == Me ? Ar.YouBan : Ar.TheyBan) + " " + StarterHeroes.Get(hero).NameAr, 1.2f);
            };
            Draft.Finished += () => StartCoroutine(BeginAfterDraft());
            DraftStarted?.Invoke();
            StartCoroutine(DraftBots());
        }

        static string[] PoolIds()
        {
            var a = new string[StarterHeroes.All.Length];
            for (int i = 0; i < a.Length; i++) a[i] = StarterHeroes.All[i].Id;
            return a;
        }

        /// <summary>The player's own draft step (from the draft screen).</summary>
        public bool DraftAct(string hero) => Draft != null && Draft.Act(Me, hero);

        IEnumerator DraftBots()
        {
            while (Draft != null && !Draft.Done)
            {
                var who = Draft.Current.Who;
                if (who == Me && !autoDraftForMe) { yield return null; continue; }
                yield return new WaitForSeconds(who == Me ? 0.4f : botThinkSeconds);
                if (Draft == null || Draft.Done || Draft.Current.Who != who) continue;
                var h = Draft.BotChoice(who, StarterHeroes.Get, PoolIds(), draftRng);
                if (h != null) Draft.Act(who, h);
            }
        }

        IEnumerator BeginAfterDraft()
        {
            yield return new WaitForSeconds(0.8f);
            var left = new HeroDef[3]; var right = new HeroDef[3];
            for (int i = 0; i < 3; i++) { left[i] = StarterHeroes.Get(Draft.Picked[0][i]); right[i] = StarterHeroes.Get(Draft.Picked[1][i]); }
            BeginDuel(left, right, Draft.First);
        }

        void BeginDuel(HeroDef[] left, HeroDef[] right, Side first)
        {
            int n = mapper.WindowRects.Length;
            // rows: bottom row = direct (1), the rest indirect (2). Rows are found by y of the window rects.
            var rows = RowsOf(mapper.WindowRects);
            var shards = new int[n];
            float meanArea = 0f; foreach (var r in mapper.WindowRects) meanArea += r.width * r.height; meanArea /= Mathf.Max(1, n);
            for (int i = 0; i < n; i++) shards[i] = Mathf.Max(5, Mathf.RoundToInt(shardsPerWindow * (mapper.WindowRects[i].width * mapper.WindowRects[i].height) / Mathf.Max(0.01f, meanArea)));
            Rules = new DuelRules(rows, shards, left, right, randomSeed);
            Bot = new DuelBot(randomSeed);
            Rules.TurnStarted += OnTurnStarted;
            Rules.WindowTaken += (s, w, v) => { if (w == Rules.HiddenWindow) Toast?.Invoke(Ar.HiddenThree, 1.6f); };
            Rules.Ended += OnEnded;
            Rules.RageGained += s => { if (s == Me) Toast?.Invoke(Ar.RageReady, 1.2f); };
            PlaceBallAtCentre();
            Curses[1] = Mathf.Max(0, Curses[0] + Random.Range(-3, 4)); // a "matched" opponent
            DuelStarted?.Invoke();
            // the drafter who banned first also opens (and gets the single first action)
            Rules.Start(first);
        }

        static int[] RowsOf(Rect[] rects)
        {
            // group by centre y (windows of one row share it within 10 cm); return counts bottom row first
            var ys = new System.Collections.Generic.List<float>();
            foreach (var r in rects) { bool found = false; foreach (var y in ys) if (Mathf.Abs(y - r.center.y) < 0.1f) { found = true; break; } if (!found) ys.Add(r.center.y); }
            ys.Sort();
            var rows = new int[ys.Count];
            foreach (var r in rects) for (int i = 0; i < ys.Count; i++) if (Mathf.Abs(ys[i] - r.center.y) < 0.1f) { rows[i]++; break; }
            return rows;
        }

        /// <summary>Window index to row index (bottom = 0), for the value table and the HUD.</summary>
        public int RowOf(int window)
        {
            var ys = new System.Collections.Generic.List<float>();
            foreach (var r in mapper.WindowRects) { bool f = false; foreach (var y in ys) if (Mathf.Abs(y - r.center.y) < 0.1f) { f = true; break; } if (!f) ys.Add(r.center.y); }
            ys.Sort();
            for (int i = 0; i < ys.Count; i++) if (Mathf.Abs(ys[i] - mapper.WindowRects[window].center.y) < 0.1f) return i;
            return 0;
        }

        void OnTurnStarted(Side s, int turn, int actions)
        {
            TurnTimeLeft = DuelRules.TurnSeconds;
            TurnChanged?.Invoke(s);
            if (s != Me)
            {
                Bot.NewTurn();
                if (botRoutine != null) StopCoroutine(botRoutine);
                botRoutine = StartCoroutine(BotTurn());
            }
        }

        void Update()
        {
            if (Rules == null || Rules.Finished) return;
            if (!Rules.KickInFlight)
            {
                TurnTimeLeft -= Time.deltaTime;
                if (TurnTimeLeft <= 0f)
                {
                    Toast?.Invoke(Ar.TimeUp, 1.2f);
                    var who = Rules.Current;
                    Rules.Timeout(who);
                    if (!Rules.Finished) TurnTimeLeft = DuelRules.TurnSeconds;
                }
            }
            else if (Time.time - kickStarted > 6f) FinishKick(); // the ball got stuck somewhere: resolve anyway
        }

        void OnEnded(Side winner, string reason)
        {
            if (botRoutine != null) StopCoroutine(botRoutine);
            var (shards, curse) = Rules.RewardFor(Me);
            Curses[0] = DuelRules.ApplyCurse(Curses[0], curse);
            ShardBank += shards;
            PlayerPrefs.SetInt(PrefCurse, Curses[0]); PlayerPrefs.SetInt(PrefShards, ShardBank); PlayerPrefs.Save();
            ball.Hide();
            Finished?.Invoke(winner, winner == Me);
        }

        // ------------------------------------------------------------------ player actions (from the HUD)

        public ActionReject Poison(int hero) => Act(Me, hero, true);
        public ActionReject Antidote(int hero) => Act(Me, hero, false);
        public ActionReject Rage() { var r = Rules.UseRage(Me); if (r == ActionReject.None) Toast?.Invoke(Ar.RageUsed, 1f); return r; }
        public ActionReject Pass() { var r = Rules.Pass(Me); return r; }
        public void Surrender() { Rules.Surrender(Me); }

        ActionReject Act(Side s, int hero, bool poison)
        {
            var h = Rules.Heroes[(int)s][hero];
            if (poison)
            {
                int[] targets = TargetsFor(h.Poison);
                var r = Rules.Poison(s, h.Poison, targets);
                if (r == ActionReject.None) { Rules.NoteActivity(s); Toast?.Invoke((s == Me ? Ar.YouPoison : Ar.TheyPoison) + " " + h.Poison.NameAr, 1.4f); }
                return r;
            }
            else
            {
                var r = Rules.Antidote(s, h.Antidote);
                if (r == ActionReject.None) { Rules.NoteActivity(s); Toast?.Invoke((s == Me ? Ar.YouAntidote : Ar.TheyAntidote) + " " + h.Antidote.NameAr, 1.4f); }
                return r;
            }
        }

        /// <summary>Cover/Close poisons need windows: take the most valuable breakable ones, never all (the rules refuse that).</summary>
        public int[] TargetsFor(Tool poison)
        {
            if (poison.Effect != Effect.Cover && poison.Effect != Effect.Close) return new int[0];
            var list = new System.Collections.Generic.List<int>();
            for (int w = 0; w < Rules.WindowCount; w++) if (Rules.IsBreakable(w)) list.Add(w);
            list.Sort((a, b) => Rules.ShownValue(b).CompareTo(Rules.ShownValue(a)));
            int take = poison.Effect == Effect.Close ? 1 : Mathf.Min(poison.Power + 1, list.Count - 1);
            take = Mathf.Clamp(take, 0, Mathf.Max(0, list.Count - 1));
            return list.GetRange(0, take).ToArray();
        }

        // ------------------------------------------------------------------ kicks

        void OnKickGesture(KickGesture g)
        {
            if (Rules == null || Rules.Finished || Rules.Current != Me) return;
            var why = Rules.CanAct(Me);
            if (why != ActionReject.None) { if (why == ActionReject.NoActionsLeft) Toast?.Invoke(Ar.NoActions, 0.8f); return; }
            Kick(Me, g, -1);
        }

        void Kick(Side s, KickGesture g, int target)
        {
            if (Rules.BeginKick(s) != ActionReject.None) return;
            Rules.NoteActivity(s);
            var mods = Rules.ModifiersFor(s);
            // Poison physics: wind scatters the aim, a puddle takes the power out of the kick
            if (mods.Accuracy > 0) g.Aim += Random.Range(-1f, 1f) * KickModifiers.ErrorDeg(mods.Accuracy) / Mathf.Max(1f, config.kick.MaxAimYawDeg);
            if (mods.Reach > 0) g.Strength01 *= 1f - 0.18f * mods.Reach;
            Vector3 b = PlaceBallAtCentre();
            float quality = 1f;
            var res = KickSolver.SolveAtWall(g, 0f, quality, config.kick, Random.Range(-1f, 1f), Random.Range(-1f, 1f),
                                             mapper.FacadeZ - b.z, b.y, mapper.MaxTargetHeight);
            var v = new Vector3(res.Velocity.X, res.Velocity.Y, res.Velocity.Z);
            ball.Launch(v, res.SpinY, 0f);
            kicker = s; kickActive = true; hitWindow = -1; groundBounces = 0; kickStarted = Time.time;
            GameEvents.RaiseKicked(g, quality, v);
            KickLaunched?.Invoke(s, target);
            float intercept = KickModifiers.Intercept(mods.Chance);
            if (intercept > 0f && Random.value < intercept) StartCoroutine(Intercept(config.kick.WallFlightTime * 0.45f));
        }

        IEnumerator Intercept(float after)
        {
            yield return new WaitForSeconds(after);
            if (!kickActive || !ball.InPlay) yield break;
            Toast?.Invoke(Ar.Intercepted, 1.2f);
            GameEvents.RaiseWallHit(ball.transform.position); // a thud and a shake
            ball.SetVelocity(new Vector3(Random.Range(-1f, 1f), 1.5f, -2.5f));
        }

        Vector3 PlaceBallAtCentre()
        {
            Vector3 spot = foot.StrikeSpot + Vector3.up * ball.Radius;
            ball.PlaceAtRest(spot);
            return spot;
        }

        /// <summary>Aim gesture that lands on a window's centre (inverse of KickSolver.SolveAtWall), with the bot's error.</summary>
        KickGesture GestureFor(int window, float error)
        {
            Rect r = mapper.WindowRects[window];
            float d = mapper.FacadeZ - foot.StrikeSpot.z;
            float yaw = Mathf.Atan2(r.center.x, d) * Mathf.Rad2Deg;
            float aim = yaw / Mathf.Max(1f, config.kick.MaxAimYawDeg);
            float s = Mathf.InverseLerp(config.kick.MinTargetHeight, mapper.MaxTargetHeight, r.center.y);
            float strength = Mathf.Pow(Mathf.Clamp01(s), 1f / Mathf.Max(0.1f, config.kick.StrengthCurve));
            aim += Random.Range(-1f, 1f) * error * 0.25f;
            strength += Random.Range(-1f, 1f) * error * 0.12f;
            return new KickGesture(Mathf.Clamp01(strength), Mathf.Clamp(aim, -1f, 1f), 0f);
        }

        IEnumerator BotTurn()
        {
            Side bot = Side.Right;
            while (Rules != null && !Rules.Finished && Rules.Current == bot)
            {
                yield return new WaitForSeconds(botThinkSeconds);
                if (Rules.KickInFlight) { yield return null; continue; }
                var move = Bot.Next(Rules, bot);
                switch (move.What)
                {
                    case BotMove.Kind.Antidote: Act(bot, move.Hero, false); break;
                    case BotMove.Kind.Poison: Act(bot, move.Hero, true); break;
                    case BotMove.Kind.Kick:
                        Kick(bot, GestureFor(move.Window, 1f - botAccuracy), move.Window);
                        while (Rules.KickInFlight) yield return null;
                        break;
                    default: Rules.Pass(bot); break;
                }
            }
            botRoutine = null;
        }

        void FinishKick()
        {
            if (!kickActive) return;
            kickActive = false;
            var outcome = Rules.EndKick(kicker, hitWindow);
            LastHitWindow = hitWindow;
            if (outcome == KickOutcome.Missed && kicker == Me) Toast?.Invoke(Ar.Missed, 0.8f);
            if (!Rules.Finished) PlaceBallAtCentre();
        }

        IEnumerator ResolveAfter(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            resolveRoutine = null;
            FinishKick();
        }

        // ------------------------------------------------------------------ physics reports

        public void OnBallGround(Vector3 point)
        {
            if (!kickActive) return;
            groundBounces++;
            GameEvents.RaiseBounced(point, groundBounces, hitWindow >= 0 || groundBounces > 0);
            if (groundBounces >= 2 && resolveRoutine == null) resolveRoutine = StartCoroutine(ResolveAfter(0.3f));
        }

        public void OnBallRolling(Vector3 point) { if (kickActive && resolveRoutine == null) resolveRoutine = StartCoroutine(ResolveAfter(0.1f)); }

        public void OnBallFacade(Vector3 point)
        {
            if (!kickActive || hitWindow >= 0) { GameEvents.RaiseWallHit(point); return; }
            int w = mapper.FindWindow(point, ball.Radius);
            if (w >= 0)
            {
                hitWindow = w;
                if (Rules.IsBreakable(w))
                {
                    ball.Hide();
                    GameEvents.RaiseWindowBroken(w, point);
                    if (resolveRoutine == null) resolveRoutine = StartCoroutine(ResolveAfter(1.0f));
                    return;
                }
                Toast?.Invoke(Rules.IsCovered(w) ? Ar.Covered : Rules.IsClosed(w) ? Ar.Shut : Ar.AlreadyBroken, 1f);
            }
            GameEvents.RaiseWallHit(point);
            // send it back toward the street so it settles quickly
            if (ball.InPlay)
            {
                Vector3 v = ball.Body.linearVelocity; if (v.z > 0f) v.z = -v.z * 0.5f;
                ball.SetVelocity(new Vector3(v.x * 0.5f, Mathf.Max(1f, v.y * 0.3f), Mathf.Min(-2f, v.z)));
            }
        }

        public void OnBallKilled(LossReason reason) { if (kickActive && resolveRoutine == null) resolveRoutine = StartCoroutine(ResolveAfter(0.1f)); }

        // ------------------------------------------------------------------ world

        void EnsureWorldColliders()
        {
            Transform root = transform;
            if (FindAnyObjectByType<GroundSurface>() == null)
                MakeBox("Ground (gameplay)", root, new Vector3(0f, -0.5f, 10f), new Vector3(120f, 1f, 80f), false).AddComponent<GroundSurface>();
            if (mapper.facadeCollider == null)
            {
                var f = MakeBox("Facade (gameplay)", root, new Vector3(0f, -100f, 0f), Vector3.one, false);
                f.AddComponent<FacadeSurface>();
                mapper.facadeCollider = f.GetComponent<BoxCollider>();
            }
            MakeBox("Behind (gameplay)", root, new Vector3(0f, 5f, foot.Z - 3.5f), new Vector3(60f, 20f, 2f), true).AddComponent<KillZone>().reason = LossReason.PassedPlayer;
            MakeBox("Left (gameplay)", root, new Vector3(-30f, 5f, 10f), new Vector3(2f, 30f, 60f), true).AddComponent<KillZone>().reason = LossReason.OutOfBounds;
            MakeBox("Right (gameplay)", root, new Vector3(30f, 5f, 10f), new Vector3(2f, 30f, 60f), true).AddComponent<KillZone>().reason = LossReason.OutOfBounds;
        }

        static GameObject MakeBox(string name, Transform parent, Vector3 pos, Vector3 size, bool trigger)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var box = go.AddComponent<BoxCollider>();
            box.size = size; box.isTrigger = trigger;
            return go;
        }
    }
}
