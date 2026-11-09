using System;
using Sheytoonak.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Sheytoonak
{
    /// <summary>
    /// Touch kick (the game's kick since 2026-10-23) plus editor controls.
    /// Touch or mouse, since 2026-11-08 a swipe: drag from the ball toward the windows; the drag's length is
    /// the power and its angle the aim (user: «انگشتش رو بکشه … هرچی بیشتر بکشه شدتش بیشتر می‌شه، جهتش هم مهمه»).
    /// ⚠️ 2026-10-24 to 2026-11-08 a tap was the kick (x = aim, y = power); gone. The finger's x position also
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
        [Tooltip("Swipe shorter than this fraction of the screen height is not a kick.")]
        public float minSwipe = 0.025f;
        [Tooltip("Swipe this fraction of the screen height (or longer) = full power.")]
        public float fullSwipe = 0.32f;
        [Tooltip("Swipe angle from straight up that means full aim (MaxAimYawDeg).")]
        public float fullAimDeg = 55f;
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
        public void IgnoreFor(float seconds) { ignoreUntil = Time.unscaledTime + seconds; pressing = false; Dragging = false; }

        /// <summary>
        /// Height of the strip along the bottom of the screen that is UI, in pixels: a press that starts there is never a
        /// kick. User (2026-11-05): «وقتی می‌خوای یک قهرمان دیگه رو انتخاب کنی اتوماتیک همونی انتخاب می‌شه که پشت توپه و یه
        /// ضربه می‌زنه». A quick tap is press and release inside one Update, which runs before OnGUI: the kick fired before
        /// the face button could say IgnoreFor. The HUD sets this every frame.
        /// </summary>
        public float DeadZoneHeight;
        /// <summary>Widths (px) of the button blocks at the left and right ends of that strip; only they are dead.</summary>
        public float DeadZoneLeft, DeadZoneRight;

        void Update()
        {
            var ptr = Pointer.current;
            if (ptr != null)
            {
                Vector2 pos = ptr.position.ReadValue();
                // ⚠️ until 2026-11-08 the whole bottom strip (≈200 px at 900p) was dead, and the ball sits inside it: a swipe
                // that started near the ball, as asked, never registered (user: «ضربه زدن سخت شده و تقریباً نمی‌شه زد»).
                // Now only the button blocks at the strip's left and right ends are dead.
                bool onButtons = pos.y <= DeadZoneHeight && (pos.x <= DeadZoneLeft || pos.x >= Screen.width - DeadZoneRight);
                if (ptr.press.wasPressedThisFrame && Time.unscaledTime >= ignoreUntil && !onButtons) { pressing = true; pressPos = pos; pressTime = Time.unscaledTime; }
                // the swipe so far, for the aim arc (AimArc) -- same rule as the kick fired on release
                KickGesture live = default;
                Dragging = pressing && GestureFor(pos - pressPos, out live);
                if (Dragging) Live = live;
                if (pressing && moveWithFinger) Lateral = Mathf.Clamp((pressPos.x / Mathf.Max(1, Screen.width)) * 2f - 1f, -1f, 1f);
                if (pressing && ptr.press.wasReleasedThisFrame)
                {
                    pressing = false; Dragging = false;
                    // User (2026-11-08): «فرد موقع زدن توپ انگشتش رو بکشه روی صفحه از مبدأ توپ به سمت پنجره‌ها؛ هرچی بیشتر بکشه
                    // شدتش بیشتر می‌شه؛ جهتش هم مهمه». The kick is the swipe: its length is the power (a swipe of 55 % of the
                    // screen height = full), its angle from straight up is the aim (45 degrees = full yaw). A tap or a
                    // downward flick is not a kick. The rebound is hit the same way.
                    // User (2026-11-08, second pass): «الگوش باید ساده باشه؛ دستش رو می‌ذاره نزدیک توپ و به یه سمتی حرکت می‌ده،
                    // انگشتش رو که برداشت باید ضربه فایر شه». No time limit, any direction except back toward the player.
                    if (GestureFor(pos - pressPos, out var g)) Kicked?.Invoke(g);
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

        /// <summary>True while a finger is down and its swipe would already be a kick; <see cref="Live"/> is that kick.</summary>
        public bool Dragging { get; private set; }
        public KickGesture Live { get; private set; }

        bool GestureFor(Vector2 d, out KickGesture g)
        {
            g = default;
            float h = Mathf.Max(1f, Screen.height);
            if (!(d.y > -0.3f * Mathf.Abs(d.x) && d.magnitude > h * minSwipe)) return false;
            float power = Mathf.Clamp01((d.magnitude - h * minSwipe) / (h * (fullSwipe - minSwipe)));
            float aim = Mathf.Clamp(Mathf.Atan2(d.x, d.y) * Mathf.Rad2Deg / fullAimDeg, -1f, 1f);
            g = new KickGesture(power, aim, 0f);
            return true;
        }
    }
}
