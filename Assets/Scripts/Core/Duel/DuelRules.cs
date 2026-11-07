using System;
using System.Collections.Generic;

namespace Sheytoonak.Core.Duel
{
    public enum Side { Left = 0, Right = 1 }

    public enum KickOutcome
    {
        /// <summary>A sound window broke: the kicker takes its value.</summary>
        Broke,
        /// <summary>Hit a window that was already broken, covered or shut, or the wall: nothing.</summary>
        Bounced,
        /// <summary>Second bounce on the ground, intercepted, lost: nothing.</summary>
        Missed
    }

    public enum ActionReject { None, NotYourTurn, NoActionsLeft, BallInFlight, PoisonWouldCloseEverything, Finished, NoRage, NoTarget }

    /// <summary>A poison sitting on the shared stage.</summary>
    public sealed class ActivePoison
    {
        public Tool Tool;
        public Side By;
        public int TurnsLeft;            // half-turns (each player's turn is one)
        public int[] Windows = new int[0]; // Cover / Close targets
    }

    /// <summary>What the stage does to the next kick of one side, after that side's antidotes.</summary>
    public struct KickModifiers
    {
        public int Accuracy;   // 0..3 extra error steps
        public int Reach;      // 0..3 reach loss steps
        public int Chance;     // 0..3 intercept steps
        public bool Any => Accuracy > 0 || Reach > 0 || Chance > 0;
        public static float ErrorDeg(int steps) => steps * 2f;          // SCENARIO §14
        public static float ReturnLoss(int steps) => steps * 0.10f;
        public static float Intercept(int steps) => steps == 0 ? 0f : 0.05f + steps * 0.10f; // 15/25/35 %
    }

    /// <summary>
    /// The whole duel, free of Unity (docs/SCENARIO.fa.md §2–§5). The scene reports what the ball did;
    /// this decides turns, windows, rage, poisons, the winner and the two currencies.
    /// </summary>
    public sealed class DuelRules
    {
        // Design review 2026-11-07 §2.3: 60 s turns made a duel up to ~16 minutes, half of it watching; 20 s, and a
        // missed turn is a weak automatic kick (DuelManager), not a lost turn.
        public const int TurnSeconds = 20;
        /// <summary>Half-turns after which the side with more windows wins (odd total: no tie). Review §2.3.</summary>
        public const int MaxTurns = 16;
        public const int RageEveryTurns = 3;
        public const int CurseWin = 5, CurseLose = 2;

        public readonly int[] WindowValue;      // 1 / 2 / 3 per window; the 3 is Hidden until it breaks
        public readonly int[] WindowShards;     // shards each window is worth (by size)
        public readonly int HiddenWindow;
        /// <summary>The window of an even house that never breaks (worth 0); -1 when the house is odd. Not shown to anyone.</summary>
        public readonly int SealedWindow;
        public readonly int TotalValue;
        readonly int[] brokenBy;                // -1 sound, else Side
        public readonly HeroDef[][] Heroes = new HeroDef[2][];

        public int[] Windows { get; } = new int[2];       // value collected per side
        public int[] Shards { get; } = new int[2];        // shards collected this round
        public int[] RageTimer { get; } = new int[2];     // own turns until the next rage
        public bool[] RageReady { get; } = new bool[2];
        public int[] Timeouts { get; } = new int[2];      // consecutive
        public Side Current { get; private set; }
        public int Turn { get; private set; }             // 1-based, counts both sides
        public int ActionsLeft { get; private set; }
        public bool KickInFlight { get; private set; }
        public bool Finished { get; private set; }
        public Side? Winner { get; private set; }
        public string FinishReason { get; private set; } = "";
        public List<ActivePoison> Poisons { get; } = new List<ActivePoison>();
        readonly List<ActivePoison>[] protections = { new List<ActivePoison>(), new List<ActivePoison>() };
        readonly Random rng;

        public event Action<Side, int, int> TurnStarted;         // side, turn, actions
        public event Action<Side, ActivePoison> PoisonPlaced;
        public event Action<Side, Tool> AntidoteUsed;
        public event Action<ActivePoison> PoisonExpired;
        public event Action<Side, int, int> WindowTaken;         // side, window, value
        public event Action<Side> RageGained;
        public event Action<Side, string> Ended;                 // winner, reason

