using System;

namespace Sheytoonak.Core
{
    /// <summary>Where the rally is right now.</summary>
    public enum RallyPhase
    {
        /// <summary>Ball rests at the player's feet, waiting for the first kick.</summary>
        Serve,
        /// <summary>Ball is flying toward the facade.</summary>
        Outgoing,
        /// <summary>Ball bounced off the facade (or was thrown back) and is coming at the player.</summary>
        Returning,
        /// <summary>A window broke or a life was lost; waiting for the next serve.</summary>
        Resolving,
        /// <summary>Every window of this house is broken; walking to the next house.</summary>
        HouseCleared,
        GameOver,
        Victory
    }

    public enum LossReason
    {
        /// <summary>The returning ball touched the ground a second time.</summary>
        SecondBounce,
        /// <summary>The kick never reached the facade.</summary>
        ShortShot,
        /// <summary>The ball flew over the roofline.</summary>
        OverRoof,
        /// <summary>The ball left the street sideways.</summary>
        OutOfBounds,
        /// <summary>The ball went through an already broken window and the owner kept it.</summary>
        BallLostInHouse,
        /// <summary>The returning ball got past the player.</summary>
        PassedPlayer
    }

    public enum KickRejectReason
    {
        None,
        /// <summary>The ball is not in play for the player (flying out, resolving, game over...).</summary>
        NotYourTurn,
        /// <summary>Tennis rule: the returning ball must touch the ground exactly once before the kick.</summary>
        NeedsBounce,
        OutOfReach
    }

    /// <summary>Result of a ball hitting a window rectangle on the facade.</summary>
    public enum WindowHitResult
    {
        Broke,
        BrokeAndClearedHouse,
        BrokeAndWonGame,
        ThrownBack,
        Lost
    }

    /// <summary>
    /// The complete rule set of the game, free of any Unity type so it can be unit-tested.
    /// The Unity layer reports physical events (kick, bounce, wall, window, kill zone) and
    /// reads back the phase, lives, score and medals. It never decides a rule by itself.
    /// </summary>
    public sealed class RallyRules
    {
        public RallyPhase Phase { get; private set; }
        public int Lives { get; private set; }
        public int Score { get; private set; }
        public int Medals { get; private set; }
        public int HouseIndex { get; private set; }
        public int HouseCount { get; }
        public int BouncesSinceWall { get; private set; }
        public int BouncesBeforeWall { get; private set; }
        public int MaxBouncesBeforeWall { get; }

        bool[] broken = Array.Empty<bool>();

        public event Action<RallyPhase> PhaseChanged;
        public event Action<int> ScoreChanged;
        public event Action<int, LossReason> LifeLost;
        public event Action<int> WindowBroken;
        public event Action<int> MedalEarned;
        public event Action GameOver;
        public event Action Victory;

        /// <param name="lives">Starting lives (design: 3).</param>
        /// <param name="houseCount">Houses in the street (design: 8).</param>
        /// <param name="firstHouseWindows">Window count of house 0.</param>
        /// <param name="maxBouncesBeforeWall">How often an outgoing kick may hit the street before it counts as short.</param>
        public RallyRules(int lives, int houseCount, int firstHouseWindows, int maxBouncesBeforeWall = 2)
        {
            if (lives < 1) throw new ArgumentOutOfRangeException(nameof(lives));
            if (houseCount < 1) throw new ArgumentOutOfRangeException(nameof(houseCount));
            Lives = lives;
            HouseCount = houseCount;
            MaxBouncesBeforeWall = Math.Max(1, maxBouncesBeforeWall);
            LoadHouse(0, firstHouseWindows);
            Phase = RallyPhase.Serve;
        }

        public int WindowCount => broken.Length;
        public bool IsBroken(int window) => broken[window];

        public int WindowsLeft
        {
            get
            {
                int n = 0;
                foreach (var b in broken) if (!b) n++;
                return n;
            }
        }

        public bool IsFinished => Phase == RallyPhase.GameOver || Phase == RallyPhase.Victory;

        /// <summary>May the player kick right now? Reach is checked by the Unity layer and passed in.</summary>
        public KickRejectReason CanKick(bool ballInReach)
        {
            if (Phase == RallyPhase.Serve) return ballInReach ? KickRejectReason.None : KickRejectReason.OutOfReach;
            if (Phase != RallyPhase.Returning) return KickRejectReason.NotYourTurn;
            if (BouncesSinceWall < 1) return KickRejectReason.NeedsBounce;
            return ballInReach ? KickRejectReason.None : KickRejectReason.OutOfReach;
        }

