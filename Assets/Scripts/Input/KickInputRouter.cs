using System;
using Sheytoonak.Core;
using UnityEngine;

namespace Sheytoonak
{
    /// <summary>
    /// The single input the game listens to. Uses phone motion when the device has a gyroscope,
    /// otherwise touch/keyboard; kicks from either source always count (handy while testing).
    /// </summary>
    public class KickInputRouter : MonoBehaviour, IKickInput
    {
        public MotionKickInput motion;
        public TouchKickInput touch;
        [Tooltip("Off = ignore the phone sensors even if present (settings toggle).")]
        public bool useMotion = true;

        public event Action<KickGesture> Kicked;
        public bool IsAvailable => (motion != null && motion.IsAvailable) || (touch != null && touch.IsAvailable);
        public bool UsingMotion => useMotion && motion != null && motion.IsAvailable;
        public float Lateral => UsingMotion ? motion.Lateral : (touch != null ? touch.Lateral : 0f);

        void OnEnable()
        {
            if (motion != null) motion.Kicked += Forward;
            if (touch != null) touch.Kicked += Forward;
        }

        void OnDisable()
        {
            if (motion != null) motion.Kicked -= Forward;
            if (touch != null) touch.Kicked -= Forward;
        }

        void Forward(KickGesture g) => Kicked?.Invoke(g);

        public void Recenter()
        {
            if (motion != null) motion.Recenter();
            if (touch != null) touch.Recenter();
        }
    }
}
