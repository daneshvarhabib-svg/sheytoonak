using Sheytoonak.Core;
using UnityEngine;

namespace Sheytoonak
{
    /// <summary>A trigger volume where the ball is out of play.</summary>
    public class KillZone : MonoBehaviour
    {
        public LossReason reason = LossReason.OutOfBounds;
    }
}
