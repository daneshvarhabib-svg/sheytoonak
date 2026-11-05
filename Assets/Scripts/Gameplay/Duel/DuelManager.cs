using System;
using System.Collections;
using Sheytoonak.Core;
using Sheytoonak.Core.Duel;
using Sheytoonak.Core.Challenge;
using Sheytoonak.Core.Bags;
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
        /// <summary>Every painted house; each duel picks one, never the same twice in a row (user 2026-11-05: «خونه بک‌گراند
        /// نباید ثابت باشه همیشه و بهتره پنجاه شصت تا خونه مختلف باشه و بازیکنان هر بار یه خونه ببینن»).</summary>
        public HouseDefinition[] housePool;

        void PickHouse()
        {
            if (housePool == null || housePool.Length == 0) return;
            if (housePool.Length == 1) { house = housePool[0]; return; }
            HouseDefinition next;
            do next = housePool[Random.Range(0, housePool.Length)]; while (next == house);
            house = next;
        }
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
        /// <summary>A picture instead of a sentence (user 2026-11-02: «کلا تو بازی نوشته زیاده»): icon id, side, tint kind.</summary>
        public event Action<string, Side, PopKind> Pop;
        public enum PopKind { Poison, Antidote, Info }

        IKickInput input;
        Coroutine botRoutine, resolveRoutine;
        int hitWindow = -1;
        int groundBounces;
        bool kickActive;
        // rally inside one kick action: the ball came off the wall and may be hit again (like the old rally mode)
        bool returning;
        int bouncesSinceWall, rallyHits;
        /// <summary>True while a kick action is still alive (outgoing or coming back to be hit again).</summary>
        public bool RallyActive => kickActive;
        public Side Kicker => kicker;
        public event Action<Side> Rekicked;
        Side kicker;
        float kickStarted;
        const string PrefCurse = "sheytoonak.curses", PrefShards = "sheytoonak.shards";

        void Awake()
        {
            input = inputSource as IKickInput;
            ball.Init(this, config);
            foot.Init(config, input); // the finger moves the kicker along the sidewalk to reach the rebound (user: «باید بتونه توپ برگشتی رو هم بزنه»)
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

        // ------------------------------------------------------------------ challenge (12 wins / 3 losses, SCENARIO §10)

        const string PrefChallenge = "sheytoonak.challenge", PrefChallengeFree = "sheytoonak.challenge.freeUsed";
        ChallengeRun challenge;
        /// <summary>The current or last run (null = never started). Saved after every duel.</summary>
        public ChallengeRun Challenge => challenge ?? (challenge = ChallengeRun.Parse(PlayerPrefs.GetString(PrefChallenge, "")));
        public bool InChallenge { get; private set; }
        public bool ChallengeActive => Challenge != null && !Challenge.Collected && !Challenge.IsOver(DateTime.UtcNow);
        /// <summary>⚠️ The first run is free: a new player has no shards yet and could never try it otherwise.</summary>
        public bool ChallengeFree => PlayerPrefs.GetInt(PrefChallengeFree, 0) == 0;
        public bool CanEnterChallenge => ChallengeActive || ChallengeFree || ShardBank >= ChallengeRun.DefaultEntry;
        public event Action<int> ChallengeCollected; // shards paid

        void SaveChallenge() { if (challenge != null) PlayerPrefs.SetString(PrefChallenge, challenge.Serialize()); PlayerPrefs.SetInt(PrefShards, ShardBank); PlayerPrefs.Save(); }

        /// <summary>Pays any finished run, then starts a new one if needed (paying the entry), then the next duel of the run.</summary>
        // ------------------------------------------------------------------ garbage bags (SCENARIO §8)

        const string PrefBags = "sheytoonak.bags", PrefCards = "sheytoonak.cards";
        BagRules bags;
        /// <summary>Four slots; created on first use with the install time (the first-week clock counts from it).</summary>
        public BagRules Bags => bags ?? (bags = BagRules.Parse(PlayerPrefs.GetString(PrefBags, "")) ?? new BagRules(DateTime.UtcNow));
        /// <summary>slot, shards, cards — raised when a bag is opened so the HUD can show the loot.</summary>
        public event Action<int, int, int> BagOpened;
        void SaveBags() { PlayerPrefs.SetString(PrefBags, Bags.Serialize()); PlayerPrefs.SetInt(PrefShards, ShardBank); PlayerPrefs.Save(); }

        static string[] ToolIds()
        {
            var ids = new System.Collections.Generic.List<string>();
            foreach (var h in StarterHeroes.All) { ids.Add(h.Poison.Id); ids.Add(h.Antidote.Id); }
            return ids.ToArray();
        }

        /// <summary>Tap on a slot: starts its timer if nothing else is unlocking, or opens it when ready. Returns true if something happened.</summary>
        public bool TapBag(int slot)
        {
            var now = DateTime.UtcNow;
            if (Bags.IsReady(slot, now))
            {
                var c = Bags.Open(slot, now, ToolIds());
                if (c == null) return false;
                ShardBank += c.Value.Shards;
                // ⚠️ upgrade cards are only counted for now: the tool-upgrade screen comes later (SCENARIO §8)
                var cards = PlayerPrefs.GetString(PrefCards, "");
                foreach (var id in c.Value.UpgradeCards) cards += (cards.Length > 0 ? "," : "") + id;
                PlayerPrefs.SetString(PrefCards, cards);
                SaveBags();
                BagOpened?.Invoke(slot, c.Value.Shards, c.Value.UpgradeCards.Length);
                return true;
            }
            bool started = Bags.StartUnlock(slot, now);
            if (started) SaveBags();
            return started;
        }

        /// <summary>CI screenshots only: three bags, one counting down and one ready to open.</summary>
        public void ScreenshotBags()
        {
            var now = DateTime.UtcNow;
            while (Bags.AddFromWin() >= 0 && Bags.FreeSlot() > 0) { }
            Bags.StartUnlock(1, now);
            if (Bags.Rack[0] != null) { Bags.Rack[0].UnlockStartTicks = now.Ticks - 1; Bags.Rack[0].ReadyTicks = now.Ticks - 1; }
        }

        public int UpgradeCardCount { get { var s = PlayerPrefs.GetString(PrefCards, ""); return s.Length == 0 ? 0 : s.Split(',').Length; } }

        public bool PlayChallenge()
        {
            CollectChallengeIfOver();
            if (!ChallengeActive)
            {
                if (!CanEnterChallenge) return false;
                int entry = ChallengeRun.DefaultEntry;
                if (ChallengeFree) { PlayerPrefs.SetInt(PrefChallengeFree, 1); }
                else ShardBank -= entry;
                challenge = new ChallengeRun(entry, DateTime.UtcNow);
                SaveChallenge();
            }
            NewChallengeDuel();
            return true;
        }

        public int CollectChallengeIfOver()
        {
            if (Challenge == null) return 0;
            int paid = Challenge.Collect(DateTime.UtcNow);
            if (paid > 0) { ShardBank += paid; SaveChallenge(); ChallengeCollected?.Invoke(paid); }
            else if (Challenge.Collected) SaveChallenge();
            return paid;
        }

        /// <summary>No draft: three heroes dealt at random from the shared pool to each side, equal tools (money does not help).</summary>
        void NewChallengeDuel()
        {
            StopAllCoroutines(); botRoutine = resolveRoutine = null;
            Rules = null; Draft = null;
            ball.Hide();
            PickHouse();
            mapper.Map(house);
            GameEvents.RaiseHouseLoaded(house, 0);
            InChallenge = true;
            BeginDuel(Deal(), Deal(), Random.value < 0.5f ? Side.Left : Side.Right);
        }

        static HeroDef[] Deal()
        {
            var pool = new System.Collections.Generic.List<HeroDef>(StarterHeroes.All);
            var hand = new HeroDef[3];
            for (int i = 0; i < 3; i++) { int k = Random.Range(0, pool.Count); hand[i] = pool[k]; pool.RemoveAt(k); }
            return hand;
        }

        /// <summary>Back to the home screen: nothing on the stage, no HUD (it draws only while Rules exist).</summary>
        public void ToHome()
        {
            StopAllCoroutines(); botRoutine = resolveRoutine = null;
            Rules = null; Draft = null; InChallenge = false;
            ball.Hide();
        }

        /// <summary>Starts the pre-duel draft (ban/pick); the duel itself begins when the draft is done.</summary>
        public void NewDuel()
        {
            InChallenge = false;
            StopAllCoroutines(); botRoutine = resolveRoutine = null;
            Rules = null;
            ball.Hide();
            PickHouse();
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
            var rowMap = new int[n]; for (int i = 0; i < n; i++) rowMap[i] = RowOf(i);
            Rules = new DuelRules(rows, shards, left, right, randomSeed, rowMap);
            Bot = new DuelBot(randomSeed);
            Rules.TurnStarted += OnTurnStarted;
            Rules.WindowTaken += (s, w, v) => { if (w == Rules.HiddenWindow) Toast?.Invoke("٣ ★", 1.6f); };
            Rules.Ended += OnEnded;
            Rules.RageGained += s => Pop?.Invoke("rage", s, PopKind.Info);
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
            foot.Hold = s != Me; // the bot always kicks from the middle; my finger moves the kicker on my turn
            Selected = -1; KickArmed = false; kickerReady[0] = kickerReady[1] = false;
            KickerReleased?.Invoke(Side.Left); KickerReleased?.Invoke(Side.Right);
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
                // the resting ball moves with the kicker (my finger walks the kicker along the pavement); before, it stayed
                // where the last kick left it and jumped to the foot only at the kick
                if (!kickActive && ball.gameObject.activeSelf && ball.Body != null && ball.Body.isKinematic)
                    ball.transform.position = foot.StrikeSpot + Vector3.up * ball.Radius;
                TurnTimeLeft -= Time.deltaTime;
                if (TurnTimeLeft <= 0f)
                {
                    Pop?.Invoke("clock", Rules.Current, PopKind.Info);
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
            if (InChallenge)
            {
                // a challenge duel pays nothing on its own: it counts toward the run, the run pays at its end
                Challenge?.Record(winner == Me, DateTime.UtcNow);
                SaveChallenge();
                CollectChallengeIfOver();
            }
            else
            {
                var (shards, curse) = Rules.RewardFor(Me);
                Curses[0] = DuelRules.ApplyCurse(Curses[0], curse);
                ShardBank += shards;
                PlayerPrefs.SetInt(PrefCurse, Curses[0]); PlayerPrefs.SetInt(PrefShards, ShardBank); PlayerPrefs.Save();
                Bags.AddFromCurses(Curses[0]);
            }
            // a bag comes from a win (any duel, challenge too), not from time; a full rack gets nothing
            if (winner == Me) Bags.AddFromWin();
            SaveBags();
            ball.Hide();
            Finished?.Invoke(winner, winner == Me);
        }

        // ------------------------------------------------------------------ player actions (from the HUD)

        // ------------------------------------------------------------------ choosing the hero of the action
        // User (2026-11-04): «باید اول قهرمان انتخاب شه بعد قدرتش یا زدن ضربه. اگر خواست ضربه بزنه تازه بیاد پشت توپ».
        // ⚠️ until then a hero walked to the ball by itself at the start of every turn and any swipe kicked.

        /// <summary>My chosen hero this turn (0..2), -1 = none yet.</summary>
        public int Selected { get; private set; } = -1;
        /// <summary>The chosen hero stands behind the ball: only now does a swipe kick.</summary>
        public bool KickArmed { get; private set; }
        /// <summary>side, hero slot: that hero should walk up behind the ball (the stage answers with <see cref="KickerArrived"/>).</summary>
        public event Action<Side, int> KickerChosen;
        /// <summary>side: whoever stands at the ball goes home (another hero was chosen, or the turn passed).</summary>
        public event Action<Side> KickerReleased;
        readonly bool[] kickerReady = new bool[2];

        public void Select(int slot)
        {
            if (Rules == null || Rules.Current != Me || Rules.KickInFlight || slot < 0 || slot > 2) return;
            if (Selected == slot) return;
            if (KickArmed || kickerReady[(int)Me]) { KickArmed = false; kickerReady[(int)Me] = false; KickerReleased?.Invoke(Me); }
            Selected = slot;
        }

        /// <summary>The kick button of the chosen hero: it walks up behind the ball; the swipe is accepted when it is there.</summary>
        public void ArmKick()
        {
            if (Selected < 0 || Rules == null || Rules.CanAct(Me) != ActionReject.None || KickArmed) return;
            kickerReady[(int)Me] = false;
            KickerChosen?.Invoke(Me, Selected);
        }

        /// <summary>Called by the stage when the hero has reached the ball (or at once when there is no stage).</summary>
        public void KickerArrived(Side s)
        {
            kickerReady[(int)s] = true;
            if (s == Me && Selected >= 0) KickArmed = true;
        }

        public ActionReject Poison(int hero) => Act(Me, hero, true);
        public ActionReject Antidote(int hero) => Act(Me, hero, false);
        public ActionReject Rage() { var r = Rules.UseRage(Me); if (r == ActionReject.None) Pop?.Invoke("rage", Me, PopKind.Info); return r; }
        public ActionReject Pass() { var r = Rules.Pass(Me); return r; }
        public void Surrender() { Rules.Surrender(Me); }

        ActionReject Act(Side s, int hero, bool poison)
        {
            var h = Rules.Heroes[(int)s][hero];
            if (poison)
            {
                int[] targets = TargetsFor(h.Poison);
                var r = Rules.Poison(s, h.Poison, targets);
                if (r == ActionReject.None) { Rules.NoteActivity(s); Pop?.Invoke(h.Poison.Id, s, PopKind.Poison); }
                return r;
            }
            else
            {
                var r = Rules.Antidote(s, h.Antidote);
                if (r == ActionReject.None) { Rules.NoteActivity(s); Pop?.Invoke(h.Antidote.Id, s, PopKind.Antidote); }
                return r;
            }
        }

        /// <summary>Cover/Close poisons need windows: take the most valuable breakable ones, never all (the rules refuse that).</summary>
        public int[] TargetsFor(Tool poison)
        {
            if (poison.Effect != Effect.Cover && poison.Effect != Effect.Close) return new int[0];
            var list = new System.Collections.Generic.List<int>();
            for (int w = 0; w < Rules.WindowCount; w++) if (Rules.IsBreakable(w)) list.Add(w);
            if (poison.Effect == Effect.Cover)
            {
                // a bus can only hide what is behind it: the lowest breakable windows, never all of them
                list.Sort((a, b) => RowOf(a) != RowOf(b) ? RowOf(a).CompareTo(RowOf(b)) : mapper.WindowRects[a].x.CompareTo(mapper.WindowRects[b].x));
                int lowRow = list.Count > 0 ? RowOf(list[0]) : 0;
                var row = list.FindAll(w => RowOf(w) == lowRow);
                int take = Mathf.Min(row.Count, list.Count - 1);
                return row.GetRange(0, Mathf.Max(0, take)).ToArray();
            }
            list.Sort((a, b) => Rules.ShownValue(b).CompareTo(Rules.ShownValue(a)));
            return list.Count > 1 ? new[] { list[0] } : new int[0];
        }

        // ------------------------------------------------------------------ kicks

        void OnKickGesture(KickGesture g)
        {
            if (Rules == null || Rules.Finished || Rules.Current != Me) return;
            if (kickActive)
            {
                // the same action goes on: hit the ball that came back off the wall, if the foot reaches it
                if (kicker == Me && returning && ball.InPlay && foot.BallInReach(ball.transform.position, ball.Radius)) Rekick(Me, g);
                return;
            }
            var why = Rules.CanAct(Me);
            if (why != ActionReject.None) { return; }
            if (!KickArmed) { Pop?.Invoke("pick", Me, PopKind.Info); return; } // first a hero, then its kick button
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
            returning = false; bouncesSinceWall = 0; rallyHits = 0;
            GameEvents.RaiseKicked(g, quality, v);
            KickLaunched?.Invoke(s, target);
            float intercept = KickModifiers.Intercept(mods.Chance);
            if (intercept > 0f && Random.value < intercept) StartCoroutine(Intercept(config.kick.WallFlightTime * 0.45f));
        }

        /// <summary>Hits the rebound: same action, the contact (height, how far off the foot) decides how clean it is.</summary>
        void Rekick(Side s, KickGesture g)
        {
            var mods = Rules.ModifiersFor(s);
            if (mods.Accuracy > 0) g.Aim += Random.Range(-1f, 1f) * KickModifiers.ErrorDeg(mods.Accuracy) / Mathf.Max(1f, config.kick.MaxAimYawDeg);
            if (mods.Reach > 0) g.Strength01 *= 1f - 0.18f * mods.Reach;
            Vector3 b = ball.transform.position;
            float offset = s == Me ? foot.ContactOffset(b) : 0f;
            float quality = s == Me ? KickSolver.ContactQuality(b.y, offset, config.kick) : 1f;
            var res = KickSolver.SolveAtWall(g, offset, quality, config.kick, Random.Range(-1f, 1f), Random.Range(-1f, 1f),
                                             mapper.FacadeZ - b.z, b.y, mapper.MaxTargetHeight);
            var v = new Vector3(res.Velocity.X, res.Velocity.Y, res.Velocity.Z);
            ball.Launch(v, res.SpinY, 0f);
            if (resolveRoutine != null) { StopCoroutine(resolveRoutine); resolveRoutine = null; }
            returning = false; bouncesSinceWall = 0; groundBounces = 0; hitWindow = -1; rallyHits++;
            Rules.NoteActivity(s);
            GameEvents.RaiseKicked(g, quality, v);
            Rekicked?.Invoke(s);
            float intercept = KickModifiers.Intercept(mods.Chance);
            if (intercept > 0f && Random.value < intercept) StartCoroutine(Intercept(config.kick.WallFlightTime * 0.45f));
        }

        /// <summary>Off the wall the ball comes back on a designed arc that lands where the kicker can run to it.</summary>
        void SendBack()
        {
            if (!ball.InPlay) return;
            Vector3 from = ball.transform.position;
            Vector3 real = ball.Body.linearVelocity; if (real.z > 0f) real.z = -real.z * 0.6f;
            float x = Mathf.Clamp(foot.X + Random.Range(-1f, 1f) * (house != null ? house.returnSpreadX : 0.8f), -config.laneHalfWidth, config.laneHalfWidth);
            float z = foot.Z + Random.Range(config.returnLandingDepth.x, config.returnLandingDepth.y);
            var d = KickSolver.BallisticVelocity(new Vec3(from.x, from.y, from.z), new Vec3(x, ball.Radius, z), house != null ? house.returnFlightTime : 1.6f, -Physics.gravity.y);
            ball.SetVelocity(Vector3.Lerp(real, new Vector3(d.X, d.Y, d.Z), config.returnAssist));
            returning = true; bouncesSinceWall = 0;
        }

        IEnumerator Intercept(float after)
        {
            yield return new WaitForSeconds(after);
            if (!kickActive || !ball.InPlay) yield break;
            Pop?.Invoke("pigeon", kicker, PopKind.Poison);
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

        int botKicks;
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
                        if (!kickerReady[(int)bot])
                        {
                            KickerChosen?.Invoke(bot, botKicks++ % 3);
                            float wait = Time.time + 2.5f;
                            while (!kickerReady[(int)bot] && Time.time < wait) yield return null;
                            yield return new WaitForSeconds(0.3f);
                        }
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
            if (returning)
            {
                // the rebound may be hit before it bounces twice
                bouncesSinceWall++;
                GameEvents.RaiseBounced(point, bouncesSinceWall, true);
                if (kicker != Me && bouncesSinceWall == 1 && BotRehits()) return;
                if (bouncesSinceWall >= 2 && resolveRoutine == null) resolveRoutine = StartCoroutine(ResolveAfter(0.3f));
                return;
            }
            groundBounces++;
            GameEvents.RaiseBounced(point, groundBounces, hitWindow >= 0 || groundBounces > 0);
            if (groundBounces >= 2 && resolveRoutine == null) resolveRoutine = StartCoroutine(ResolveAfter(0.3f));
        }

        public void OnBallRolling(Vector3 point) { if (kickActive && resolveRoutine == null) resolveRoutine = StartCoroutine(ResolveAfter(0.1f)); }

        /// <summary>The bot "runs" to its rebound: hits it again now and then, aiming at the best window.</summary>
        bool BotRehits()
        {
            if (rallyHits >= 3 || Random.value > botAccuracy * 0.8f) return false;
            int best = -1;
            for (int w = 0; w < Rules.WindowCount; w++)
                if (Rules.IsBreakable(w) && (best < 0 || Rules.ShownValue(w) > Rules.ShownValue(best))) best = w;
            if (best < 0) return false;
            Rekick(kicker, GestureFor(best, 1f - botAccuracy));
            return true;
        }

        public void OnBallFacade(Vector3 point)
        {
            if (!kickActive || hitWindow >= 0 || returning) { GameEvents.RaiseWallHit(point); return; }
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
                if (!Rules.IsBroken(w)) Pop?.Invoke(Rules.IsCovered(w) ? "bus" : "open_window", kicker, PopKind.Poison); // the poison that stopped it
            }
            GameEvents.RaiseWallHit(point);
            // ⚠️ until 2026-11-02 the ball was just dropped here and the action ended: one shot, no rebound.
            // User: «بازیکن نمی‌تونه توپ برگشتی رو بزنه … باید بتونه توپ برگشتی رو هم بزنه … مثل ورژن قبلی بازی».
            SendBack();
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
