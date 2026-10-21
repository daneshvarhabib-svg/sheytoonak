using System;
using Sheytoonak.Core;

namespace Sheytoonak
{
    /// <summary>The two controls of the game, whatever produces them (phone motion, touch, keyboard).</summary>
    public interface IKickInput
    {
        /// <summary>True when this source can work on this device right now.</summary>
        bool IsAvailable { get; }
        /// <summary>-1 (left edge of the lane) .. 1 (right edge).</summary>
        float Lateral { get; }
        event Action<KickGesture> Kicked;
        /// <summary>Treat the current pose as neutral.</summary>
        void Recenter();
    }
}
