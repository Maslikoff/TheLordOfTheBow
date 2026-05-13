using Game.Scripts.Levels;
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

        [Inject]
        private void Construct(
            ILevelService levelService,
            PlayerSpawner playerSpawner,
            WaveSystem waveSystem,
            UpgradeChoicePanel upgradeChoicePanel)
        {
            _levelService = levelService;
            _playerSpawner = playerSpawner;
            _waveSystem = waveSystem;
            _upgradeChoicePanel = upgradeChoicePanel;
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

            _playerSpawner.Spawn();
            _upgradeChoicePanel.SetAvailableUpgrades(config.AvailableUpgrades);

            _waveSystem.StartWaves();
        }
    }
}