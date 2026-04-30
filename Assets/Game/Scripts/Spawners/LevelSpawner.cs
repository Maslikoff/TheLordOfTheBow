using Game.Scripts.Levels;
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

        [Inject]
        private void Construct(
            ILevelService levelService,
            PlayerSpawner playerSpawner,
            WaveSystem waveSystem)
        {
            _levelService = levelService;
            _playerSpawner = playerSpawner;
            _waveSystem = waveSystem;
        }

        private void Start()
        {
            Spawn();
        }

        private void Spawn()
        {
            LevelConfig config = _levelService.CurrentConfig;

            _waveSystem.SetLevelWaveConfig(config.WaveConfig);

            _playerSpawner.Spawn();

            _waveSystem.StartWaves();
        }
    }
}