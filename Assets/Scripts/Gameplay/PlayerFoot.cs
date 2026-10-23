using Sheytoonak.Core;
using UnityEngine;

namespace Sheytoonak
{
    /// <summary>
    /// Where the player's kicking foot is. Follows the input's lateral value along the sidewalk,
    /// limited to a running speed so it feels physical. The frontend parents the first-person
    /// legs to this transform and reads <see cref="Velocity"/> to blend a side-step animation.
    /// World convention: x = right, y = up, z = toward the facade; the player stands at z = 0.
    /// </summary>
    public class PlayerFoot : MonoBehaviour
    {
        public float X { get; private set; }
        public float Velocity { get; private set; }
        public float Z => transform.position.z;

        GameConfig cfg;
        IKickInput input;

        public void Init(GameConfig config, IKickInput kickInput)
        {
            cfg = config;
            input = kickInput;
            X = 0f;
            Apply();
        }

        public void ResetToCentre()
        {
            X = 0f;
            Velocity = 0f;
            Apply();
        }

        void Update()
        {
            if (cfg == null || input == null) return;
            float target = KickSolver.Clamp(input.Lateral, -1f, 1f) * cfg.laneHalfWidth;
            float next = Mathf.MoveTowards(X, target, cfg.footMaxSpeed * Time.deltaTime);
            Velocity = Time.deltaTime > 0f ? (next - X) / Time.deltaTime : 0f;
            X = next;
            Apply();
        }

        void Apply()
        {
            var p = transform.position;
            transform.position = new Vector3(X, p.y, p.z);
        }

        /// <summary>The spot in front of the foot where the ball is served and ideally struck.</summary>
        public Vector3 StrikeSpot => new Vector3(X, 0f, Z + cfg.serveForwardOffset);

        /// <summary>Can the player hit the ball right now? See <see cref="ReachRule"/> for the rule and why.</summary>
        public bool BallInReach(Vector3 ballCentre, float ballRadius)
        {
            Vector3 s = StrikeSpot;
            return ReachRule.InReach(new Vec3(ballCentre.x, ballCentre.y, ballCentre.z), ballRadius,
                new Vec3(s.x, s.y, s.z), cfg.reachX, cfg.reachBehind, cfg.reachAhead, cfg.maxKickHeight);
        }

        /// <summary>-1..1: ball left/right of the foot relative to the reach. Feeds the contact yaw and quality.</summary>
        public float ContactOffset(Vector3 ballCentre) => KickSolver.Clamp((ballCentre.x - X) / Mathf.Max(0.01f, cfg.reachX), -1f, 1f);
    }
}
