using System;
using UnityEngine;
using YG;

namespace Game.Scripts.Shop
{
    /// <summary>
    /// Yandex Games IAP via PluginYG2 Payments module.
    /// PluginYG2 Payments integration. Create non-consumable products with these ids in Yandex Games:
    /// skin_ninja, skin_valkyrie
    /// </summary>
    public sealed class YandexPaymentsService : IPaymentsService
    {
        public event Action<string> PurchaseSucceeded;
        public event Action<string> PurchaseFailed;

        private bool _initialized;

        public void Initialize()
        {
            if (_initialized)
                return;

            _initialized = true;

#if Payments_yg
            YG2.onPurchaseSuccess += OnPurchaseSuccess;
            YG2.onPurchaseFailed += OnPurchaseFailed;
            ConsumePendingPurchases();
#else
#if UNITY_EDITOR
            Debug.LogWarning(
                "[Payments] Payments_yg is not defined. Import PluginYG2 Payments module. " +
                "Editor stub will simulate successful purchases.");
#else
            Debug.LogError(
                "[Payments] Payments_yg is not defined. IAP purchases are disabled in this build.");
#endif
#endif
        }

        public void Buy(string productId)
        {
            if (string.IsNullOrEmpty(productId))
            {
                Debug.LogWarning("[Payments] Empty product id");
                return;
            }

            Debug.Log($"[Payments] Buy requested: {productId}");

#if Payments_yg
            YG2.BuyPayments(productId);
#else
#if UNITY_EDITOR
            // Editor / missing-module stub so shop flow can be tested.
            Debug.Log($"[Payments] Stub success for {productId}");
            PurchaseSucceeded?.Invoke(productId);
#else
            // Never grant paid content when the platform payments module is missing.
            Debug.LogError($"[Payments] Cannot buy {productId}: Payments_yg is not enabled");
            PurchaseFailed?.Invoke(productId);
#endif
#endif
        }

        public void ConsumePendingPurchases()
        {
#if Payments_yg
            YG2.ConsumePurchases();
#else
            Debug.Log("[Payments] ConsumePendingPurchases skipped (Payments module not installed)");
#endif
        }

        private void OnPurchaseSuccess(string id)
        {
            Debug.Log($"[Payments] Success: {id}");
            PurchaseSucceeded?.Invoke(id);
        }

        private void OnPurchaseFailed(string id)
        {
            Debug.LogWarning($"[Payments] Failed: {id}");
            PurchaseFailed?.Invoke(id);
        }
    }
}
