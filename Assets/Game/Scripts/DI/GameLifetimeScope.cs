using Game.Scripts.Audio;
using Game.Scripts.Characters.Player;
using Game.Scripts.Environment.Effect;
using Game.Scripts.Levels;
using Game.Scripts.ObjectPool;
using Game.Scripts.Spawners;
using Game.Scripts.StateServices;
using Game.Scripts.UI;
using Game.Scripts.Wave;
using TransitionsPlus;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using VContainer;
using VContainer.Unity;

namespace Game.Scripts.DI
{
    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private EffectsPool _effectsPool;
        [SerializeField] private EffectSpawner _effectSpawner;
        [SerializeField] private WaveSystem _waveSystem;
        [SerializeField] private PlayerSpawner _playerSpawner;
        [SerializeField] private BulletSpawner _playerBulletSpawner;
        [SerializeField] private BulletSpawner _enemyBulletSpawner;
        [SerializeField] private DynamicJoystick _playerJoystick;
        [SerializeField] private UpgradeChoicePanel _upgradeChoicePanel;
        [SerializeField] private AudioMixer _audioMixer;
        [SerializeField] private Volume _globalVolume;
        [SerializeField] private CameraVignetteEffect _cameraVignetteEffect;
        [SerializeField] private TransitionProfile _transitionProfile;
        [SerializeField] private TapToStartZone _tapToStartZone;
        [SerializeField] private EnemySpawner _enemySpawner;
        [SerializeField] private LevelPanels _levelPanels;

        protected override void Configure(IContainerBuilder builder)
        {
            ConfigureModalServices(builder); 
            ConfigureEffects(builder);
            ConfigureLevelFlow(builder);
            ConfigurePlayerFlow(builder);
            ConfigureAudio(builder);
            ConfigureRendering(builder);
            ConfigureUI(builder);
            ConfigureGameState(builder);
        }
        
        private void ConfigureModalServices(IContainerBuilder builder)
        {
            builder.Register<PauseService>(Lifetime.Singleton).As<IPauseService>();
            builder.Register<ModalCoordinator>(Lifetime.Singleton).As<IModalCoordinator>();
            
            builder.Register<GameplayControlService>(Lifetime.Singleton)
                .As<IGameplayControlService>();
        }

        private void ConfigureEffects(IContainerBuilder builder)
        {
            builder.Register<IEffectService, EffectService>(Lifetime.Singleton);
            builder.RegisterComponent(_effectsPool);
            builder.RegisterComponent(_effectSpawner);

            builder.RegisterComponent(_transitionProfile);
        }

        private void ConfigureLevelFlow(IContainerBuilder builder)
        {
            builder.Register<IObjectFactory, ObjectFactory>(Lifetime.Scoped);
            
            builder.RegisterComponent(_waveSystem);
        }

        private void ConfigurePlayerFlow(IContainerBuilder builder)
        {
            builder.RegisterComponent(_playerSpawner);
            builder.Register<IPlayerProvider, PlayerProvider>(Lifetime.Scoped);
            builder.RegisterComponent(_playerBulletSpawner);
            builder.RegisterComponent(_playerJoystick);
        }
        
        private void ConfigureAudio(IContainerBuilder builder)
        {
            builder.RegisterComponent(_audioMixer);
            builder.Register<IAudioSettingsService, AudioSettingsService>(Lifetime.Singleton);
        }
        
        private void ConfigureRendering(IContainerBuilder builder)
        {
            builder.RegisterComponent(_globalVolume);
            builder.RegisterComponent(_cameraVignetteEffect);
            builder.Register<ICameraVignetteService, CameraVignetteService>(Lifetime.Singleton);
        }
        
        private void ConfigureUI(IContainerBuilder builder)
        {
            builder.RegisterComponent(_upgradeChoicePanel);
            builder.RegisterComponent(_levelPanels);
        }
        
        private void ConfigureGameState(IContainerBuilder builder)
        {
            builder.Register<GameStateService>(Lifetime.Singleton).As<IGameStateService>();
            builder.RegisterComponent(_tapToStartZone);
            builder.RegisterComponent(_enemySpawner);
        }
    }
}