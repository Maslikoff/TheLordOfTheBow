using Game.Scripts.Characters.Player;
using Game.Scripts.Levels;
using Game.Scripts.Save;
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
        
        [Inject]
        private void Construct(
            ILevelService levelService,
            PlayerSpawner playerSpawner,
            WaveSystem waveSystem,
            UpgradeChoicePanel upgradeChoicePanel,
            IGameStateService gameStateService,
            ISaveSystem saveSystem,
            LevelSessionService levelSessionService)
        {
            _levelService = levelService;
            _playerSpawner = playerSpawner;
            _waveSystem = waveSystem;
            _upgradeChoicePanel = upgradeChoicePanel;
            _gameStateService = gameStateService;
            _saveSystem = saveSystem;
            _levelSessionService = levelSessionService;
        }

        private void Start()
        {
            _waveSystem.WaveStarted += OnWaveStarted;
            _waveSystem.AllWavesCompleted += OnAllWavesCompleted;
            
            Spawn();
        }

        private void Spawn()
        {
            LevelConfig config = _levelService.CurrentConfig;

            _waveSystem.SetLevelWaveConfig(config.WaveConfig);
            _waveSystem.SetEnemyPoolConfig(config.EnemyRaceConfigs);
            _waveSystem.ResetToPreStartState();
            
            Player player = _playerSpawner.Spawn();
            _upgradeChoicePanel.SetAvailableUpgrades(config.AvailableUpgrades);
            
            _levelSessionService.CaptureSnapshot(player.Experience);

            int levelIndex = _levelService.CurrentLevelIndex;
            int startWaveIndex = _saveSystem.GetWaveCheckpointOrDefault(levelIndex);
            
            if (_gameStateService.IsGameStarted)
                _waveSystem.StartWaves(startWaveIndex);
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
        
        private void OnWaveStarted(int waveIndex)
        {
            _saveSystem.SaveWaveCheckpoint(_levelService.CurrentLevelIndex, waveIndex);
        }

        private void OnAllWavesCompleted()
        {
            _saveSystem.ClearWaveCheckpoint(_levelService.CurrentLevelIndex);
        }
    }
}