        /// <param name="rows">windows per row, bottom row first (the bottom row is the "direct" row worth 1).</param>
        /// <param name="rowOfWindow">optional row of each window index (bottom = 0). ⚠️ the facade's window indices
        /// are NOT bottom-first (run 30: the top row came out worth 1); without this map indices are assumed bottom-first.</param>
        public DuelRules(int[] rows, int[] shardsPerWindow, HeroDef[] left, HeroDef[] right, int seed = 0, int[] rowOfWindow = null)
        {
            rng = seed != 0 ? new Random(seed) : new Random();
            int n = 0; foreach (int r in rows) n += r;
            if (n < 3) throw new ArgumentException("a duel needs at least 3 windows");
            if (rowOfWindow != null && rowOfWindow.Length != n) throw new ArgumentException("row map must match the window count");
            if (rowOfWindow == null)
            {
                rowOfWindow = new int[n];
                int idx = 0;
                for (int r = 0; r < rows.Length; r++)
                    for (int c = 0; c < rows[r]; c++) rowOfWindow[idx++] = r;
            }
            // User (2026-11-05): every window is worth 1, "most windows wins". With an even count there must be no tie;
            // the 2-point window of 2026-11-05 made the total odd but the user saw «۴ ۴» anyway and chose (2026-11-06):
            // «می‌تونیم تو هر دست یه پنجره رو نشون بذاریم. هرچی ضربه بزنی نشکنه. اینطوری هیجانش هم بیشتره». So one upper
            // window of an even house is sealed: it never breaks, scores nothing, and nobody is told which one.
            // SealedWindow names it (-1 when odd); HiddenWindow is the same number, kept for old callers.
            WindowValue = new int[n];
            for (int i = 0; i < n; i++) WindowValue[i] = 1;
            SealedWindow = -1;
            if (n % 2 == 0)
            {
                var upper = new System.Collections.Generic.List<int>();
                for (int i = 0; i < n; i++) if (rowOfWindow[i] > 0) upper.Add(i);
                SealedWindow = upper.Count > 0 ? upper[rng.Next(upper.Count)] : rng.Next(n);
                WindowValue[SealedWindow] = 0;
            }
            HiddenWindow = SealedWindow;
            int total = 0; foreach (int v in WindowValue) total += v;
            TotalValue = total;
            WindowShards = shardsPerWindow ?? new int[n];
            if (WindowShards.Length != n) throw new ArgumentException("shards per window must match the window count");
            brokenBy = new int[n];
            for (int i = 0; i < n; i++) brokenBy[i] = -1;
            Heroes[0] = left; Heroes[1] = right;
            RageTimer[0] = RageTimer[1] = RageEveryTurns;
        }

        public int WindowCount => WindowValue.Length;
        public bool IsBroken(int w) => brokenBy[w] >= 0;
        public int BrokenBy(int w) => brokenBy[w];
        public bool IsHidden(int w) => false; // the secret 3 is gone (2026-11-05)
        /// <summary>What the HUD shows for a window: 1 for every window, the sealed one included (its secret is the point).</summary>
        // Design review 2026-11-07 §2.2: a secret seal felt like a cheat («بازی تقلب کرد»); the sealed window is now shown
        // as such (shutter on the stage, no badge) and nobody targets it.
        public int ShownValue(int w) => WindowValue[w];
        public int Majority => TotalValue / 2 + 1;

        public void Start(Side first)
        {
            Current = first;
            Turn = 1;
            ActionsLeft = 1; // the opener gets one action in the very first turn
            TurnStarted?.Invoke(Current, Turn, ActionsLeft);
        }

        // ------------------------------------------------------------------ stage queries

        public bool IsCovered(int w)
        {
            foreach (var p in Poisons) if (p.Tool.Effect == Effect.Cover && Array.IndexOf(p.Windows, w) >= 0) return true;
            return false;
        }
        public bool IsClosed(int w)
        {
            foreach (var p in Poisons) if (p.Tool.Effect == Effect.Close && Array.IndexOf(p.Windows, w) >= 0) return true;
            return false;
        }
        /// <summary>Sound, open and not covered: can be broken right now.</summary>
        public bool IsBreakable(int w) => w != SealedWindow && !IsBroken(w) && !IsCovered(w) && !IsClosed(w);
        /// <summary>What a player (or the bot) may aim at: a sealed window looks like any other until the ball hits it.</summary>
        public bool LooksBreakable(int w) => IsBreakable(w); // the seal is visible now, so looks = is
        public int BreakableCount() { int c = 0; for (int i = 0; i < WindowCount; i++) if (IsBreakable(i)) c++; return c; }

