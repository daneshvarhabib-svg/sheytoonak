using Sheytoonak.Core;
using UnityEngine;

namespace Sheytoonak
{
    public enum BackdropFit
    {
        /// <summary>Image fills the screen, the overflowing edges are cropped (recommended for phones).</summary>
        Cover,
        /// <summary>Whole image visible, bars on the sides or top.</summary>
        Contain
    }

    /// <summary>
    /// Bridges the painted backdrop and the 3D physics. The facade is an invisible vertical plane
    /// at the house's facadeDistance; every window marked on the image (normalized uv) is cast from
    /// the camera onto that plane, so a ball hitting the plane can be checked against exactly the
    /// windows the player sees. Call <see cref="Map"/> whenever the house, the camera or the screen size changes.
    /// </summary>
    public class BackdropWindowMapper : MonoBehaviour
    {
        [Tooltip("The camera that renders the scene over the backdrop. Its pose must match the reference image.")]
        public Camera cam;
        public BackdropFit fit = BackdropFit.Cover;
        [Tooltip("Invisible box collider standing in for the facade. Positioned and sized by Map().")]
        public BoxCollider facadeCollider;
        [Tooltip("Trigger behind the facade; a ball reaching it went over the roof (OverRoof). Positioned by Map().")]
        public BoxCollider roofKillZone;
        public float facadeThickness = 0.4f;

        public HouseDefinition House { get; private set; }
        public Rect[] WindowRects { get; private set; } = new Rect[0]; // in facade plane coords: x = world x, y = world y
        public float FacadeZ { get; private set; }
        public float RoofHeight { get; private set; }
        public Vector2 FacadeX { get; private set; }

        Vector2Int mappedScreen;

        public void Map(HouseDefinition house)
        {
            House = house;
            if (house == null || cam == null) return;
            FacadeZ = transform.position.z + house.facadeDistance;
            mappedScreen = new Vector2Int(cam.pixelWidth, cam.pixelHeight);

            if (house.UsesWorldGeometry)
            {
                // Placeholder prototype: windows are real 3D quads, there is no image to cast from.
                WindowRects = new Rect[house.worldWindows.Length];
                for (int i = 0; i < WindowRects.Length; i++)
                {
                    Rect wr = house.worldWindows[i];
                    Vector2 ws = wr.size * house.windowHitScale;
                    WindowRects[i] = new Rect(wr.center - ws * 0.5f, ws);
                }
                RoofHeight = house.worldRoofHeight;
                FacadeX = new Vector2(-house.worldFacadeWidth * 0.5f, house.worldFacadeWidth * 0.5f);
                PlaceColliders();
                return;
            }

            WindowRects = new Rect[house.windows.Length];
            for (int i = 0; i < house.windows.Length; i++)
            {
                Rect uv = house.windows[i].uv;
                Vector3 a = UvToFacade(new Vector2(uv.xMin, uv.yMin));
                Vector3 b = UvToFacade(new Vector2(uv.xMax, uv.yMax));
                var r = Rect.MinMaxRect(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y), Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y));
                // Shrink/grow around the centre for difficulty without touching the art.
                Vector2 c = r.center, s = r.size * house.windowHitScale;
                WindowRects[i] = new Rect(c - s * 0.5f, s);
            }

