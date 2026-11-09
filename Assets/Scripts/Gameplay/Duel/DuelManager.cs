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
            // A shuffled bag: every house comes up once before any repeats (user 2026-11-07: «چرا هنوز خونه های جدید رو
            // نیاوردی تو بازی؟» — they were in the pool, pure random just kept showing the same few).
            if (bag == null || bag.Count == 0)
            {
                bag = new System.Collections.Generic.List<HouseDefinition>(housePool);
                for (int i = bag.Count - 1; i > 0; i--) { int k = Random.Range(0, i + 1); (bag[i], bag[k]) = (bag[k], bag[i]); }
                if (bag.Count > 1 && bag[bag.Count - 1] == house) (bag[0], bag[bag.Count - 1]) = (bag[bag.Count - 1], bag[0]);
            }
            house = bag[bag.Count - 1];
            bag.RemoveAt(bag.Count - 1);
        }
        System.Collections.Generic.List<HouseDefinition> bag;
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
        bool autoKick;
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
        /// <summary>A pigeon knocked the ball down at this point (user 2026-11-05: the bird must fall too, not fly on).</summary>
        public event Action<Vector3> PigeonHit;
        /// <summary>The action is over and the ball is dead where it lies; the stage settles it and brings it back (user: no vanishing).</summary>
        public event Action<Vector3> BallDead;
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

        /// <summary>Challenge duel: the ban/pick draft (review 2026-11-07 §2.5: the draft belongs to the competitive mode).</summary>
        void NewChallengeDuel()
        {
            StopAllCoroutines(); botRoutine = resolveRoutine = null; hitsToCome = false; kickActive = false;
            Rules = null; Draft = null;
            ball.Hide();
            PickHouse();
            mapper.Map(house);
            FitAimToHouse();
            GameEvents.RaiseHouseLoaded(house, 0);
            InChallenge = true;
            StartDraft();
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
            StopAllCoroutines(); botRoutine = resolveRoutine = null; hitsToCome = false; kickActive = false;
            Rules = null; Draft = null; InChallenge = false;
            ball.Hide();
        }

        // Design review 2026-11-07 §2.5: with eight heroes a ban/pick before every duel is dead weight and the first
        // duel must start within 30 s of installing. The quick duel uses the team chosen in the lobby (saved, default
        // the first three); the draft is the challenge's ritual.
        const string PrefTeam = "sheytoonak.team";
        public string[] Team
        {
            get
            {
                var s = PlayerPrefs.GetString(PrefTeam, "");
                var ids = s.Length > 0 ? s.Split(',') : new string[0];
                var ok = new System.Collections.Generic.List<string>();
                foreach (var id in ids) if (System.Array.Exists(StarterHeroes.All, h => h.Id == id) && !ok.Contains(id)) ok.Add(id);
                for (int i = 0; ok.Count < 3 && i < StarterHeroes.All.Length; i++) if (!ok.Contains(StarterHeroes.All[i].Id)) ok.Add(StarterHeroes.All[i].Id);
                return ok.GetRange(0, 3).ToArray();
            }
        }
        public void SetTeam(string[] ids) { PlayerPrefs.SetString(PrefTeam, string.Join(",", ids)); PlayerPrefs.Save(); }

        /// <summary>Quick duel: my saved team against three other heroes dealt at random, no draft.</summary>
        public void NewDuel()
        {
            InChallenge = false;
            StopAllCoroutines(); botRoutine = resolveRoutine = null; hitsToCome = false; kickActive = false;
            Rules = null; Draft = null;
            ball.Hide();
            PickHouse();
            mapper.Map(house);
            FitAimToHouse();
            GameEvents.RaiseHouseLoaded(house, 0);
            var mine = new HeroDef[3]; var team = Team;
            for (int i = 0; i < 3; i++) mine[i] = StarterHeroes.Get(team[i]);
            var pool = new System.Collections.Generic.List<HeroDef>(StarterHeroes.All);
            pool.RemoveAll(h => System.Array.IndexOf(team, h.Id) >= 0);
            var theirs = new HeroDef[3];
            for (int i = 0; i < 3; i++) { int k = Random.Range(0, pool.Count); theirs[i] = pool[k]; pool.RemoveAt(k); }
            BeginDuel(mine, theirs, Random.value < 0.5f ? Side.Left : Side.Right);
        }

        /// <summary>Starts the pre-duel draft (ban/pick); the duel itself begins when the draft is done.</summary>
        void StartDraft()
        {
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
            Rules.WindowTaken += (s, w, v) => { if (v > 1) Toast?.Invoke(Ar.N(v) + " ★", 1.6f); };
            Rules.Ended += OnEnded;
            Rules.RageGained += s => Pop?.Invoke("rage", s, PopKind.Info);
            PlaceBallAtCentre();
            Curses[1] = Mathf.Max(0, Curses[0] + Random.Range(-3, 4)); // a "matched" opponent
            GameEvents.RaiseRoundStarted();
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
        /// <summary>How wide a run of windows a painted vehicle hides (metres of facade); 0 = not a vehicle.</summary>
        public static float VehicleReach(string id) => id == "bus" ? 8.5f : id == "truck" ? 7.5f : id == "cart" ? 3.2f : 0f;

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
            TurnTimeLeft = DuelRules.TurnSeconds; autoKick = false;
            foot.Hold = true; // user (2026-11-06): «بازیکن وقتی ضربه میزنه چرا حرکت می‌کنه؟ نباید افقی بره» — phone tilt moved the kicker; nobody moves now
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
                if (!kickActive && BallSettled && ball.gameObject.activeSelf && ball.Body != null && ball.Body.isKinematic)
                    ball.transform.position = foot.StrikeSpot + Vector3.up * ball.Radius;
                // ⚠️ The clock only runs while the player is being waited for. With 20 s turns it used to run through the
                // rally too, and a timeout in the middle of the third hit ended the turn under a flying ball (user 2026-11-07:
                // «ضربه سوم رو که می‌زنی توپ غیب می‌شه و درست حرکت نمیکنه»).
                if (!kickActive && BallSettled && !hitsToCome) TurnTimeLeft -= Time.deltaTime;
                if (autoKick && Rules.Current == Me && KickArmed && BallSettled && !kickActive && !hitsToCome)
                {
                    autoKick = false;
                    Kick(Me, new KickGesture(0.35f, Random.Range(-0.25f, 0.25f), 0f), -1);
                }
                if (TurnTimeLeft <= 0f && !kickActive && BallSettled)
                {
                    Pop?.Invoke("clock", Rules.Current, PopKind.Info);
                    var who = Rules.Current;
                    // Review 2026-11-07 §2.3: my missed turn is a weak kick by whoever is chosen (or the first hero), not a
                    // lost turn; a second miss in a row is still the usual timeout.
                    if (who == Me && !autoKick && Rules.CanAct(Me) == ActionReject.None && !kickActive && !hitsToCome && Rules.Timeouts[(int)Me] == 0)
                    {
                        autoKick = true;
                        Rules.Timeouts[(int)Me]++;
                        if (!KickArmed) { if (Selected < 0) Select(0); ArmKick(); }
                        TurnTimeLeft = 4f; // the walk up and the kick; then the turn really ends
                    }
                    else
                    {
                        autoKick = false;
                        Rules.Timeout(who);
                        if (!Rules.Finished) TurnTimeLeft = DuelRules.TurnSeconds;
                    }
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
            // a bag comes from a win (any duel, challenge too), not from time; a full rack gets nothing.
            // Review 2026-11-07 §2.6: no timers — the bag you tapped opens with your next win.
            if (winner == Me) { Bags.WinUnlocks(); Bags.AddFromWin(); }
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
            if (poison.Effect == Effect.Cover && VehicleReach(poison.Id) > 0f)
            {
                // User (2026-11-05): «گاری گاهی سایزش خیلی بزرگه … اتوبوس هم گاهی خیلی بزرگ می‌شه». The vehicle used to grow to
                // whatever it had to hide: once the bottom row was broken it covered the next row up and became a giant.
                // A vehicle has its real size now, so it can only hide the house's lowest row, and only as many windows
                // as its length reaches; with that row broken it has no target.
                // ⚠️ user (2026-11-06): «سم‌هایی مثل اتوبوس و کامیون و گاری قفله». On the wide paintings the ground floor is a
                // barred window at one end and a shop front wider than a bus at the other: the exact-row test split them
                // and the "whole window inside the reach" test rejected the shop, so no vehicle ever had a target. Now
                // the ground floor is every window whose sill is within 1.5 m of the lowest sill, and a vehicle counts a
                // window as hidden when its centre is within the reach (it parks in front of a wide shop and hides it).
                float lowest = float.MaxValue;
                foreach (int w in list) lowest = Mathf.Min(lowest, mapper.WindowRects[w].yMin);
                var low = list.FindAll(w => mapper.WindowRects[w].yMin <= lowest + 1.5f);
                low.Sort((a, b) => mapper.WindowRects[a].x.CompareTo(mapper.WindowRects[b].x));
                float reach = VehicleReach(poison.Id);
                System.Collections.Generic.List<int> best = new System.Collections.Generic.List<int>();
                for (int i = 0; i < low.Count; i++)
                {
                    var run = new System.Collections.Generic.List<int>();
                    for (int j = i; j < low.Count && mapper.WindowRects[low[j]].center.x - mapper.WindowRects[low[i]].center.x <= reach; j++) run.Add(low[j]);
                    if (run.Count > best.Count) best = run;
                }
                int keep = Mathf.Min(best.Count, list.Count - 1);
                return best.GetRange(0, Mathf.Max(0, keep)).ToArray();
            }
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
                // user (2026-11-06): «سه بار فرصت داره تا یه بار توپ بخوره زمین دوباره ضربه بزنه … رو هوا ضربه نمی‌زنه بعد از یه بار
                // زمین خوردن». Once the rebound has bounced it may be struck wherever it is in the lane (the kid runs to it);
                // before the bounce only when it is actually at the foot.
                bool reachable = foot.BallInReach(ball.transform.position, ball.Radius) || (bouncesSinceWall >= 1 && Mathf.Abs(ball.transform.position.x) <= config.laneHalfWidth + 1.5f);
                if (kicker == Me && returning && ball.InPlay && rallyHits + 1 < HitsPerBall && reachable) Rekick(Me, g);
                return;
            }
            if (hitsToCome && kicker == Me)
            {
                if (BallSettled) Kick(Me, g, -1); // the same action: another hit on the ball that came back
                return;
            }
            var why = Rules.CanAct(Me);
            if (why != ActionReject.None) { return; }
            if (!KickArmed) { Pop?.Invoke("pick", Me, PopKind.Info); return; } // first a hero, then its kick button
            if (!BallSettled) return; // the ball is still rolling back from the last action
            Kick(Me, g, -1);
        }

        void Kick(Side s, KickGesture g, int target)
        {
            bool again = hitsToCome && s == kicker;
            if (!again && Rules.BeginKick(s) != ActionReject.None) return;
            FitAimToHouse();
            int hitsSoFar = again ? rallyHits + 1 : 0;
            hitsToCome = false;
            Rules.NoteActivity(s);
            var mods = Rules.ModifiersFor(s);
            // Poison physics: wind scatters the aim, a puddle takes the power out of the kick
            if (mods.Accuracy > 0) g.Aim += Random.Range(-1f, 1f) * KickModifiers.ErrorDeg(mods.Accuracy) / Mathf.Max(1f, config.kick.MaxAimYawDeg);
            if (mods.Reach > 0) g.Strength01 *= 1f - 0.18f * mods.Reach;
            Vector3 b = PlaceBallAtCentre();
            float quality = 1f;
            var res = KickSolver.SolveAtWall(g, 0f, quality, config.kick, Random.Range(-1f, 1f), Random.Range(-1f, 1f),
                                             mapper.FacadeZ - b.z, b.y, mapper.MaxTargetHeight, mapper.MinTargetHeight(config.kick.MinTargetHeight));
            var v = new Vector3(res.Velocity.X, res.Velocity.Y, res.Velocity.Z);
            ball.Launch(v, res.SpinY, 0f);
            kicker = s; kickActive = true; hitWindow = -1; groundBounces = 0; kickStarted = Time.time;
            returning = false; bouncesSinceWall = 0; rallyHits = hitsSoFar;
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
                                             mapper.FacadeZ - b.z, b.y, mapper.MaxTargetHeight, mapper.MinTargetHeight(config.kick.MinTargetHeight));
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
            // User (2026-11-07): the rebound used to land up to returnSpreadX (~2 m) left or right of the kicker, so he
            // ran sideways after every hit. It now comes back onto his own line, with at most a hand's width of wobble.
            float spread = Mathf.Min(0.15f, house != null ? house.returnSpreadX : 0.15f);
            float x = Mathf.Clamp(foot.X + Random.Range(-1f, 1f) * spread, -config.laneHalfWidth, config.laneHalfWidth);
            float z = foot.Z + Random.Range(config.returnLandingDepth.x, config.returnLandingDepth.y);
            float flight = house != null ? house.returnFlightTime : 1.6f;
            if (rallyHits + 1 >= HitsPerBall)
            {
                // User (2026-11-09): «اگر نشد و خورد به دیوار از همون جایی که خورده به دیوار با سرعت کمتر برگرده سمت بازیکن».
                // The last hit has no follow-up, so its rebound is not shaped for the foot: it drops off the wall slower and
                // lands well short of the kicker, bounces twice there and rolls the rest of the way (no vanishing: it used to
                // land at the foot, bounce on past him into the kill zone behind and get re-placed in front).
                z = foot.Z + Random.Range(3.2f, 4.6f);
                flight *= 1.15f;
                real *= 0.5f;
            }
            var d = KickSolver.BallisticVelocity(new Vec3(from.x, from.y, from.z), new Vec3(x, ball.Radius, z), flight, -Physics.gravity.y);
            ball.SetVelocity(Vector3.Lerp(real, new Vector3(d.X, d.Y, d.Z), config.returnAssist));
            returning = true; bouncesSinceWall = 0;
        }

        /// <summary>True while a swipe now would kick: my turn with the ball at the foot, or a live rebound I may still hit.</summary>
        public bool CanAimNow
        {
            get
            {
                if (Rules == null || Rules.Finished || Rules.Current != Me) return false;
                if (kickActive) return kicker == Me && returning && ball != null && ball.InPlay && rallyHits + 1 < HitsPerBall;
                return BallSettled && KickArmed;
            }
        }

        /// <summary>The launch a gesture would give right now, without the random contact error (for the aim arc).</summary>
        public Vector3 PreviewLaunch(KickGesture g, out Vector3 from)
        {
            FitAimToHouse();
            from = kickActive && ball != null ? ball.transform.position : foot.StrikeSpot + Vector3.up * ball.Radius;
            var res = KickSolver.SolveAtWall(g, 0f, 1f, config.kick, 0f, 0f, mapper.FacadeZ - from.z, from.y, mapper.MaxTargetHeight, mapper.MinTargetHeight(config.kick.MinTargetHeight));
            return new Vector3(res.Velocity.X, res.Velocity.Y, res.Velocity.Z);
        }

        /// <summary>The glass that ends the duel just broke (the stage slows it down).</summary>
        public event Action<Vector3> FinalBlow;

        IEnumerator Intercept(float after)
        {
            yield return new WaitForSeconds(after);
            if (!kickActive || !ball.InPlay) yield break;
            Pop?.Invoke("pigeon", kicker, PopKind.Poison);
            GameEvents.RaiseWallHit(ball.transform.position); // a thud and a shake
            PigeonHit?.Invoke(ball.transform.position);       // the stage drops the bird where the ball met it
            ball.SetVelocity(new Vector3(Random.Range(-1f, 1f), 1.5f, -2.5f));
        }

        /// <summary>False while the dead ball is still being brought back to the foot (no kick can start).</summary>
        public bool BallSettled { get; private set; } = true;
        /// <summary>The stage finished bringing the ball back: rest it at the foot.</summary>
        public void BallReturned() { PlaceBallAtCentre(); BallSettled = true; }

        Vector3 PlaceBallAtCentre()
        {
            Vector3 spot = foot.StrikeSpot + Vector3.up * ball.Radius;
            ball.PlaceAtRest(spot);
            return spot;
        }

        /// <summary>
        /// User (run 110): the far-left and far-right windows could never be hit and the duel never ended. The aim's full
        /// swing used to be a fixed 25 degrees (about 6 m at the facade); a 3:1 painting is 30 m wide. The yaw at Aim = ±1
        /// now reaches a little past the outermost window of this house.
        /// </summary>
        void FitAimToHouse()
        {
            // ⚠️ run 117 (user): the outermost windows were still out of reach. This ran once at NewDuel, before the stage
            // scaled the painting to the camera (the rects doubled afterwards). Now it is also called at every kick, on the
            // rects as they are then, from where the ball actually lies.
            float far = 0f;
            foreach (var r in mapper.WindowRects) far = Mathf.Max(far, Mathf.Abs(r.center.x) + r.width * 0.5f);
            float d = Mathf.Max(1f, mapper.FacadeZ - foot.StrikeSpot.z);
            config.kick.MaxAimYawDeg = Mathf.Max(25f, Mathf.Atan2(far + Mathf.Abs(foot.StrikeSpot.x) + 0.6f, d) * Mathf.Rad2Deg);
        }

        /// <summary>Aim gesture that lands on a window's centre (inverse of KickSolver.SolveAtWall), with the bot's error.</summary>
        KickGesture GestureFor(int window, float error)
        {
            FitAimToHouse();
            Rect r = mapper.WindowRects[window];
            float d = mapper.FacadeZ - foot.StrikeSpot.z;
            float yaw = Mathf.Atan2(r.center.x, d) * Mathf.Rad2Deg;
            float aim = yaw / Mathf.Max(1f, config.kick.MaxAimYawDeg);
            float s = Mathf.InverseLerp(mapper.MinTargetHeight(config.kick.MinTargetHeight), mapper.MaxTargetHeight, r.center.y);
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
                    case BotMove.Kind.Poison:
                        // its own target guess may differ from TargetsFor (a vehicle with nothing left to hide): pass, never loop
                        if (Act(bot, move.Hero, true) != ActionReject.None) Rules.Pass(bot);
                        break;
                    case BotMove.Kind.Kick:
                        if (!kickerReady[(int)bot])
                        {
                            KickerChosen?.Invoke(bot, botKicks++ % 3);
                            float wait = Time.time + 2.5f;
                            while (!kickerReady[(int)bot] && Time.time < wait) yield return null;
                            yield return new WaitForSeconds(0.3f);
                        }
                        while (!BallSettled) yield return null;
                        Kick(bot, GestureFor(move.Window, 1f - botAccuracy), move.Window);
                        while (Rules.KickInFlight)
                        {
                            if (hitsToCome && kicker == bot && BallSettled)
                            {
                                yield return new WaitForSeconds(0.5f);
                                var again = Bot.Next(Rules, bot);
                                int w = again.What == BotMove.Kind.Kick ? again.Window : move.Window;
                                Kick(bot, GestureFor(w, 1f - botAccuracy), w);
                            }
                            yield return null;
                        }
                        break;
                    default: Rules.Pass(bot); break;
                }
            }
            botRoutine = null;
        }

        /// <summary>Hits already made on this ball in the current action (first kick + rekicks).</summary>
        public int HitsUsed => kickActive || hitsToCome ? rallyHits + 1 : 0;
        // User (2026-11-09): «به جز ضربه اول ۳ ضربه داریم» -- the kick plus three hits on the rebound.
        public const int HitsPerBall = 4;
        bool hitsToCome; // the ball is rolling back to the foot and the same action goes on with another hit

        void FinishKick()
        {
            if (!kickActive) return;
            kickActive = false;
            // User (2026-11-05): «هر توپ رو جدا از اولش که میزنی تا سه ضربه باید بشه زد». A ball that did not break glass
            // comes back to the foot and the same action continues, up to three hits in all; only then the action ends.
            // ⚠️ until 2026-11-08 a rebound that bounced twice was rolled back to the foot and the same action went on from
            // rest ("hitsToCome"). User: «ما فقط و فقط در صورتی می‌تونیم با حداکثر سه ضربه به توپ شیشه رو بشکونیم که توپ
            // بیشتر از یه بار زمین نخورده باشه و اگر بیشتر از یه بار زمین بخوره یا بخوره به شیشه نوبتمون می‌سوزه». So the
            // extra hits exist only on the live rebound (Rekick, before its second bounce); two bounces end the turn.
            hitsToCome = false;
            var outcome = Rules.EndKick(kicker, hitWindow);
            LastHitWindow = hitWindow;
            if (Rules.Finished) return;
            // User (2026-11-05): «توپ یهویی غیب می‌شه و ضربه برنمی‌گرده … توپ برگرده ولی دوبار بخوره زمین و بادش کم شه انگار».
            // The ball is no longer teleported to the foot: where it died it goes limp (BallDead), the stage rolls it back
            // to the kicker; until it is there no kick can start (BallSettled false).
            // User (2026-11-08): «تو ضربه سوم حتماً دوبار زمین بخوره و بیاد سمت بازیکن … نه این که توپی که دو ثانیه پیش شوت
            // کردیم یهویی قل بخوره». A ball that already bounced twice on its own is not re-dropped with fake hops: it
            // rolls on from where it really is (DeadBallRolls); the hops are only for a ball killed out of play.
            DeadBallRolls = bouncesSinceWall >= 2 || groundBounces >= 2;
            if (ball.InPlay && BallDead != null) { BallSettled = false; BallDead.Invoke(DeadBallRolls ? RollingBallSpot() : DeadBallSpot()); }
            else PlaceBallAtCentre();
        }

        /// <summary>True while the dead ball should just roll home from its real spot (it bounced twice itself).</summary>
        public bool DeadBallRolls { get; private set; }

        Vector3 RollingBallSpot()
        {
            Vector3 p = ball.transform.position;
            float zMin = foot.Z + 0.6f, zMax = mapper.FacadeZ - 1f;
            float xMax = config.laneHalfWidth + 2.5f;
            return new Vector3(Mathf.Clamp(p.x, -xMax, xMax), ball.Radius, Mathf.Clamp(p.z, zMin, zMax));
        }

        /// <summary>
        /// Where the dead ball starts its roll back: its own spot, pulled into the visible lane. A ball that was killed
        /// behind the camera, off the lane or past the facade used to be "settled" from there (invisible) and then rolled
        /// in from nowhere for seconds (user 2026-11-07).
        /// </summary>
        Vector3 DeadBallSpot()
        {
            Vector3 p = ball.transform.position;
            float zMin = foot.Z + 0.6f, zMax = mapper.FacadeZ - 1f;
            // x stays near the kicker so the roll back is straight, not a diagonal that drags him sideways (2026-11-07)
            float xMax = 0.6f;
            return new Vector3(Mathf.Clamp(p.x, foot.X - xMax, foot.X + xMax), ball.Radius, Mathf.Clamp(p.z, zMin, zMax));
        }

        /// <summary>
        /// The ball bounced twice on its own: let physics carry it until it is nearly still (or 2.5 s), then the stage
        /// rolls it home from where it really is. Taking over 0.45 s after the bounce froze a ball still moving at speed
        /// and then "rolled" it back in a second (user 2026-11-08).
        /// </summary>
        IEnumerator ResolveWhenSlow()
        {
            float until = Time.time + 2.5f;
            while (Time.time < until && ball.InPlay && ball.Body != null && ball.Body.linearVelocity.magnitude > 1.2f) yield return null;
            resolveRoutine = null;
            FinishKick();
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
                // the second bounce ends the turn whatever hits are left (user 2026-11-08); the ball keeps rolling under
                // physics for a moment and then rolls home from where it is (FinishKick -> DeadBallRolls)
                if (bouncesSinceWall >= 2 && resolveRoutine == null) resolveRoutine = StartCoroutine(ResolveWhenSlow());
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
            if (rallyHits + 1 >= HitsPerBall || Random.value > botAccuracy * 0.8f) return false;
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
                    if (Rules.Windows[(int)kicker] + Rules.WindowValue[w] >= Rules.Majority) FinalBlow?.Invoke(point);
                    GameEvents.RaiseWindowBroken(w, point);
                    if (resolveRoutine == null) resolveRoutine = StartCoroutine(ResolveAfter(1.0f));
                    return;
                }
                if (w == Rules.SealedWindow) Toast?.Invoke(Ar.Sealed, 1.4f); // the one window of an even house that never breaks
                else if (!Rules.IsBroken(w)) Pop?.Invoke(Rules.IsCovered(w) ? "bus" : "open_window", kicker, PopKind.Poison); // the poison that stopped it
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