        public KickModifiers ModifiersFor(Side s)
        {
            var m = new KickModifiers();
            foreach (var p in Poisons)
            {
                int power = Math.Max(0, p.Tool.Power - Protection(s, p.Tool.Effect));
                if (power == 0) continue;
                switch (p.Tool.Effect)
                {
                    case Effect.Accuracy: m.Accuracy = Math.Max(m.Accuracy, power); break;
                    case Effect.Reach: m.Reach = Math.Max(m.Reach, power); break;
                    case Effect.Chance: m.Chance = Math.Max(m.Chance, power); break;
                }
            }
            return m;
        }

        int Protection(Side s, Effect e)
        {
            int best = 0;
            foreach (var a in protections[(int)s]) if (a.Tool.Effect == e) best = Math.Max(best, a.Tool.Power);
            return best;
        }

        // ------------------------------------------------------------------ actions

        public ActionReject CanAct(Side s)
        {
            if (Finished) return ActionReject.Finished;
            if (s != Current) return ActionReject.NotYourTurn;
            if (KickInFlight) return ActionReject.BallInFlight;
            if (ActionsLeft <= 0) return ActionReject.NoActionsLeft;
            return ActionReject.None;
        }

        /// <summary>The kick itself happens in the scene; call this before launching, then <see cref="EndKick"/>.</summary>
        public ActionReject BeginKick(Side s)
        {
            var r = CanAct(s);
            if (r != ActionReject.None) return r;
            KickInFlight = true;
            ActionsLeft--;
            return ActionReject.None;
        }

        /// <summary>The ball came to rest. <paramref name="window"/> is the window it went through, if any.</summary>
        public KickOutcome EndKick(Side s, int window)
        {
            KickInFlight = false;
            var outcome = KickOutcome.Missed;
            if (window >= 0)
            {
                if (IsBreakable(window))
                {
                    brokenBy[window] = (int)s;
                    Windows[(int)s] += WindowValue[window];
                    Shards[(int)s] += WindowShards[window];
                    WindowTaken?.Invoke(s, window, WindowValue[window]);
                    outcome = KickOutcome.Broke;
                    // covering/closing a now-broken window is pointless: drop it from those poisons
                    foreach (var p in Poisons) p.Windows = Array.FindAll(p.Windows, x => x != window);
                }
                else outcome = KickOutcome.Bounced;
            }
            // user (2026-11-04): «قدرت‌ها گاهی هستن و هیچ پنجره‌ی قابل شکستنی نیست». When the last open window breaks and
            // only covered/closed ones are left, the poisons blocking them go away at once: there must always be a target.
            if (!Finished && Windows[(int)s] < Majority && BreakableCount() == 0 && !AllBrokenOrDead())
                for (int i = Poisons.Count - 1; i >= 0; i--)
                {
                    var p = Poisons[i];
                    if (p.Tool.Effect != Effect.Cover && p.Tool.Effect != Effect.Close) continue;
                    Poisons.RemoveAt(i); PoisonExpired?.Invoke(p);
                }
            if (Windows[(int)s] >= Majority) Finish(s, "majority");
            else if (BreakableCount() == 0 && AllBrokenOrDead()) Finish(Windows[0] > Windows[1] ? Side.Left : Side.Right, "no windows left");
            else if (ActionsLeft <= 0) EndTurn();
            return outcome;
        }

        bool AllBrokenOrDead() { for (int i = 0; i < WindowCount; i++) if (!IsBroken(i)) return false; return true; }

        public ActionReject CanPoison(Side s, Tool poison, int[] windows)
        {
            var r = CanAct(s);
            if (r != ActionReject.None) return r;
            if (poison.Effect == Effect.Cover || poison.Effect == Effect.Close)
            {
                // nothing to hide (a bus only reaches the lowest row; once that row is broken it has no target)
                if (windows == null || windows.Length == 0) return ActionReject.NoTarget;
                // never zero: at least one breakable window must survive the poison (SCENARIO §5)
                int left = 0;
                for (int i = 0; i < WindowCount; i++)
                    if (IsBreakable(i) && Array.IndexOf(windows, i) < 0) left++;
                if (left == 0) return ActionReject.PoisonWouldCloseEverything;
            }
            return ActionReject.None;
        }

