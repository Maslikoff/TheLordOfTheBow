using Game.Scripts.PoolSystem;
using Game.Scripts.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Scripts
{
    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private PoolService _poolServicePrefab;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<PlayerReferenceService>(Lifetime.Singleton);

            if (_poolServicePrefab != null)
            {
                builder.RegisterComponentInNewPrefab(_poolServicePrefab, Lifetime.Singleton)
                    .DontDestroyOnLoad();
            }
        }

        private void Start()
        {
            var playerService = Container.Resolve<PlayerReferenceService>();
            playerService.SetPlayerTransform(_playerTransform);
        }
    }
}