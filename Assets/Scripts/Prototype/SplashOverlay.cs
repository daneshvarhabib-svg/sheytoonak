using UnityEngine;

namespace Sheytoonak
{
    /// <summary>
    /// The opening screen, drawn above everything (GUI.depth) so the lobby is already there underneath when it fades.
    /// User (2026-11-09): «گاهی تصویر جدیدی که دادم هست و بعدش در حد یه ثانیه تصویر قبلی بدون آدمک‌ها پلی می‌شه … پیشنهادم اینه
    /// که تصویر اول بیاد بعد لوگوی بازی از بالا بیاد تا جایی که الان هست؛ یه نوار لودینگ هم پایین صفحه باشه که آیکون لودش توپ
    /// فوتبالی باشه … با سایز خیلی خیلی خیلی کوچیک؛ اون لود که پر شد وارد بازی شیم».
    /// ⚠️ until then the splash returned early from the bootstrap's OnGUI and faded out over nothing, so the bare 3D street
    /// (no heroes) showed through for the last half second -- the "old picture" the user saw.
    /// </summary>
    public class SplashOverlay : MonoBehaviour
    {
        public const float LoadSeconds = 4.0f;
        Texture2D splash, logo, ball;
        float start;
        public static bool Showing { get; private set; } = true;

        void Awake()
        {
            splash = Resources.Load<Texture2D>("Art/UI/splash");
            logo = Resources.Load<Texture2D>("Art/UI/logo");
            ball = Resources.Load<Texture2D>("Art/Props/ball_street_icon");
            start = Time.realtimeSinceStartup;
            Showing = splash != null;
        }

        static float Bounce(float t) // ease-out bounce: the sign drops and settles with two small hops
        {
            const float n = 7.5625f, d = 2.75f;
            if (t < 1f / d) return n * t * t;
            if (t < 2f / d) { t -= 1.5f / d; return n * t * t + 0.75f; }
            if (t < 2.5f / d) { t -= 2.25f / d; return n * t * t + 0.9375f; }
            t -= 2.625f / d; return n * t * t + 0.984375f;
        }

        void OnGUI()
        {
            if (!Showing) return;
            GUI.depth = -1000;
            float now = Time.realtimeSinceStartup - start;
            const float fade = 0.35f;
            if (now >= LoadSeconds + fade) { Showing = false; enabled = false; return; }
            float a = now < LoadSeconds ? 1f : 1f - (now - LoadSeconds) / fade;
            float sw = Screen.width, sh = Screen.height;
            var old = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, a);
            float k = Mathf.Max(sw / splash.width, sh / splash.height);
            GUI.DrawTexture(new Rect((sw - splash.width * k) * 0.5f, (sh - splash.height * k) * 0.5f, splash.width * k, splash.height * k), splash, ScaleMode.StretchToFill);
            if (logo != null)
            {
                // first the picture alone for 0.4 s, then the sign comes down slowly from above the screen and settles
                // (user 2026-11-09: «از بالای صفحه بیاد آروم آروم روی تصویر»; the bouncing drop of the day before was too quick)
                float lw = sw * 0.34f, lh = lw * logo.height / logo.width, restY = -lh * 0.06f;
                float t = Mathf.Clamp01((now - 0.4f) / 2.4f);
                float e = 1f - Mathf.Pow(1f - t, 3f); // ease-out: fast start, long soft landing
                float y = Mathf.Lerp(-lh * 1.1f, restY, e) + Mathf.Sin(now * 1.6f) * lh * 0.006f * t; // a slight sway on its chains
                GUI.DrawTexture(new Rect((sw - lw) * 0.5f, y, lw, lh), logo, ScaleMode.StretchToFill);
            }
            // loading bar along the bottom; a tiny football rolls along its filled edge
            float u = Mathf.Max(1f, sh / 900f);
            float bw = sw * 0.46f, bh = 7f * u, bx = (sw - bw) * 0.5f, by = sh - 46f * u;
            float fill = Mathf.Clamp01(now / LoadSeconds);
            fill = 1f - Mathf.Pow(1f - fill, 1.6f); // a touch faster at the start
            GUI.color = new Color(0f, 0f, 0f, 0.45f * a); GUI.DrawTexture(new Rect(bx - 2f * u, by - 2f * u, bw + 4f * u, bh + 4f * u), Texture2D.whiteTexture);
            GUI.color = new Color(1f, 0.82f, 0.35f, a); GUI.DrawTexture(new Rect(bx, by, bw * fill, bh), Texture2D.whiteTexture);
            if (ball != null)
            {
                float bs = bh * 3.6f, cx = bx + bw * fill, cy = by + bh * 0.5f;
                var m = GUI.matrix;
                GUIUtility.RotateAroundPivot(now * 540f, new Vector2(cx, cy));
                GUI.color = new Color(1f, 1f, 1f, a);
                GUI.DrawTexture(new Rect(cx - bs * 0.5f, cy - bs * 0.5f, bs, bs), ball, ScaleMode.ScaleToFit);
                GUI.matrix = m;
            }
            GUI.color = old;
        }
    }
}
