using Game.Scripts.Characters.Enemy;
using UnityEngine;
using Zenject;

namespace Game.Scripts
{
    public class GameInstaller : MonoInstaller
    {
        [SerializeField] private PlayerProvider _playerProvider; 
        
        public override void InstallBindings()
        {
            InstallPlayerProvider();
        }
        
        private void InstallPlayerProvider()
        {
            Container.Bind<IPlayerProvider>()
                .To<PlayerProvider>()
                .FromInstance(_playerProvider)
                .AsSingle();
            
            Container.Bind<PlayerProvider>()
                .FromInstance(_playerProvider)
                .AsSingle();
            
            Container.Bind<Vector3>()
                .FromMethod(() => _playerProvider.GetPlayerPosition())
                .WhenInjectedInto<Enemy>();
        }
    }
}