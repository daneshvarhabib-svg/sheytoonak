using System.Runtime.InteropServices;

namespace Sheytoonak
{
    /// <summary>
    /// The web build runs inside a Telegram mini app (user 2026-11-15: «می‌خوام لانچ بازی اونجا باشه و بعداً که یوزر گرفتیم
    /// بیاریمش روی اپ مستقل»). The page side lives in Assets/WebGLTemplates/Telegram; this is the C# side of
    /// Assets/Plugins/WebGL/Telegram.jslib. Outside the web player every call does nothing.
    /// </summary>
    public static class TelegramWeb
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] static extern void SheytoonakTgHaptic(int strength);
        [DllImport("__Internal")] static extern int SheytoonakTgInside();
        /// <summary>0 light, 1 medium, 2 heavy.</summary>
        public static void Haptic(int strength) { try { SheytoonakTgHaptic(strength); } catch { } }
        /// <summary>True when the page was opened from Telegram (not a plain browser tab).</summary>
        public static bool Inside { get { try { return SheytoonakTgInside() == 1; } catch { return false; } } }
#else
        public static void Haptic(int strength) { }
        public static bool Inside => false;
#endif
    }
}
