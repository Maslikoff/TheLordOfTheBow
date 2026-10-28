#if UNITY_EDITOR
using Game.Scripts.DI;
using Game.Scripts.Shop;
using UnityEditor;
using UnityEngine;
using VContainer;

namespace Game.Scripts.Editor
{
    public static class ShopTestTools
    {
        // Change this value to alter the amount granted by the editor-only command.
        public const int TestCoinGrant = 1000;

        [MenuItem("Tools/Shop/Add 1000 Test Coins")]
        private static void AddTestCoins()
        {
            RootLifetimeScope scope = Object.FindObjectOfType<RootLifetimeScope>(true);
            if (scope == null || scope.Container == null)
            {
                Debug.LogWarning("[ShopTest] Enter Play Mode and wait for the Boot scene to initialize.");
                return;
            }

            ICurrencyService currency = scope.Container.Resolve<ICurrencyService>();
            currency.Add(TestCoinGrant);
            currency.Persist();
            Debug.Log($"[ShopTest] Added {TestCoinGrant} editor-only test coins. Balance: {currency.Coins}");
        }

        [MenuItem("Tools/Shop/Add 1000 Test Coins", true)]
        private static bool ValidateAddTestCoins() => EditorApplication.isPlaying;
    }
}
#endif