            float midU = (house.facadeU.x + house.facadeU.y) * 0.5f;
            RoofHeight = UvToFacade(new Vector2(midU, house.roofV)).y;
            FacadeX = new Vector2(UvToFacade(new Vector2(house.facadeU.x, house.roofV)).x, UvToFacade(new Vector2(house.facadeU.y, house.roofV)).x);
            PlaceColliders();
        }

        void PlaceColliders()
        {
            if (facadeCollider != null)
            {
                // One wide wall: neighbours are walls too, only the window rects and roofline are special.
                float width = 80f;
                facadeCollider.transform.position = new Vector3(0f, RoofHeight * 0.5f, FacadeZ + facadeThickness * 0.5f);
                facadeCollider.transform.rotation = Quaternion.identity;
                facadeCollider.size = new Vector3(width, Mathf.Max(0.5f, RoofHeight), facadeThickness);
                facadeCollider.center = Vector3.zero;
            }
            if (roofKillZone != null)
            {
                roofKillZone.isTrigger = true;
                // Everything behind the facade: the only way there is over the roof.
                roofKillZone.transform.position = new Vector3(0f, 25f, FacadeZ + facadeThickness + 2.5f);
                roofKillZone.transform.rotation = Quaternion.identity;
                roofKillZone.size = new Vector3(80f, 50f, 4f);
                roofKillZone.center = Vector3.zero;
            }
        }

        void LateUpdate()
        {
            if (House != null && !House.UsesWorldGeometry && cam != null && (cam.pixelWidth != mappedScreen.x || cam.pixelHeight != mappedScreen.y))
                Map(House); // rotation / resize changes the crop of a Cover backdrop
        }

        /// <summary>Index of the window under a point on the facade plane, or -1.</summary>
        public int FindWindow(Vector3 facadePoint, float ballRadius)
        {
            var p = new Vector2(facadePoint.x, facadePoint.y);
            for (int i = 0; i < WindowRects.Length; i++)
            {
                Rect r = WindowRects[i];
                // The ball's centre must be inside the frame by a bit, so grazing the frame is a wall hit.
                Rect inner = new Rect(r.x + ballRadius * 0.3f, r.y + ballRadius * 0.3f, r.width - ballRadius * 0.6f, r.height - ballRadius * 0.6f);
                if (inner.width > 0f && inner.height > 0f && inner.Contains(p)) return i;
            }
            return -1;
        }

        /// <summary>Screen position (pixels) of a point on the backdrop image (normalized uv).</summary>
        public Vector2 UvToScreen(Vector2 uv)
        {
            float sw = cam.pixelWidth, sh = cam.pixelHeight;
            float imgAspect = 1f;
            if (House != null && House.backdrop != null)
                imgAspect = House.backdrop.rect.width / Mathf.Max(1f, House.backdrop.rect.height);
            float screenAspect = sw / Mathf.Max(1f, sh);
            float w, h;
            bool widthBound = fit == BackdropFit.Cover ? imgAspect < screenAspect : imgAspect > screenAspect;
            if (widthBound) { w = sw; h = sw / imgAspect; }
            else { h = sh; w = sh * imgAspect; }
            float x0 = (sw - w) * 0.5f, y0 = (sh - h) * 0.5f;
            return new Vector2(x0 + uv.x * w, y0 + uv.y * h);
        }

        /// <summary>Normalized uv of the backdrop -> point on the facade plane.</summary>
        public Vector3 UvToFacade(Vector2 uv)
        {
            Vector2 s = UvToScreen(uv);
            Ray ray = cam.ScreenPointToRay(new Vector3(s.x, s.y, 0f));
            var plane = new Plane(Vector3.back, new Vector3(0f, 0f, FacadeZ));
            return plane.Raycast(ray, out float enter) ? ray.GetPoint(enter) : new Vector3(0f, 0f, FacadeZ);
        }

        /// <summary>
        /// Logs which windows a straight kick can reach at all with the current kick tuning.
        /// Run it (context menu) after authoring a house: an unreachable window makes the medal impossible.
        /// </summary>
        public void ReachabilityReport(KickTuning kick, float startHeight)
        {
            if (House == null) { Debug.LogWarning("Map a house first."); return; }
            float minH = float.MaxValue, maxH = float.MinValue;
            for (int i = 0; i <= 40; i++)
            {
                var res = KickSolver.Solve(new KickGesture(i / 40f, 0f, 0f), 0f, 1f, kick, 0f, 0f);
                float h = KickSolver.HeightAtDistance(res.Velocity, startHeight, House.facadeDistance, kick.Gravity);
                if (float.IsNaN(h)) continue;
                minH = Mathf.Min(minH, h);
                maxH = Mathf.Max(maxH, h);
            }
            for (int i = 0; i < WindowRects.Length; i++)
            {
                Rect r = WindowRects[i];
                bool ok = r.yMax >= minH && r.yMin <= maxH;
                string msg = $"{House.name} window {i}: y {r.yMin:0.0}-{r.yMax:0.0} m, kick reaches {minH:0.0}-{maxH:0.0} m (drag-free)";
                if (ok) Debug.Log(msg + " OK"); else Debug.LogWarning(msg + " UNREACHABLE");
            }
            if (maxH > RoofHeight) Debug.Log($"{House.name}: full-strength kicks clear the roof ({RoofHeight:0.0} m) - good, overhitting is possible.");
        }

        void OnDrawGizmos()
        {
            if (WindowRects == null) return;
            Gizmos.color = Color.cyan;
            foreach (var r in WindowRects)
                Gizmos.DrawWireCube(new Vector3(r.center.x, r.center.y, FacadeZ), new Vector3(r.width, r.height, 0.05f));
            Gizmos.color = Color.red;
            Gizmos.DrawLine(new Vector3(-20f, RoofHeight, FacadeZ), new Vector3(20f, RoofHeight, FacadeZ));
        }
    }
}
