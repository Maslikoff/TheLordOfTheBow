using Assets.Game.Scripts.Characters.Player;
using Game.Scripts.Environment.Effect;
using Game.Scripts.ObjectPool;
using Game.Scripts.Spawners;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Scripts.DI
{
    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private PlayerTransformHolder _playerTransformHolder;
        [SerializeField] private EffectsPool _effectsPool;
        [SerializeField] private EffectSpawner _effectSpawner;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_playerTransformHolder)
                .As<ITransformHolder>();
            
            builder.Register<IEffectService, EffectService>(Lifetime.Singleton);
            
            builder.RegisterComponent(_effectsPool);
            builder.RegisterComponent(_effectSpawner);
        }
    }
}