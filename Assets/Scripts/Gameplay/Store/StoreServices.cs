using System;

namespace Sheytoonak.Gameplay.Store
{
    /// <summary>
    /// Real-money purchases. ⚠️ Nothing is connected: no store account exists yet (the game launches in the Arab world: Google Play
    /// Billing, later the App Store; the owner's developer account is needed, then the SDK is wired in here). Until then <see cref="Available"/>
    /// is false and the shop shows «قريباً» on every money button: nothing can be charged and nothing is granted for free.
    /// </summary>
    public static class StorePayments
    {
        public static bool Available => Provider != null;
        public static Action<string, Action<bool>> Provider; // sku, done(paid)
        /// <summary>The store's own localized price for a sku (e.g. «٣٫٧٩ ر.س»), or null before a store is connected.</summary>
        public static Func<string, string> LocalPrice;
        public static string PriceText(string sku, int usdCents) => LocalPrice?.Invoke(sku) ?? Sheytoonak.Core.Bags.ShopCatalog.Usd(usdCents);
        public static void Buy(string sku, Action<bool> done)
        {
            if (Provider == null) { done?.Invoke(false); return; }
            Provider(sku, done);
        }
    }

    /// <summary>
    /// «Watch and win» ads. ⚠️ No ad network is connected (one for the Arab market, e.g. AdMob or Unity Ads, needs the owner's
    /// account and keys). Until then the pile that the reference sells for an ad is simply free.
    /// </summary>
    public static class RewardedAds
    {
        public static bool Available => Provider != null;
        public static Action<Action<bool>> Provider; // done(watched to the end)
        public static void Show(Action<bool> done)
        {
            if (Provider == null) { done?.Invoke(true); return; }
            Provider(done);
        }
    }
}
