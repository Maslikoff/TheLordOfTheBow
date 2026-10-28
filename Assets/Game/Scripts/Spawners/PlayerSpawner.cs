using Cysharp.Threading.Tasks;
using Game.Scripts.AddressablesContent;
using Game.Scripts.Characters.Player;
using Game.Scripts.Levels;
using Game.Scripts.UI;
using Game.Scripts.Shop;
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
        private IAddressablesContentService _content;
        private Player _currentPlayer;
        private ICosmeticInventoryService _inventory;
        private ShopCatalogProvider _shopCatalog;

        public Player CurrentPlayer => _currentPlayer;

        [Inject]
        private void Construct(
            IObjectFactory factory,
            GameStartupConfig startupConfig,
            IAddressablesContentService content,
            ICosmeticInventoryService inventory,
            ShopCatalogProvider shopCatalog)
        {
            _factory = factory;
            _startupConfig = startupConfig;
            _content = content;
            _inventory = inventory;
            _shopCatalog = shopCatalog;
        }

        public async UniTask<Player> SpawnAsync()
        {
            Player playerPrefab = await _startupConfig.GetPlayerPrefabAsync(_content);

            DespawnCurrentPlayer();

            _currentPlayer = _factory.Create(playerPrefab, _spawnPoint.Position, _spawnPoint.Rotation);
            var skin = _currentPlayer.GetComponent<PlayerSkinController>();
            if (skin != null)
                await skin.InitializeAsync(_inventory, _shopCatalog, _content);
            InitializeUI(_currentPlayer);
            MessageBroker.Default.Publish(new M_PlayerSpawned(_currentPlayer));
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
