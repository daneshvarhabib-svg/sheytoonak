using System;
using Sheytoonak.Core;
using UnityEngine;

namespace Sheytoonak
{
    /// <summary>
    /// The single input the game listens to. Phone motion (tilt/slide) moves the player when the
    /// device has a gyroscope, otherwise touch/keyboard does. Kicks come from the touchscreen.
    /// Reason (user, 2026-10-23): "زدن ضربه به توپ از خلال کلیک روی صفحه بگذره، فقط راست و چپش
    /// با حرکت موبایل بمونه" - swinging the phone to kick was unreliable on a real phone; a tap is not.
    /// <see cref="kickFromMotion"/> keeps the old swing-to-kick available for experiments.
    /// </summary>
    public class KickInputRouter : MonoBehaviour, IKickInput
    {
        public MotionKickInput motion;
        public TouchKickInput touch;
        [Tooltip("Off = ignore the phone sensors even if present (settings toggle).")]
        public bool useMotion = true;
        [Tooltip("Off (default) = a phone swing never kicks; only the touchscreen does. Motion still moves the player.")]
        public bool kickFromMotion = false;

        public event Action<KickGesture> Kicked;
        public bool IsAvailable => (motion != null && motion.IsAvailable) || (touch != null && touch.IsAvailable);
        public bool UsingMotion => useMotion && motion != null && motion.IsAvailable;
        public float Lateral => UsingMotion ? motion.Lateral : (touch != null ? touch.Lateral : 0f);

        void OnEnable()
        {
            if (motion != null) motion.Kicked += ForwardMotion;
            if (touch != null) touch.Kicked += Forward;
        }

        void OnDisable()
        {
            if (motion != null) motion.Kicked -= ForwardMotion;
            if (touch != null) touch.Kicked -= Forward;
        }

        void Forward(KickGesture g) => Kicked?.Invoke(g);
        void ForwardMotion(KickGesture g) { if (kickFromMotion) Kicked?.Invoke(g); }

        public void Recenter()
        {
            if (motion != null) motion.Recenter();
            if (touch != null) touch.Recenter();
        }
    }
}
