using System.Collections.Generic;
using UnityEngine;

namespace Sheytoonak
{
    /// <summary>
    /// Research proposal 17, approved 2026-11-09 («هر سه تا پیشنهاد همین الان اعمال بشه»): while the finger is down and
    /// the swipe would already be a kick, a short dotted arc leaves the ball along the launch that kick would have (no
    /// random contact error). Only the first ~35 % of the way to the facade is shown, so where it lands is still the
    /// player's skill. It vanishes when the finger lifts.
    /// </summary>
    public class AimArc : MonoBehaviour
    {
        public DuelManager duel;
        public TouchKickInput touch;
        public int dots = 9;
        public float shownFraction = 0.35f;

        readonly List<SpriteRenderer> pool = new List<SpriteRenderer>();
        static Sprite dot;

        void Awake()
        {
            if (dot == null)
            {
                const int n = 32; var t = new Texture2D(n, n, TextureFormat.RGBA32, false);
                var px = new Color32[n * n];
                for (int y = 0; y < n; y++) for (int x = 0; x < n; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(15.5f, 15.5f)) / 15.5f;
                    px[y * n + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01((1f - d) * 3f) * 255));
                }
                t.SetPixels32(px); t.Apply();
                dot = Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), 100f);
            }
            for (int i = 0; i < dots; i++)
            {
                var r = new GameObject("Aim dot " + i).AddComponent<SpriteRenderer>();
                r.sprite = dot; r.sortingOrder = 50; r.transform.SetParent(transform, false);
                r.enabled = false; pool.Add(r);
            }
        }

        void LateUpdate()
        {
            bool show = duel != null && touch != null && touch.Dragging && duel.CanAimNow;
            if (!show) { foreach (var r in pool) r.enabled = false; return; }
            Vector3 v = duel.PreviewLaunch(touch.Live, out Vector3 from);
            float wall = duel.mapper != null ? duel.mapper.FacadeZ : from.z + 15f;
            float tWall = v.z > 0.1f ? (wall - from.z) / v.z : 1f;
            float g = -Physics.gravity.y;
            var cam = Camera.main;
            for (int i = 0; i < pool.Count; i++)
            {
                float k = (i + 1f) / pool.Count;
                float t = tWall * shownFraction * k;
                Vector3 p = from + v * t + 0.5f * g * t * t * Vector3.down;
                var r = pool[i];
                r.enabled = true;
                r.transform.position = p;
                if (cam != null) r.transform.rotation = cam.transform.rotation;
                // dots shrink and fade along the arc; a stronger swipe is a brighter, warmer arc
                float size = Mathf.Lerp(0.16f, 0.07f, k);
                r.transform.localScale = Vector3.one * size / 0.32f;
                r.color = new Color(1f, Mathf.Lerp(1f, 0.55f, touch.Live.Strength01), Mathf.Lerp(1f, 0.25f, touch.Live.Strength01), Mathf.Lerp(0.95f, 0.25f, k));
            }
        }
    }
}