        /// <summary>Places a poison on the shared stage. For Cover/Close pass the target windows.</summary>
        public ActionReject Poison(Side s, Tool poison, int[] windows = null)
        {
            windows = windows ?? new int[0];
            var r = CanPoison(s, poison, windows);
            if (r != ActionReject.None) return r;
            // +1: the poisoner's own turn ends right after placing it; "one turn" means the opponent's next turn
            var p = new ActivePoison { Tool = poison, By = s, TurnsLeft = (int)poison.Life + 1, Windows = windows };
            Poisons.Add(p);
            ActionsLeft--;
            PoisonPlaced?.Invoke(s, p);
            if (ActionsLeft <= 0) EndTurn();
            return ActionReject.None;
        }

        /// <summary>An antidote protects its user against one effect for two half-turns, and lifts UntilAntidote poisons of that effect.</summary>
        public ActionReject Antidote(Side s, Tool antidote)
        {
            var r = CanAct(s);
            if (r != ActionReject.None) return r;
            protections[(int)s].Add(new ActivePoison { Tool = antidote, By = s, TurnsLeft = 3 }); // own turn, opponent's, own next
            Poisons.RemoveAll(p =>
            {
                bool lift = p.Tool.Life == Lifetime.UntilAntidote && p.Tool.Effect == antidote.Effect && antidote.Power >= p.Tool.Power;
                if (lift) PoisonExpired?.Invoke(p);
                return lift;
            });
            ActionsLeft--;
            AntidoteUsed?.Invoke(s, antidote);
            if (ActionsLeft <= 0) EndTurn();
            return ActionReject.None;
        }

        public ActionReject UseRage(Side s)
        {
            if (Finished) return ActionReject.Finished;
            if (s != Current) return ActionReject.NotYourTurn;
            if (!RageReady[(int)s]) return ActionReject.NoRage;
            RageReady[(int)s] = false;
            RageTimer[(int)s] = RageEveryTurns; // the clock restarts when it is spent (SCENARIO §3)
            ActionsLeft++;
            return ActionReject.None;
        }

        /// <summary>The player gives up the rest of the turn.</summary>
        public ActionReject Pass(Side s)
        {
            if (Finished) return ActionReject.Finished;
            if (s != Current) return ActionReject.NotYourTurn;
            if (KickInFlight) return ActionReject.BallInFlight;
            EndTurn();
            return ActionReject.None;
        }

        /// <summary>The minute ran out. Two in a row = surrender.</summary>
        public void Timeout(Side s)
        {
            if (Finished || s != Current) return;
            Timeouts[(int)s]++;
            if (Timeouts[(int)s] >= 2) { Surrender(s); return; }
            EndTurn();
        }

        public void Surrender(Side s)
        {
            if (Finished) return;
            Finish(s == Side.Left ? Side.Right : Side.Left, "surrender");
        }

        void EndTurn()
        {
            if (Finished) return;
            // age the stage
            for (int i = Poisons.Count - 1; i >= 0; i--)
            {
                var p = Poisons[i];
                if (p.Tool.Life == Lifetime.UntilAntidote) continue;
                if (--p.TurnsLeft <= 0) { Poisons.RemoveAt(i); PoisonExpired?.Invoke(p); }
            }
            foreach (var list in protections)
                for (int i = list.Count - 1; i >= 0; i--) if (--list[i].TurnsLeft <= 0) list.RemoveAt(i);

            Current = Current == Side.Left ? Side.Right : Side.Left;
            Turn++;
            if (Turn > MaxTurns) { Finish(Windows[0] >= Windows[1] ? Side.Left : Side.Right, "turn cap"); return; }
            ActionsLeft = 2;
            int c = (int)Current;
            if (!RageReady[c] && --RageTimer[c] <= 0) { RageReady[c] = true; RageGained?.Invoke(Current); }
            TurnStarted?.Invoke(Current, Turn, ActionsLeft);
        }

        /// <summary>A real turn was played (not timed out): resets the timeout streak of that side.</summary>
        public void NoteActivity(Side s) => Timeouts[(int)s] = 0;

        void Finish(Side winner, string reason)
        {
            Finished = true;
            Winner = winner;
            FinishReason = reason;
            KickInFlight = false;
            Ended?.Invoke(winner, reason);
        }

        /// <summary>Shards and curses after the duel (SCENARIO §2): loser's round shards go to the winner.</summary>
        public (int shards, int curseDelta) RewardFor(Side s)
        {
            if (!Finished || Winner == null) return (0, 0);
            bool won = Winner == s;
            int shards = won ? Shards[0] + Shards[1] : 0;
            return (shards, won ? CurseWin : -CurseLose);
        }

        public static int ApplyCurse(int current, int delta) => Math.Max(0, current + delta);
    }
}
