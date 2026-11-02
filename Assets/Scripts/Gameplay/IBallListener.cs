using Sheytoonak.Core;
using UnityEngine;

namespace Sheytoonak
{
    /// <summary>What the ball reports to whoever runs the rules (the rally GameManager or the DuelManager).</summary>
    public interface IBallListener
    {
        void OnBallGround(Vector3 point);
        void OnBallRolling(Vector3 point);
        void OnBallFacade(Vector3 point);
        void OnBallKilled(LossReason reason);
    }
}
