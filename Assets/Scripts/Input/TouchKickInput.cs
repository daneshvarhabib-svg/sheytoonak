using System;
using Sheytoonak.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Sheytoonak
{
    /// <summary>
    /// Touch kick (the game's kick since 2026-10-23) plus editor controls.
    /// Touch or mouse: a tap anywhere kicks (tapStrength); holding before lifting charges a harder kick;
    /// an upward flick kicks with strength from its length and aim from its angle. The finger's x
    /// position also moves the player, used only when the phone has no gyroscope (see KickInputRouter).
    /// Keyboard: A/D or arrows move, hold Space to charge, release to kick, Q/E aim.
    /// </summary>
    public class TouchKickInput : MonoBehaviour, IKickInput
    {
        [Tooltip("Upward flick length (fraction of screen height) for a full-strength kick.")]
        public float fullFlick = 0.45f;
        [Tooltip("Seconds of holding Space (or the finger) for a full-strength kick.")]
        public float fullCharge = 1.0f;
        [Tooltip("Strength of a plain quick tap (0..1); hits the lower windows, holding longer hits higher.")]
        public float tapStrength = 0.55f;

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
                if (pressing) Lateral = Mathf.Clamp((pressPos.x / Mathf.Max(1, Screen.width)) * 2f - 1f, -1f, 1f);
                if (pressing && ptr.press.wasReleasedThisFrame)
                {
                    pressing = false;
                    Vector2 d = pos - pressPos;
                    float held = Time.unscaledTime - pressTime;
                    if (d.y > Screen.height * 0.06f && held < 0.6f)
                    {
                        // Upward flick: length = strength, angle = aim.
                        float strength = Mathf.Clamp01(d.y / (Screen.height * fullFlick));
                        float aim = Mathf.Clamp(d.x / Mathf.Max(1f, d.y) * 1.5f, -1f, 1f);
                        Kicked?.Invoke(new KickGesture(strength, aim, 0f));
                    }
                    else if (d.magnitude < Screen.height * 0.06f)
                    {
                        // Tap: a quick tap kicks at tapStrength, holding longer charges towards 1.
                        float strength = Mathf.Max(tapStrength, Mathf.Clamp01(held / Mathf.Max(0.05f, fullCharge)));
                        Kicked?.Invoke(new KickGesture(strength, 0f, 0f));
                    }
                }
            }

            var kb = Keyboard.current;
            if (kb == null) return;
            float move = (kb.dKey.isPressed || kb.rightArrowKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed || kb.leftArrowKey.isPressed ? 1f : 0f);
            if (move != 0f) Lateral = Mathf.Clamp(Lateral + move * 1.5f * Time.deltaTime, -1f, 1f);
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
