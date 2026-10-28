using Cysharp.Threading.Tasks;
using Game.Scripts.AddressablesContent;
using Game.Scripts.Characters.Player;
using Game.Scripts.Levels;
using Game.Scripts.ObjectPool;
using Game.Scripts.Save;
using Game.Scripts.Shop;
using Game.Scripts.StateServices;
using Game.Scripts.UI;
using Game.Scripts.Wave;
using UnityEngine;
using VContainer;

namespace Game.Scripts.Spawners
{
    public class LevelSpawner : MonoBehaviour
    {
        private ILevelService _levelService;
        private PlayerSpawner _playerSpawner;
        private WaveSystem _waveSystem;
        private UpgradeChoicePanel _upgradeChoicePanel;
        private IGameStateService _gameStateService;
        private ISaveSystem _saveSystem;
        private LevelSessionService _levelSessionService;
        private ICurrencyService _currencyService;
        private BulletPool[] _bulletPools;
        private EffectsPool _effectsPool;
        private GroundPool _groundPool;
        private HealthKitPool _healthKitPool;

        private IAddressablesContentService _content;
        private int _startWave;

        [Inject]
        private void Construct(
            ILevelService levelService,
            PlayerSpawner playerSpawner,
            WaveSystem waveSystem,
            UpgradeChoicePanel upgradeChoicePanel,
            IGameStateService gameStateService,
            ISaveSystem saveSystem,
            LevelSessionService levelSessionService,
            ICurrencyService currencyService,
            IAddressablesContentService content)
        {
            _levelService = levelService;
            _playerSpawner = playerSpawner;
            _waveSystem = waveSystem;
            _upgradeChoicePanel = upgradeChoicePanel;
            _gameStateService = gameStateService;
            _saveSystem = saveSystem;
            _levelSessionService = levelSessionService;
            _currencyService = currencyService;
            _content = content;
        }

        private void Start()
        {
            _waveSystem.WaveStarted += OnWaveStarted;
            _waveSystem.HealingGranted += OnHealingGranted;
            SpawnAsync().Forget();
        }

        private async UniTaskVoid SpawnAsync()
        {
            await WarmupPoolsAsync();

            LevelConfig config = _levelService.CurrentConfig;

            _waveSystem.SetLevelWaveConfig(config.WaveConfig);
            _waveSystem.SetEnemyPoolConfig(config.EnemyRaceConfigs);
            _waveSystem.ResetToPreStartState();

            Player player = await _playerSpawner.SpawnAsync();
            _upgradeChoicePanel.SetAvailableUpgrades(config.AvailableUpgrades);

            _startWave = _saveSystem.BeginStage(player);
            _waveSystem.CanContinue = () => player != null && !player.IsDead && !_upgradeChoicePanel.HasPendingChoices;

            if (_gameStateService.IsGameStarted)
                _waveSystem.StartWaves(_startWave);
            else
                _gameStateService.GameStarted += OnGameStarted;
        }

        private async UniTask WarmupPoolsAsync()
        {
            _bulletPools = FindObjectsOfType<BulletPool>(true);
            _effectsPool = FindObjectOfType<EffectsPool>(true);
            _groundPool = FindObjectOfType<GroundPool>(true);
            _healthKitPool = FindObjectOfType<HealthKitPool>(true);

            if (_bulletPools != null)
            {
                foreach (BulletPool pool in _bulletPools)
                {
                    if (pool != null)
                        await pool.WarmupAsync(_content);
                }
            }

            if (_effectsPool != null)
                await _effectsPool.WarmupAsync(_content);

            if (_groundPool != null)
                await _groundPool.WarmupAsync(_content);

            if (_healthKitPool != null)
                await _healthKitPool.WarmupAsync(_content);
        }

        private void OnGameStarted()
        {
            _gameStateService.GameStarted -= OnGameStarted;
            _waveSystem.StartWaves(_startWave);
        }

        private void OnDestroy()
        {
            _waveSystem.StopWaves();
            _waveSystem.WaveStarted -= OnWaveStarted;
            _waveSystem.HealingGranted -= OnHealingGranted;
            if (_gameStateService != null)
                _gameStateService.GameStarted -= OnGameStarted;
        }

        private void OnWaveStarted(int waveIndex)
        {
            _saveSystem.SaveWaveCheckpoint(_levelService.CurrentLevelIndex, waveIndex);
        }

        private void OnHealingGranted(float fraction)
        {
            var health = _playerSpawner.CurrentPlayer.PlayerHealth;
            health.Heal(Mathf.CeilToInt(health.MaxCount * fraction));
        }
    }
}
