using Sheytoonak.Core;
using UnityEngine;

namespace Sheytoonak
{
    /// <summary>
    /// Every number a designer may want to tune, in one asset.
    /// Create with Assets > Create > Sheytoonak > Game Config.
    /// </summary>
    [CreateAssetMenu(menuName = "Sheytoonak/Game Config", fileName = "GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [Header("Rules")]
        [Min(1)] public int lives = 3;
        [Tooltip("An outgoing kick may hit the street this many times before it counts as 'did not reach'.")]
        [Min(1)] public int maxBouncesBeforeWall = 2;
        [Tooltip("Pause after a broken window or a lost life before the next ball appears.")]
        public float resolveDelay = 1.2f;
        [Tooltip("Length of the walk to the next house; the frontend plays its walk animation during it.")]
        public float walkDuration = 2.0f;

        [Header("Player / foot")]
        [Tooltip("Half width of the strip of sidewalk the player can move along (metres).")]
        public float laneHalfWidth = 2.2f;
        [Tooltip("How fast the player's foot can follow the phone (m/s). Keeps movement physical.")]
        public float footMaxSpeed = 6f;
        [Tooltip("Sideways reach of the kicking foot (m).")]
        public float reachX = 0.6f;
        [Tooltip("How far in front of the player a ball can still be reached with a lunge (m). Behind the player counts up to reachBehind.")]
        public float reachAhead = 2.0f;
        [Tooltip("How far behind the strike spot a ball is still kickable (m).")]
        public float reachBehind = 0.5f;
        [Tooltip("The ball must be lower than this to be kicked (m). Head height: a ball sailing over the player is a real miss.")]
        public float maxKickHeight = 1.9f;
        [Tooltip("Where the ball waits for the serve, in front of the foot (m).")]
        public float serveForwardOffset = 0.45f;

        [Header("Kick")]
        public KickTuning kick = new KickTuning();

        [Header("Motion controls")]
        public MotionTuning motion = new MotionTuning();

        [Header("Ball physics")]
        public float ballRadius = 0.11f;
        [Tooltip("Magnus side force per unit of (spin x speed). Curls a spinning ball.")]
        public float magnusCoefficient = 0.0025f;
        public float ballMass = 0.43f;

        [Header("Facade return")]
        [Tooltip("0 = pure physics reflection, 1 = always lands where it is designed to land. 0.85 keeps it fair but alive.")]
        [Range(0f, 1f)] public float returnAssist = 0.95f; // 0.85 until 2026-10-24; raised to make the return easier to read
        [Tooltip("Where the first bounce of a returning ball lands, measured forward from the player (m).")]
        public Vector2 returnLandingDepth = new Vector2(1.2f, 1.9f); // inside reachAhead so a well-placed player always reaches it

        [Header("Broken windows")]
        [Tooltip("Chance that the owner throws the ball back out of an already broken window (otherwise the ball is lost = -1 life).")]
        [Range(0f, 1f)] public float ownerThrowBackChance = 0.5f;
        public float ownerThrowDelay = 0.9f;
        public float ownerThrowFlightTime = 1.6f;
    }
}
