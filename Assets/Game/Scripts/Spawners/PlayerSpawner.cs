using Game.Scripts.Characters.Player;
using Game.Scripts.Levels;
using Game.Scripts.UI;
using UnityEngine;
using VContainer;
using UniRx;

namespace Game.Scripts.Spawners
{
    public class PlayerSpawner : MonoBehaviour
    {
        [SerializeField] private PlayerSpawnPoint _spawnPoint;
        [SerializeField] private ShootView _shootView;
        [SerializeField] private ExperienceUI _experienceUI;
        [SerializeField] private UpgradeChoicePanel _upgradeChoicePanel;

        private IObjectFactory _factory;
        private GameStartupConfig _startupConfig;

        private Player _currentPlayer;
        
        public Player CurrentPlayer => _currentPlayer;

        [Inject]
        private void Construct(IObjectFactory factory, GameStartupConfig startupConfig)
        {
            _factory = factory;
            _startupConfig = startupConfig;
        }
        
        public Player Spawn()
        {
            Player playerPrefab = _startupConfig.PlayerPrefab;

            DespawnCurrentPlayer();

            _currentPlayer = _factory.Create(playerPrefab, _spawnPoint.Position, _spawnPoint.Rotation);
            
            MessageBroker.Default.Publish(new M_PlayerSpawned(_currentPlayer));
            
            InitializeUI(_currentPlayer);

            return _currentPlayer;
        }

        private void DespawnCurrentPlayer()
        {
            if (_currentPlayer == null)
                return;
            
            Destroy(_currentPlayer.gameObject);
            
            _currentPlayer = null;
        }

        private void InitializeUI(Player player)
        {
            _shootView.Initialize(player.PlayerShoot, player.BulletUpgrades);
            _experienceUI.Initialize(player.Experience);
            _upgradeChoicePanel.Initialize(player.Experience, player.UpgradeApplier);
        }
    }
}