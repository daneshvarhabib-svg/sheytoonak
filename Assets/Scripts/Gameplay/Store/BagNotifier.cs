using System;
using UnityEngine;
using Sheytoonak.Core.Bags;

namespace Sheytoonak.Gameplay.Store
{
    /// <summary>The phone's local notifications. Set by the Android-only assembly (Scripts/Notifications); null elsewhere.</summary>
    public interface INoticeBackend
    {
        void RequestPermission();
        void Schedule(string title, string body, DateTime fireUtc);
        void CancelAll();
    }

    /// <summary>
    /// Research proposal 25 (user 2026-11-11: «پ۲۵ اجرا بشه باقیش نه»): when the game is closed while a bag unlocks, the
    /// phone says «كيسك جاهز!» when it is ready. No server: the time is known in advance, so the phone itself is told
    /// when the game goes to the background, and everything is cancelled when the game comes back.
    /// ⚠️ The Android code lives in its own assembly (Sheytoonak.Notifications, Android only) and registers itself here,
    /// so the editor, the CI screenshots and the local compile check never need the notifications package.
    /// </summary>
    public static class BagNotifier
    {
        public static INoticeBackend Backend;

        const string PrefOn = "sheytoonak.notify", PrefLastFire = "sheytoonak.notify.last", PrefPending = "sheytoonak.notify.pending";
        public const string Title = "مدينة الفاجعة";
        public const string Body = "كيسك جاهز! تعال افتحه 🎒";

        /// <summary>The switch in the lobby; on by default (Android 13+ still asks the player once).</summary>
        public static bool On
        {
            get => PlayerPrefs.GetInt(PrefOn, 1) == 1;
            set { PlayerPrefs.SetInt(PrefOn, value ? 1 : 0); PlayerPrefs.Save(); if (!value) Backend?.CancelAll(); else Backend?.RequestPermission(); }
        }

        /// <summary>Called when a bag starts unlocking: the moment the player understands why we ask.</summary>
        public static void BagStarted() { if (On) Backend?.RequestPermission(); }

        /// <summary>The game goes to the background (or quits).</summary>
        public static void Leaving(BagRules bags)
        {
            if (Backend == null || bags == null) return;
            Backend.CancelAll();
            if (!On) return;
            long ready = 0;
            foreach (var b in bags.Rack) if (b != null && b.Unlocking && b.ReadyTicks > ready && b.ReadyTicks != long.MaxValue) ready = b.ReadyTicks;
            var when = BagNotice.When(ready, DateTime.UtcNow, LastFire());
            if (when == null) { PlayerPrefs.DeleteKey(PrefPending); PlayerPrefs.Save(); return; }
            Backend.Schedule(Title, Body, when.Value);
            PlayerPrefs.SetString(PrefPending, when.Value.Ticks.ToString()); PlayerPrefs.Save();
        }

        /// <summary>The game is back: a notification whose time has passed has fired (counts for «one a day»); cancel the rest.</summary>
        public static void Back()
        {
            if (long.TryParse(PlayerPrefs.GetString(PrefPending, ""), out long t))
            {
                if (t <= DateTime.UtcNow.Ticks) PlayerPrefs.SetString(PrefLastFire, t.ToString());
                PlayerPrefs.DeleteKey(PrefPending); PlayerPrefs.Save();
            }
            Backend?.CancelAll();
        }

        static DateTime? LastFire()
            => long.TryParse(PlayerPrefs.GetString(PrefLastFire, ""), out long t) ? new DateTime(t, DateTimeKind.Utc) : (DateTime?)null;
    }
}
