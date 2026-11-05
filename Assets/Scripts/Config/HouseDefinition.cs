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
        [Tooltip("Off (default since 2026-10-25): the wall continues upward without end, so a ball can never be lost over the roof. " +
                 "User: 'بهتره پشت بوم نباشه و تصویر تا بالای بالا خود ساختمون باشه'. roofV / worldRoofHeight then only mark where the windows stop.")]
        public bool hasRoof = false;
        [Tooltip("Distance from the player to the facade in metres. Bigger = harder kicks needed.")]
        public float facadeDistance = 12f;
        [Tooltip("Roofline height on the image, normalized 0..1 from the bottom. A ball above it is lost.")]
        [Range(0f, 1f)] public float roofV = 0.9f;
        [Tooltip("Left and right edge of this building's facade on the image (normalized). Outside it, neighbours act as plain walls.")]
        public Vector2 facadeU = new Vector2(0.17f, 0.92f);
        public WindowSpot[] windows = Array.Empty<WindowSpot>();

        [Header("Painted facade (prototype, 3D quad)")]
        [Tooltip("Resources path of the facade painting (e.g. 'Art/house_01'). The prototype shows it on a quad " +
                 "worldFacadeWidth metres wide at facadeDistance; windows come from worldWindows, which the " +
                 "prototype derives from the image's uv rectangles. Empty = coloured boxes.")]
        public string paintedTexture = "";
        [Tooltip("Same painting with every pane smashed (same size, same framing). When set, a broken window shows " +
                 "its own rectangle cut from this picture instead of a generic broken pane (user 2026-11-05).")]
        public string brokenTexture = "";
        [Tooltip("Pixel rect of plain wall in the painting; mirror-tiled beside the house when the screen is wider than the painting.")]
        public Rect wallPatch;
        [Tooltip("Things painted in or on the windows that should move: a pot or cat falls when the window breaks, " +
                 "a curtain flutters, laundry sways in the wind all the time. Pixel rect empty = a default part of the window.")]
        public WindowDetail[] details = Array.Empty<WindowDetail>();

        [Header("Placeholder geometry (used when there is no backdrop)")]
        [Tooltip("Window rectangles directly on the facade plane, in metres: x from the street centre, y from the ground. " +
                 "Used by the placeholder prototype; leave empty when the house has a painted backdrop.")]
        public Rect[] worldWindows = Array.Empty<Rect>();
        [Tooltip("Roofline height in metres for placeholder geometry.")]
        public float worldRoofHeight = 7.2f;
        [Tooltip("Width of this building's facade in metres for placeholder geometry.")]
        public float worldFacadeWidth = 9f;

        public bool UsesWorldGeometry => backdrop == null && worldWindows != null && worldWindows.Length > 0;

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

    public enum DetailKind { Pot, Cat, Curtain, Laundry, Tree }

    [Serializable]
    public struct WindowDetail
    {
        public int window;
        public DetailKind kind;
        [Tooltip("Region on the painting, image pixels, origin top-left. Zero size = default region for the kind.")]
        public Rect pixel;
        public WindowDetail(int window, DetailKind kind) { this.window = window; this.kind = kind; pixel = Rect.zero; }
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
