using System;
using Cysharp.Threading.Tasks;
using Game.Scripts.AddressablesContent;
using Game.Scripts.Save;
using Game.Scripts.Shop;
using UnityEngine;
using VContainer.Unity;

namespace Game.Scripts.Levels
{
    public class Bootstrapper : IStartable
    {
        private readonly ILevelService _levelService;
        private readonly ISaveLoadGate _saveLoadGate;
        private readonly IAddressablesContentService _content;
        private readonly ShopCatalogProvider _shopCatalogProvider;
        private readonly ShopService _shopService;

        public Bootstrapper(
            ILevelService levelService,
            ISaveLoadGate saveLoadGate,
            IAddressablesContentService content,
            ShopCatalogProvider shopCatalogProvider,
            ShopService shopService)
        {
            _levelService = levelService ?? throw new ArgumentNullException(nameof(levelService));
            _saveLoadGate = saveLoadGate ?? throw new ArgumentNullException(nameof(saveLoadGate));
            _content = content ?? throw new ArgumentNullException(nameof(content));
            _shopCatalogProvider = shopCatalogProvider ?? throw new ArgumentNullException(nameof(shopCatalogProvider));
            _shopService = shopService ?? throw new ArgumentNullException(nameof(shopService));
        }

        public void Start()
        {
            StartAsync().Forget();
        }

        private async UniTaskVoid StartAsync()
        {
            try
            {
                Debug.Log("[Bootstrapper] Addressables init...");
                await _content.InitializeAsync();
            }
            catch (Exception e)
            {
                Debug.LogError($"[Bootstrapper] Addressables failed:\n{e.Message}\n{e.InnerException?.Message}");
                return;
            }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            await TrySmokeLoadAsync();
#endif

            Debug.Log("[Bootstrapper] Preload remote content labels...");
            await PreloadGameplayContentAsync();
            await LoadShopCatalogAsync();

            Debug.Log("[Bootstrapper] Ожидание облачных сохранений...");
            await _saveLoadGate.WaitUntilReadyAsync();

            _shopService.Initialize();

            Debug.Log("[Bootstrapper] Сохранения готовы — загрузка уровня");
            await _levelService.LoadCurrentLevelAsync();
        }

        private async UniTask PreloadGameplayContentAsync()
        {
            string[] labels =
            {
                AddressablesLabels.RemoteConfigs,
                AddressablesLabels.RemoteCharacters,
                AddressablesLabels.RemoteCombat,
                AddressablesLabels.RemoteWorld,
                AddressablesLabels.RemoteFx,
                AddressablesLabels.RemoteIcons,
                AddressablesLabels.RemoteAudio
            };

            foreach (string label in labels)
            {
                try
                {
                    await _content.DownloadDependenciesAsync(label);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Bootstrapper] Skip label '{label}': {e.Message}");
                }
            }
        }

        private async UniTask LoadShopCatalogAsync()
        {
            try
            {
                ShopCatalog catalog = await _content.LoadAssetAsync<ShopCatalog>(ContentAddresses.ShopCatalog);
                _shopCatalogProvider.SetCatalog(catalog);
                int count = catalog != null && catalog.Items != null ? catalog.Items.Count : 0;
                Debug.Log($"[Bootstrapper] Shop catalog loaded ({count} items).");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Bootstrapper] Shop catalog failed: {e.Message}");
            }
        }

        private async UniTask TrySmokeLoadAsync()
        {
            try
            {
                TextAsset smoke = await _content.LoadAssetAsync<TextAsset>(ContentAddresses.SmokeTest);
                Debug.Log($"[Bootstrapper] Smoke OK: {smoke.text}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Bootstrapper] Smoke asset not ready yet: {e.Message}");
            }
        }
    }
}
