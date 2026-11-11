using System;
using Unity.Notifications.Android;
using UnityEngine;
using UnityEngine.Android;
using Sheytoonak.Gameplay.Store;

namespace Sheytoonak.Notifications
{
    /// <summary>
    /// Android side of BagNotifier (research proposal 25), through Unity's free Mobile Notifications package: local
    /// notifications only, no server, no account, nothing leaves the phone. Registers itself before the first scene.
    /// ⚠️ This assembly builds only for Android; the editor and the CI screenshots run without it (Backend stays null).
    /// </summary>
    public sealed class AndroidNotices : INoticeBackend
    {
        const string Channel = "bags";
        bool asked;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register()
        {
            try
            {
                AndroidNotificationCenter.RegisterNotificationChannel(new AndroidNotificationChannel
                {
                    Id = Channel,
                    Name = "أكياس",
                    Importance = Importance.Default,
                    Description = "كيسك جاهز",
                });
                BagNotifier.Backend = new AndroidNotices();
            }
            catch (Exception e) { Debug.LogWarning("[Notices] not available: " + e.Message); }
        }

        /// <summary>Android 13+ asks the player once; refused = the game works the same, just silent.</summary>
        public void RequestPermission()
        {
            if (asked) return;
            asked = true;
            const string perm = "android.permission.POST_NOTIFICATIONS";
            if (!Permission.HasUserAuthorizedPermission(perm)) Permission.RequestUserPermission(perm);
        }

        public void Schedule(string title, string body, DateTime fireUtc)
        {
            AndroidNotificationCenter.SendNotification(new AndroidNotification
            {
                Title = title,
                Text = body,
                FireTime = fireUtc.ToLocalTime(),
            }, Channel);
        }

        public void CancelAll() => AndroidNotificationCenter.CancelAllNotifications();
    }
}
