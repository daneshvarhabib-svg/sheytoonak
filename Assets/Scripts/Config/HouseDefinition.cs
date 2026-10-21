using System;
using UnityEngine;

namespace Sheytoonak
{
    /// <summary>
    /// One house (building) of the street. Its look is a single backdrop image in the style of
    /// docs/reference/street-reference.png; windows and the roofline are marked on that image in
    /// normalized coordinates (0..1, origin bottom-left), so the gameplay lines up with the art
    /// without hand-placing colliders. See <see cref="BackdropWindowMapper"/>.
    /// </summary>
    [CreateAssetMenu(menuName = "Sheytoonak/House", fileName = "House_01")]
    public class HouseDefinition : ScriptableObject
    {
        [Header("Art (frontend)")]
        public Sprite backdrop;
        [Tooltip("Shown on the walk screen and the medal, e.g. 'پلاک ۱۲'.")]
        public string displayName = "پلاک ۱۲";

        [Header("Geometry (gameplay)")]
        [Tooltip("Distance from the player to the facade in metres. Bigger = harder kicks needed.")]
        public float facadeDistance = 12f;
        [Tooltip("Roofline height on the image, normalized 0..1 from the bottom. A ball above it is lost.")]
        [Range(0f, 1f)] public float roofV = 0.9f;
        [Tooltip("Left and right edge of this building's facade on the image (normalized). Outside it, neighbours act as plain walls.")]
        public Vector2 facadeU = new Vector2(0.17f, 0.92f);
        public WindowSpot[] windows = Array.Empty<WindowSpot>();

        [Header("Difficulty")]
        [Tooltip("Sideways wind (m/s² on the ball). Random in this range each serve; negative = blowing left.")]
        public Vector2 windRange = Vector2.zero;
        [Tooltip("Flight time of the ball from the facade to its first bounce near the player (s). Lower = faster returns.")]
        public float returnFlightTime = 1.6f;
        [Tooltip("How far left/right of centre the returning ball may land (m). Bigger = more running.")]
        public float returnSpreadX = 0.8f;
        [Tooltip("Scales every window's hit box. Below 1 makes this house harder without redrawing the art.")]
        [Range(0.3f, 1.2f)] public float windowHitScale = 1f;
    }

    [Serializable]
    public struct WindowSpot
    {
        [Tooltip("Window rectangle on the backdrop image, normalized 0..1 with origin bottom-left.")]
        public Rect uv;
        [Tooltip("Optional: overlay drawn once this window is broken. Frontend falls back to a shared broken-glass sprite.")]
        public Sprite brokenOverlay;
    }
}
