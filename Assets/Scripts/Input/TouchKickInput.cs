using System;
using Sheytoonak.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Sheytoonak
{
    /// <summary>
    /// Fallback / editor controls so the game can be played without a phone:
    /// touch or mouse: the finger's x position moves the player, a quick upward flick kicks
    /// (longer flick = harder, its angle = aim). Keyboard: A/D or arrows move, hold Space to charge, release to kick, Q/E aim.
    /// </summary>
    public class TouchKickInput : MonoBehaviour, IKickInput
    {
        [Tooltip("Upward flick length (fraction of screen height) for a full-strength kick.")]
        public float fullFlick = 0.45f;
        [Tooltip("Seconds of holding Space for a full-strength kick.")]
        public float fullCharge = 1.0f;

        public event Action<KickGesture> Kicked;
        public bool IsAvailable => Pointer.current != null || Keyboard.current != null;
        public float Lateral { get; private set; }

        Vector2 pressPos;
        float pressTime;
        bool pressing;
        float charge, keyAim;

        void Update()
        {
            var ptr = Pointer.current;
            if (ptr != null)
            {
                Vector2 pos = ptr.position.ReadValue();
                if (ptr.press.wasPressedThisFrame) { pressing = true; pressPos = pos; pressTime = Time.unscaledTime; }
                if (pressing) Lateral = Mathf.Clamp((pressPos.x / Mathf.Max(1, Screen.width)) * 2f - 1f, -1f, 1f);
                if (pressing && ptr.press.wasReleasedThisFrame)
                {
                    pressing = false;
                    Vector2 d = pos - pressPos;
                    float dt = Time.unscaledTime - pressTime;
                    if (d.y > Screen.height * 0.06f && dt < 0.6f)
                    {
                        float strength = Mathf.Clamp01(d.y / (Screen.height * fullFlick));
                        float aim = Mathf.Clamp(d.x / Mathf.Max(1f, d.y) * 1.5f, -1f, 1f);
                        Kicked?.Invoke(new KickGesture(strength, aim, 0f));
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
