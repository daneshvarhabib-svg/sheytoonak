using System;
using Sheytoonak.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Sheytoonak
{
    /// <summary>
    /// Touch kick (the game's kick since 2026-10-23) plus editor controls.
    /// Touch or mouse: every tap kicks, and where you tap is the shot (user, 2026-10-24: "هر ضربه هم که
    /// میزنی به توپ هم جهت داشته باشه هم سرعت"): left/right of the screen centre = aim, height on the
    /// screen = power (low tap = soft/low shot, high tap = hard/high shot). The finger's x position also
    /// moves the player, used only when the phone has no gyroscope (see KickInputRouter).
    /// Keyboard: A/D or arrows move, hold Space to charge, release to kick, Q/E aim.
    /// </summary>
    public class TouchKickInput : MonoBehaviour, IKickInput
    {
        [Tooltip("Seconds of holding Space for a full-strength kick (keyboard only).")]
        public float fullCharge = 1.0f;
        [Tooltip("Power of a tap at the very bottom of the screen (0..1); the top of the screen is 1.")]
        public float minTapStrength = 0.35f;
        [Tooltip("Aim at a tap on the screen edge (1 = the full MaxAimYawDeg of the kick tuning).")]
        public float edgeAim = 0.8f;
        [Tooltip("Off (default since 2026-10-27): the player never moves sideways; user: 'حرکت به راست و چپ رو دیگه نداشته باشیم اصلاً'.")]
        public bool moveWithFinger = false;

        public event Action<KickGesture> Kicked;
        public bool IsAvailable => Pointer.current != null || Keyboard.current != null;
        public float Lateral { get; private set; }

        Vector2 pressPos;
        float pressTime;
        bool pressing;
        float charge, keyAim;
        float ignoreUntil;

        /// <summary>A UI button was just tapped: do not read that same press/release as a kick.</summary>
        public void IgnoreFor(float seconds) { ignoreUntil = Time.unscaledTime + seconds; pressing = false; }

        void Update()
        {
            var ptr = Pointer.current;
            if (ptr != null)
            {
                Vector2 pos = ptr.position.ReadValue();
                if (ptr.press.wasPressedThisFrame && Time.unscaledTime >= ignoreUntil) { pressing = true; pressPos = pos; pressTime = Time.unscaledTime; }
                if (pressing && moveWithFinger) Lateral = Mathf.Clamp((pressPos.x / Mathf.Max(1, Screen.width)) * 2f - 1f, -1f, 1f);
                if (pressing && ptr.press.wasReleasedThisFrame)
                {
                    pressing = false;
                    if (Time.unscaledTime - pressTime < 0.8f)
                    {
                        // The release point is the shot: x = direction, y = power.
                        float nx = Mathf.Clamp(pos.x / Mathf.Max(1f, Screen.width) * 2f - 1f, -1f, 1f);
                        float ny = Mathf.Clamp01(pos.y / Mathf.Max(1f, Screen.height));
                        Kicked?.Invoke(new KickGesture(Mathf.Lerp(minTapStrength, 1f, ny), nx * edgeAim, 0f));
                    }
                }
            }

            var kb = Keyboard.current;
            if (kb == null) return;
            float move = (kb.dKey.isPressed || kb.rightArrowKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed || kb.leftArrowKey.isPressed ? 1f : 0f);
            if (move != 0f && moveWithFinger) Lateral = Mathf.Clamp(Lateral + move * 1.5f * Time.deltaTime, -1f, 1f);
            keyAim = (kb.eKey.isPressed ? 1f : 0f) - (kb.qKey.isPressed ? 1f : 0f);
            if (kb.spaceKey.isPressed) charge += Time.deltaTime / Mathf.Max(0.05f, fullCharge);
            if (kb.spaceKey.wasReleasedThisFrame)
            {
                Kicked?.Invoke(new KickGesture(Mathf.Clamp01(charge), keyAim, 0f));
                charge = 0f;
            }
        }

        public void Recenter() => Lateral = 0f;
    }
}