        /// <summary>Call only after <see cref="CanKick"/> returned None and the ball was launched.</summary>
        public void OnKick()
        {
            if (CanKick(true) != KickRejectReason.None)
                throw new InvalidOperationException("Kick is not allowed in phase " + Phase + " with " + BouncesSinceWall + " bounce(s).");
            BouncesBeforeWall = 0;
            BouncesSinceWall = 0;
            SetPhase(RallyPhase.Outgoing);
        }

        public void OnGroundBounce()
        {
            if (Phase == RallyPhase.Outgoing)
            {
                BouncesBeforeWall++;
                if (BouncesBeforeWall >= MaxBouncesBeforeWall) Lose(LossReason.ShortShot);
            }
            else if (Phase == RallyPhase.Returning)
            {
                BouncesSinceWall++;
                if (BouncesSinceWall >= 2) Lose(LossReason.SecondBounce);
            }
        }

        public void OnWallHit()
        {
            // A ball that bounces off the wall again on its way back (rare) restarts the count.
            if (Phase != RallyPhase.Outgoing && Phase != RallyPhase.Returning) return;
            BouncesSinceWall = 0;
            SetPhase(RallyPhase.Returning);
        }

        /// <param name="window">Index into the current house's windows.</param>
        /// <param name="ownerThrowsBack">Only used for an already broken window: decided by the caller's dice roll.</param>
        public WindowHitResult OnWindowHit(int window, bool ownerThrowsBack)
        {
            if (Phase != RallyPhase.Outgoing && Phase != RallyPhase.Returning)
                throw new InvalidOperationException("Window hit in phase " + Phase);

            if (broken[window])
            {
                if (ownerThrowsBack)
                {
                    BouncesSinceWall = 0;
                    SetPhase(RallyPhase.Returning);
                    return WindowHitResult.ThrownBack;
                }
                Lose(LossReason.BallLostInHouse);
                return WindowHitResult.Lost;
            }

            broken[window] = true;
            Score++;
            ScoreChanged?.Invoke(Score);
            WindowBroken?.Invoke(window);

            if (WindowsLeft > 0)
            {
                SetPhase(RallyPhase.Resolving);
                return WindowHitResult.Broke;
            }

            Medals++;
            MedalEarned?.Invoke(Medals);
            if (HouseIndex + 1 >= HouseCount)
            {
                SetPhase(RallyPhase.Victory);
                Victory?.Invoke();
                return WindowHitResult.BrokeAndWonGame;
            }
            SetPhase(RallyPhase.HouseCleared);
            return WindowHitResult.BrokeAndClearedHouse;
        }

        /// <summary>The ball entered a kill zone (roof, sides, behind the player).</summary>
        public void OnBallLost(LossReason reason)
        {
            if (Phase != RallyPhase.Outgoing && Phase != RallyPhase.Returning) return;
            Lose(reason);
        }

        /// <summary>Put a fresh ball at the player's feet after a break or a lost life.</summary>
        public void NextServe()
        {
            if (Phase != RallyPhase.Resolving) throw new InvalidOperationException("NextServe in phase " + Phase);
            BouncesSinceWall = 0;
            BouncesBeforeWall = 0;
            SetPhase(RallyPhase.Serve);
        }

        /// <summary>After the walk animation: move to the next house and serve.</summary>
        public void AdvanceHouse(int windowCount)
        {
            if (Phase != RallyPhase.HouseCleared) throw new InvalidOperationException("AdvanceHouse in phase " + Phase);
            LoadHouse(HouseIndex + 1, windowCount);
            BouncesSinceWall = 0;
            BouncesBeforeWall = 0;
            SetPhase(RallyPhase.Serve);
        }

        void LoadHouse(int index, int windowCount)
        {
            if (windowCount < 1) throw new ArgumentOutOfRangeException(nameof(windowCount), "A house needs at least one window.");
            HouseIndex = index;
            broken = new bool[windowCount];
        }

        void Lose(LossReason reason)
        {
            Lives--;
            LifeLost?.Invoke(Lives, reason);
            if (Lives <= 0)
            {
                SetPhase(RallyPhase.GameOver);
                GameOver?.Invoke();
            }
            else
            {
                SetPhase(RallyPhase.Resolving);
            }
        }

        void SetPhase(RallyPhase p)
        {
            if (Phase == p) return;
            Phase = p;
            PhaseChanged?.Invoke(p);
        }
    }
}
