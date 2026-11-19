using UnityEngine;
using Sheytoonak.Core.Missions;

namespace Sheytoonak
{
    /// <summary>
    /// The painting of the city with its quarters as a survey layer (user 2026-11-19, after the first map: «این تصاویری که
    /// گذاشتی روش ریده تو نقشه ... یه لایه جی‌آی‌اسی بهش اضافه بشه و با یه لایه رنگ که سی درصد شفافیت داره ... محله هایی
    /// که باختی قرمز باشه و اونایی که بردی سبز ... محله هایی که فتح شده هم نزدیک هم نباشه لزوما»). 50 irregular quarters
    /// inside the walls (nearest-seed cells), thin borders, nothing else drawn on the painting. Held quarters are washed
    /// green, the ones held once and lost are red, both at 30 %; the quarter being fought for grows green with its houses.
    /// The order of conquest is shuffled (fixed), so held quarters are scattered over the city.
    /// </summary>
    public static class CityMapView
    {
        const int W = 512, H = 256;
        const float Wash = 0.30f;
        static Texture2D map, layer; static bool looked;
        static int[] cell; static bool[] edge; static int[] order;
        static int bakedCurses = -1, bakedPeak = -1;

        public static Texture2D Map { get { if (!looked) { looked = true; map = Resources.Load<Texture2D>("Art/UI/city_map"); } return map; } }
        /// <summary>Width over height of the painting (2:1 when it is missing).</summary>
        public static float Aspect => Map != null ? Map.width / (float)Map.height : 2f;

        /// <summary>The south-west wall runs straight along the river: what lies beyond that line is water, not a quarter
        /// (u, v of the painting, v up; read off the painting, preview 2026-11-19).</summary>
        static bool InRiver(float u, float v) => (1f - v) > 0.44f + 0.84f * u;

        static void Survey()
        {
            if (cell != null) return;
            int n = CityMap.Quarters;
            var rng = new System.Random(4127);
            // seeds inside the walls: an ellipse of the painting (the river and the orchards stay outside), kept apart
            var sx = new float[n]; var sy = new float[n];
            const float cx = 0.55f, cy = 0.5f, rx = 0.40f, ry = 0.42f;
            for (int i = 0; i < n;)
            {
                float x = (float)rng.NextDouble() * 2f - 1f, y = (float)rng.NextDouble() * 2f - 1f;
                if (x * x + y * y > 0.92f) continue;
                if (InRiver(cx + x * rx, cy + y * ry - 0.03f)) continue;
                float px = (cx + x * rx) * W, py = (cy + y * ry) * H; bool far = true;
                for (int k = 0; k < i && far; k++) far = (sx[k] - px) * (sx[k] - px) + (sy[k] - py) * (sy[k] - py) > 30f * 30f;
                if (!far) continue;
                sx[i] = px; sy[i] = py; i++;
            }
            cell = new int[W * H]; edge = new bool[W * H];
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    float ex = (x / (float)W - cx) / (rx * 1.06f), ey = (y / (float)H - cy) / (ry * 1.06f);
                    float wall = ex * ex + ey * ey;
                    if (wall > 1f || InRiver(x / (float)W, y / (float)H)) { cell[y * W + x] = -1; continue; }
                    float d1 = float.MaxValue, d2 = float.MaxValue; int best = 0;
                    for (int k = 0; k < n; k++)
                    {
                        float d = (sx[k] - x) * (sx[k] - x) + (sy[k] - y) * (sy[k] - y);
                        if (d < d1) { d2 = d1; d1 = d; best = k; } else if (d < d2) d2 = d;
                    }
                    cell[y * W + x] = best;
                    edge[y * W + x] = Mathf.Sqrt(d2) - Mathf.Sqrt(d1) < 1.6f || wall > 0.975f || InRiver(x / (float)W, y / (float)H - 0.012f);
                }
            // the order the quarters fall in: shuffled once, the same for everyone
            order = new int[n];
            for (int i = 0; i < n; i++) order[i] = i;
            for (int i = n - 1; i > 0; i--) { int j = rng.Next(i + 1); (order[i], order[j]) = (order[j], order[i]); }
        }

        static void Bake(int curses, int peak)
        {
            Survey();
            if (layer == null) layer = new Texture2D(W, H, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear, hideFlags = HideFlags.HideAndDontSave };
            int n = CityMap.Quarters, held = CityMap.QuartersHeld(curses), peakHeld = Mathf.Max(held, CityMap.QuartersHeld(peak));
            var wash = new Color32[n];
            byte a = (byte)(255 * Wash);
            for (int i = 0; i < n; i++)
            {
                int q = order[i];
                if (i < held) wash[q] = new Color32(40, 200, 70, a);
                else if (i < peakHeld) wash[q] = new Color32(225, 45, 35, a);
                else if (i == held) wash[q] = new Color32(40, 200, 70, (byte)(a * CityMap.HousesInFront(curses) / (float)CityMap.HousesPerQuarter));
                else wash[q] = new Color32(0, 0, 0, 0);
            }
            var px = new Color32[W * H]; var line = new Color32(40, 22, 10, 120);
            for (int i = 0; i < px.Length; i++) px[i] = cell[i] < 0 ? new Color32(0, 0, 0, 0) : edge[i] ? line : wash[cell[i]];
            layer.SetPixels32(px); layer.Apply(false);
            bakedCurses = curses; bakedPeak = peak;
        }

        /// <summary>The painting with the survey layer, filling r (keep r at <see cref="Aspect"/>; a part of it with <paramref name="uv"/>).</summary>
        public static void Draw(Rect r, int curses, int peak, Rect? uv = null)
        {
            if (curses != bakedCurses || peak != bakedPeak || layer == null) Bake(curses, peak);
            var c = uv ?? new Rect(0f, 0f, 1f, 1f);
            if (Map != null) GUI.DrawTextureWithTexCoords(r, Map, c);
            else { var o = GUI.color; GUI.color = new Color(0.78f, 0.66f, 0.45f); GUI.DrawTexture(r, Texture2D.whiteTexture); GUI.color = o; }
            GUI.DrawTextureWithTexCoords(r, layer, c);
        }
    }
}
