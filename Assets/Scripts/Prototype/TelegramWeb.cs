using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Sheytoonak
{
    /// <summary>
    /// The web build runs inside a Telegram mini app (user 2026-11-15: «می‌خوام لانچ بازی اونجا باشه و بعداً که یوزر گرفتیم
    /// بیاریمش روی اپ مستقل»). The page side lives in Assets/WebGLTemplates/Telegram; this is the C# side of
    /// Assets/Plugins/WebGL/Telegram.jslib. Outside the web player every call does nothing / returns empty.
    /// </summary>
    public static class TelegramWeb
    {
        /// <summary>The bot whose main mini app is the game (user 2026-11-15: @MadinatAlFajiaBot).</summary>
        public const string BotUsername = "MadinatAlFajiaBot";
        /// <summary>start parameter of an invite: "r_" + the inviter's Telegram id.</summary>
        public const string RefPrefix = "r_";

        /// <summary>
        /// Address of our Stars server (server/stars, no trailing slash). ⚠️ Empty until the owner has put the worker up with the bot's
        /// token as its secret: until then every money button stays «قريباً» and nothing can be charged.
        /// </summary>
        public const string StarsBackend = "";

        /// <summary>«٥٠ نجمة» for a money sku of the shop, or null when it is not one.</summary>
        public static string StarsText(string sku)
        {
            int n = Sheytoonak.Core.Bags.ShopCatalog.StarsOf(sku);
            return n > 0 ? Ar.T(n + " نجمة") : null;
        }

        /// <summary>The invite link of this player (empty outside Telegram).</summary>
        public static string InviteLink => string.IsNullOrEmpty(UserId) ? "" : "https://t.me/" + BotUsername + "?startapp=" + RefPrefix + UserId;

        /// <summary>The inviter's id when the game was opened from someone's invite link ("" otherwise, or from one's own).</summary>
        public static string Inviter
        {
            get
            {
                string p = StartParam;
                if (string.IsNullOrEmpty(p) || !p.StartsWith(RefPrefix)) return "";
                string id = p.Substring(RefPrefix.Length);
                return id == UserId ? "" : id;
            }
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] static extern void SheytoonakTgPay(string backend, string sku);
        [DllImport("__Internal")] static extern int SheytoonakTgPayState();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void ConnectStars()
        {
            if (string.IsNullOrEmpty(StarsBackend) || !Inside) return;
            var go = new GameObject("StarsPay"); UnityEngine.Object.DontDestroyOnLoad(go);
            var runner = go.AddComponent<StarsRunner>();
            Sheytoonak.Gameplay.Store.StorePayments.Provider = (sku, done) => runner.StartCoroutine(runner.Pay(sku, done));
            Sheytoonak.Gameplay.Store.StorePayments.LocalPrice = StarsText;
        }

        sealed class StarsRunner : MonoBehaviour
        {
            bool busy;
            public IEnumerator Pay(string sku, Action<bool> done)
            {
                if (busy) { done?.Invoke(false); yield break; }
                busy = true;
                bool sent = true;
                try { SheytoonakTgPay(StarsBackend, sku); } catch { sent = false; }
                int state = sent ? 0 : 2; float until = Time.realtimeSinceStartup + 600f; // the sheet may stay open while they top up stars
                while (state == 0 && Time.realtimeSinceStartup < until)
                {
                    yield return null;
                    try { state = SheytoonakTgPayState(); } catch { state = 2; }
                }
                busy = false;
                done?.Invoke(state == 1);
            }
        }

        [DllImport("__Internal")] static extern void SheytoonakTgHaptic(int strength);
        [DllImport("__Internal")] static extern int SheytoonakTgInside();
        [DllImport("__Internal")] static extern string SheytoonakTgStartParam();
        [DllImport("__Internal")] static extern string SheytoonakTgUserId();
        [DllImport("__Internal")] static extern void SheytoonakTgShareLink(string url, string text);
        [DllImport("__Internal")] static extern int SheytoonakTgShareStory(string text, string link, string linkName);
        /// <summary>0 light, 1 medium, 2 heavy.</summary>
        public static void Haptic(int strength) { try { SheytoonakTgHaptic(strength); } catch { } }
        /// <summary>True when the page was opened from Telegram (not a plain browser tab).</summary>
        public static bool Inside { get { try { return SheytoonakTgInside() == 1; } catch { return false; } } }
        public static string StartParam { get { try { return SheytoonakTgStartParam() ?? ""; } catch { return ""; } } }
        public static string UserId { get { try { return SheytoonakTgUserId() ?? ""; } catch { return ""; } } }
        /// <summary>Telegram's own «send to a chat» sheet with the link and a line of text.</summary>
        public static void ShareLink(string url, string text) { try { SheytoonakTgShareLink(url, text); } catch { } }
        /// <summary>Posts the game's story picture with a link widget; false when this Telegram cannot (older than 7.8).</summary>
        public static bool ShareStory(string text, string link, string linkName) { try { return SheytoonakTgShareStory(text, link, linkName) == 1; } catch { return false; } }
#else
        public static void Haptic(int strength) { }
        public static bool Inside => false;
        public static string StartParam => "";
        public static string UserId => "";
        public static void ShareLink(string url, string text) { }
        public static bool ShareStory(string text, string link, string linkName) => false;
#endif
    }
}
