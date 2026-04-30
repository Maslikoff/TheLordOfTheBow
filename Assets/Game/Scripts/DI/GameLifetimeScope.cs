using Game.Scripts.Characters.Player;
using Game.Scripts.Environment.Effect;
using Game.Scripts.Levels;
using Game.Scripts.ObjectPool;
using Game.Scripts.Spawners;
using Game.Scripts.Wave;
using UnityEngine;
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
        [SerializeField] private BulletSpawner _bulletSpawner;
        [SerializeField] private DynamicJoystick _playerJoystick;

        protected override void Configure(IContainerBuilder builder)
        {
            ConfigureEffects(builder);
            ConfigureLevelFlow(builder);
            ConfigurePlayerFlow(builder);
        }

        private void ConfigureEffects(IContainerBuilder builder)
        {
            builder.Register<IEffectService, EffectService>(Lifetime.Singleton);
            builder.RegisterComponent(_effectsPool);
            builder.RegisterComponent(_effectSpawner);
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
            builder.RegisterComponent(_bulletSpawner);
            builder.RegisterComponent(_playerJoystick);
        }
    }
}