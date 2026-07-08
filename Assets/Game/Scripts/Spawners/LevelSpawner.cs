using Game.Scripts.Levels;
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

        [Inject]
        private void Construct(
            ILevelService levelService,
            PlayerSpawner playerSpawner,
            WaveSystem waveSystem,
            UpgradeChoicePanel upgradeChoicePanel,
            IGameStateService gameStateService)
        {
            _levelService = levelService;
            _playerSpawner = playerSpawner;
            _waveSystem = waveSystem;
            _upgradeChoicePanel = upgradeChoicePanel;
            _gameStateService = gameStateService;
        }

        private void Start()
        {
            Spawn();
        }

        private void Spawn()
        {
            LevelConfig config = _levelService.CurrentConfig;

            _waveSystem.SetLevelWaveConfig(config.WaveConfig);
            _waveSystem.SetEnemyPoolConfig(config.EnemyRaceConfigs);
            _waveSystem.ResetToPreStartState();
            
            _playerSpawner.Spawn();
            _upgradeChoicePanel.SetAvailableUpgrades(config.AvailableUpgrades);

            if (_gameStateService.IsGameStarted)
                _waveSystem.StartWaves();
            else
                _gameStateService.GameStarted += OnGameStarted;
        }
        
        private void OnGameStarted()
        {
            _gameStateService.GameStarted -= OnGameStarted;
            _waveSystem.StartWaves();
        }
        
        private void OnDestroy()
        {
            if (_gameStateService != null)
                _gameStateService.GameStarted -= OnGameStarted;
        }
    }
}