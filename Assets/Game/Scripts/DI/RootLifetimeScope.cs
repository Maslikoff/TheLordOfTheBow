using Game.Localization;
using Game.Scripts.AddressablesContent;
using Game.Scripts.Audio;
using Game.Scripts.Levels;
using Game.Scripts.Save;
using Game.Scripts.Shop;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Scripts.DI
{
    public class RootLifetimeScope : LifetimeScope
    {
        [SerializeField] private GameStartupConfig _startupConfig;
        [SerializeField] private LevelCatalog _levelCatalog;
        [SerializeField] private SaveSystem _saveSystemPrefab;

        protected override void Configure(IContainerBuilder builder)
        {
            ConfigureSceneManagement(builder);
            ConfigureConfigs(builder);
            ConfigureServices(builder);
            ConfigureShop(builder);
            ConfigureSaveSystem(builder);
        }

        private void ConfigureSceneManagement(IContainerBuilder builder)
        {
            builder.Register<ISceneLoader, SceneLoader>(Lifetime.Singleton);
        }

        private void ConfigureConfigs(IContainerBuilder builder)
        {
            builder.RegisterInstance(_startupConfig);
            builder.RegisterInstance(_levelCatalog);
        }

        private void ConfigureServices(IContainerBuilder builder)
        {
            builder.Register<IAddressablesContentService, AddressablesContentService>(Lifetime.Singleton);
            builder.Register<ILevelService, LevelService>(Lifetime.Singleton);
            builder.Register<LevelSessionService>(Lifetime.Singleton);
            builder.Register<IAudioService, AudioService>(Lifetime.Singleton);
            builder.Register<YgLanguageService>(Lifetime.Singleton)
                .As<ILanguageService>()
                .AsSelf();
        }

        private void ConfigureShop(IContainerBuilder builder)
        {
            builder.Register<ShopCatalogProvider>(Lifetime.Singleton);
            builder.Register<ICurrencyService, CurrencyService>(Lifetime.Singleton);
            builder.Register<ICosmeticInventoryService, CosmeticInventoryService>(Lifetime.Singleton);
            builder.Register<IPaymentsService, YandexPaymentsService>(Lifetime.Singleton);
            builder.Register<ShopService>(Lifetime.Singleton);
        }
        
        private void ConfigureSaveSystem(IContainerBuilder builder)
        {
            builder.RegisterComponentInNewPrefab(_saveSystemPrefab, Lifetime.Singleton)
                .As<ISaveSystem>()
                .As<ISaveLoadGate>()
                .AsSelf();
        }
    }
}
