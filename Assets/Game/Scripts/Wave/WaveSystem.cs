using System;
using System.Collections;
using System.Collections.Generic;
using Game.Scripts.Characters.Enemy;
using Game.Scripts.Levels;
using Game.Scripts.Spawners;
using UnityEngine;

namespace Game.Scripts.Wave
{
    public class WaveSystem : MonoBehaviour
    {
        [SerializeField] private EnemySpawner _enemySpawner;
        [SerializeField] private WaveConfig _config;
        
        [SerializeField, Min(1)] private int _maxSpawnRetryFrames = 120;

        private Coroutine _waveRoutine;
        private int _currentWaveIndex = -1;
        private int _enemiesSpawnedInWave;
        private int _enemiesKilledInWave;
        private int _totalEnemiesInWave;
        
        private bool _isWaveInProgress;

        public int CurrentWaveIndex => _currentWaveIndex + 1;
        public int TotalWaves => _config != null && _config.WavesEnemyCount != null ? _config.WavesEnemyCount.Count : 0;
        public int EnemiesRemaining => _enemiesSpawnedInWave - _enemiesKilledInWave;
        public int TotalEnemiesInWave => _totalEnemiesInWave;

        public event Action<int> WaveStarted;
        public event Action<int> WaveCompleted;
        public event Action<int, int, int> EnemiesCountChanged;
        public event Action AllWavesCompleted;

        private void Start()
        {
            if (_enemySpawner == null)
            {
                Debug.LogError("EnemySpawner not assigned in WaveSystem!");
                return;
            }

            _enemySpawner.enabled = false;
            
            _enemySpawner.EnemyReleased += OnEnemyReleased;
        }

        private void OnDestroy()
        {
            if (_enemySpawner != null)
                _enemySpawner.EnemyReleased -= OnEnemyReleased;
        }

        public void StartWaves()
        {
            if (IsConfigValid(_config) == false)
                return;
            
            if (_waveRoutine != null)
                StopCoroutine(_waveRoutine);

            _currentWaveIndex = -1;
            _waveRoutine = StartCoroutine(WaveRoutine());
        }

        public void StopWaves()
        {
            if (_waveRoutine != null)
            {
                StopCoroutine(_waveRoutine);
                _waveRoutine = null;
            }
            
            _isWaveInProgress = false;
        }
        
        public void SetLevelWaveConfig(WaveConfig waveConfig)
        {
            _config = waveConfig;
        }
        
        public void SetEnemyPoolConfig(IReadOnlyList<EnemyRaceConfig> configs)
        {
            _enemySpawner.SetEnemyPoolConfig(configs);
        }

        public bool IsConfigValid(WaveConfig config)
        {
            return config != null &&
                   config.WavesEnemyCount != null &&
                   config.WavesEnemyCount.Count > 0;
        }

        public IEnumerator WaveRoutine()
        {
            WaitForSeconds wait = new WaitForSeconds(_config.TimeBetweenWaves);
            
            for (int i = 0; i < _config.WavesEnemyCount.Count; i++)
            {
                _currentWaveIndex = i;
                _totalEnemiesInWave = _config.WavesEnemyCount[i];
                
                yield return StartCoroutine(ExecuteWave(_totalEnemiesInWave));

                if (i < _config.WavesEnemyCount.Count - 1)
                    yield return wait;
            }
            
            AllWavesCompleted?.Invoke();
            
            _waveRoutine = null;
            _isWaveInProgress = false;
        }

        public IEnumerator ExecuteWave(int enemyCount)
        {
            InitializeWave(enemyCount);

            yield return StartCoroutine(SpawnWaveEnemies(enemyCount));
    
            yield return StartCoroutine(WaitForWaveCompletion());
    
            FinalizeWave();
        }

        public void InitializeWave(int enemyCount)
        {
            _isWaveInProgress = true;
            _enemiesSpawnedInWave = 0;
            _enemiesKilledInWave = 0;
            
            _enemySpawner.SetMaxObjects(enemyCount);
            _enemySpawner.ResetCurrentCount();
            _enemySpawner.ForceResetCount();
            
            WaveStarted?.Invoke(_currentWaveIndex);
            EnemiesCountChanged?.Invoke(0, enemyCount, 0);
        }

        public IEnumerator SpawnWaveEnemies(int enemyCount)
        {
            int retryFramesLeft = _maxSpawnRetryFrames;
    
            while (_enemiesSpawnedInWave < enemyCount)
            {
                int spawnedBeforePass = _enemiesSpawnedInWave;
                
                while (_enemiesSpawnedInWave < enemyCount)
                {
                    bool spawnResult = _enemySpawner.ForceSpawn();
                    
                    if (spawnResult == false)
                        break;
                    
                    _enemiesSpawnedInWave++;
                    int remaining = _enemiesSpawnedInWave - _enemiesKilledInWave;
                    
                    EnemiesCountChanged?.Invoke(_enemiesSpawnedInWave, enemyCount, remaining);
                }
                
                if (_enemiesSpawnedInWave >= enemyCount)
                    break;
                
                if (_enemiesSpawnedInWave == spawnedBeforePass)
                {
                    retryFramesLeft--;
                    
                    if (retryFramesLeft <= 0)
                    {
                        Debug.LogWarning($"[WaveSystem] Could not spawn full wave. Spawned: {_enemiesSpawnedInWave}/{enemyCount}");
                        break;
                    }
                }
                yield return null;
            }
            _enemySpawner.enabled = false;
        }

        public IEnumerator WaitForWaveCompletion()
        {
            while (_enemiesKilledInWave < _enemiesSpawnedInWave)
                yield return null;
        }

        public void FinalizeWave()
        {
            _isWaveInProgress = false;
            
            WaveCompleted?.Invoke(_currentWaveIndex);
        }

        public void OnEnemyReleased()
        {
            if (_isWaveInProgress && _enemiesKilledInWave < _enemiesSpawnedInWave)
            {
                _enemiesKilledInWave++;
                
                int remaining = _enemiesSpawnedInWave - _enemiesKilledInWave;
                
                EnemiesCountChanged?.Invoke(_enemiesSpawnedInWave, _totalEnemiesInWave, remaining);
            }
        }
    }
}