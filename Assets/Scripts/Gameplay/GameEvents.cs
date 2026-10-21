using System;
using Sheytoonak.Core;
using UnityEngine;

namespace Sheytoonak
{
    /// <summary>
    /// Everything the frontend (UI, audio, VFX, leg animation, backdrop) listens to.
    /// The gameplay code only raises these; it never touches visuals directly.
    /// Subscribe in OnEnable, unsubscribe in OnDisable.
    /// </summary>
    public static class GameEvents
    {
        /// <summary>A new house is active: swap the backdrop, reset broken-glass overlays.</summary>
        public static event Action<HouseDefinition, int> HouseLoaded;
        /// <summary>A fresh ball lies at the player's feet.</summary>
        public static event Action<Vector3> Served;
        /// <summary>Wind for this serve (m/s², + = to the right). Show it on the HUD / sway the trees.</summary>
        public static event Action<float> WindChanged;
        /// <summary>The kick was accepted. Play the kick animation with this strength (0..1) and quality (0..1).</summary>
        public static event Action<KickGesture, float, Vector3> Kicked;
        /// <summary>The player swung but the kick did not count (too early, out of reach...). Play a whiff / hint.</summary>
        public static event Action<KickRejectReason> KickRejected;
        /// <summary>Ball touched the ground. bounceNumber counts since the facade (1 = the allowed bounce).</summary>
        public static event Action<Vector3, int, bool> Bounced; // position, bounceNumber, isReturning
        public static event Action<Vector3> WallHit;
        /// <summary>Glass breaks: index into HouseDefinition.windows, world point of the impact.</summary>
        public static event Action<int, Vector3> WindowBroken;
        /// <summary>The ball flew into an already broken window. thrownBack = the owner throws it out again.</summary>
        public static event Action<int, bool> BallInHouse;
        /// <summary>The owner's throw starts now from this window.</summary>
        public static event Action<int> OwnerThrow;
        public static event Action<int, LossReason> LifeLost;   // lives left, why
        public static event Action<int> ScoreChanged;
        public static event Action<int, HouseDefinition> MedalEarned; // total medals, the cleared house
        /// <summary>Walk to the next house: play the walk/camera move for exactly this many seconds.</summary>
        public static event Action<HouseDefinition, HouseDefinition, float> WalkStarted;
        public static event Action<RallyPhase> PhaseChanged;
        public static event Action<int, int, int> GameOver;  // score, best, medals
        public static event Action<int, int, int> Victory;   // score, best, medals

        internal static void RaiseHouseLoaded(HouseDefinition h, int i) => HouseLoaded?.Invoke(h, i);
        internal static void RaiseServed(Vector3 p) => Served?.Invoke(p);
        internal static void RaiseWindChanged(float w) => WindChanged?.Invoke(w);
        internal static void RaiseKicked(KickGesture g, float q, Vector3 v) => Kicked?.Invoke(g, q, v);
        internal static void RaiseKickRejected(KickRejectReason r) => KickRejected?.Invoke(r);
        internal static void RaiseBounced(Vector3 p, int n, bool returning) => Bounced?.Invoke(p, n, returning);
        internal static void RaiseWallHit(Vector3 p) => WallHit?.Invoke(p);
        internal static void RaiseWindowBroken(int i, Vector3 p) => WindowBroken?.Invoke(i, p);
        internal static void RaiseBallInHouse(int i, bool back) => BallInHouse?.Invoke(i, back);
        internal static void RaiseOwnerThrow(int i) => OwnerThrow?.Invoke(i);
        internal static void RaiseLifeLost(int left, LossReason r) => LifeLost?.Invoke(left, r);
        internal static void RaiseScoreChanged(int s) => ScoreChanged?.Invoke(s);
        internal static void RaiseMedal(int m, HouseDefinition h) => MedalEarned?.Invoke(m, h);
        internal static void RaiseWalkStarted(HouseDefinition a, HouseDefinition b, float d) => WalkStarted?.Invoke(a, b, d);
        internal static void RaisePhaseChanged(RallyPhase p) => PhaseChanged?.Invoke(p);
        internal static void RaiseGameOver(int s, int b, int m) => GameOver?.Invoke(s, b, m);
        internal static void RaiseVictory(int s, int b, int m) => Victory?.Invoke(s, b, m);
    }
}
