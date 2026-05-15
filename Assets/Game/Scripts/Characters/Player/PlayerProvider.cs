using Game.Scripts.Spawners;

namespace Game.Scripts.Characters.Player
{
    public class PlayerProvider : IPlayerProvider
    {
        private readonly PlayerSpawner _playerSpawner;

        public PlayerProvider(PlayerSpawner playerSpawner)
        {
            _playerSpawner = playerSpawner;
        }

        public Player CurrentPlayer => _playerSpawner.CurrentPlayer;
        public Player Player { get; private set; }
    }
}