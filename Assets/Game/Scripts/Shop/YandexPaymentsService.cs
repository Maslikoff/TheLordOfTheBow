using System;
using JTLStudio.SDK;
using UnityEngine;

namespace Game.Scripts.Shop
{
    /// <summary>
    /// IAP via the JTL SDK Payments module. Non-consumable products (e.g. skin_ninja, skin_valkyrie)
    /// are configured in the active platform's store. On platforms without purchases (YouTube Playables)
    /// the module reports IsSupported == false and purchases fail gracefully.
    /// </summary>
    public sealed class YandexPaymentsService : IPaymentsService
    {
        public event Action<string> PurchaseSucceeded;
        public event Action<string> PurchaseFailed;

        private bool _initialized;

        private static bool PaymentsAvailable => JTLSDK.IsCreated && JTLSDK.Payments.IsSupported;

        public void Initialize()
        {
            if (_initialized)
                return;

            _initialized = true;

            if (PaymentsAvailable)
            {
                // Restore once after the module is ready (JTL SDK records grants under "jtlsdk.granted").
                JTLSDK.WhenReady(() => JTLSDK.Payments.RestorePurchases(_ => { }));
            }
            else
            {
                Debug.Log("[Payments] Purchases are not supported on the active platform.");
            }
        }

        public void Buy(string productId)
        {
            if (string.IsNullOrEmpty(productId))
            {
                Debug.LogWarning("[Payments] Empty product id");
                return;
            }

            Debug.Log($"[Payments] Buy requested: {productId}");

            if (PaymentsAvailable == false)
            {
                Debug.LogWarning($"[Payments] Cannot buy {productId}: payments not supported on this platform");
                PurchaseFailed?.Invoke(productId);
                return;
            }

            JTLSDK.Payments.Purchase(
                productId,
                () =>
                {
                    Debug.Log($"[Payments] Success: {productId}");
                    PurchaseSucceeded?.Invoke(productId);
                },
                () =>
                {
                    Debug.LogWarning($"[Payments] Failed: {productId}");
                    PurchaseFailed?.Invoke(productId);
                });
        }

        public void ConsumePendingPurchases()
        {
            if (PaymentsAvailable)
                JTLSDK.Payments.RestorePurchases(_ => { });
            else
                Debug.Log("[Payments] ConsumePendingPurchases skipped (payments not supported)");
        }
    }
}
