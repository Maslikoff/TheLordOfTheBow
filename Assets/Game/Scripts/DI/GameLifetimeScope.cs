using Assets.Game.Scripts.Characters.Player;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Scripts.DI
{
    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private PlayerTransformHolder _playerTransformHolder;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_playerTransformHolder)
                .As<ITransformHolder>();
        }
    }
}