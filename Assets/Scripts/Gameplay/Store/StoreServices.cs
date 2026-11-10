using System;

namespace Sheytoonak.Gameplay.Store
{
    /// <summary>
    /// Real-money purchases. ⚠️ Nothing is connected: no store account exists yet (the owner creates one at Cafe Bazaar,
    /// Myket or Google Play; then its SDK -- e.g. Bazaar's Poolakey -- is wired in here). Until then <see cref="Available"/>
    /// is false and the shop shows «قريباً» on every money button: nothing can be charged and nothing is granted for free.
    /// </summary>
    public static class StorePayments
    {
        public static bool Available => Provider != null;
        public static Action<string, Action<bool>> Provider; // sku, done(paid)
        public static void Buy(string sku, Action<bool> done)
        {
            if (Provider == null) { done?.Invoke(false); return; }
            Provider(sku, done);
        }
    }

    /// <summary>
    /// «Watch and win» ads. ⚠️ No ad network is connected (an Iranian one such as Tapsell or Adivery needs the owner's
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
