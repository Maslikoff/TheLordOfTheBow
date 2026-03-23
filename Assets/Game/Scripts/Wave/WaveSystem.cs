using System;
using System.Collections;
using Game.Scripts.Spawners;
using UnityEngine;

namespace Game.Scripts.Wave
{
    public class WaveSystem : MonoBehaviour
    {
        [SerializeField] private EnemySpawner _enemySpawner;
        [SerializeField] private WaveConfig _config;
        [SerializeField] private bool _autoStartWaves = true;

        private Coroutine _waveRoutine;
        private int _currentWaveIndex = -1;
        private int _enemiesSpawnedInWave;
        private int _enemiesKilledInWave;
        private int _totalEnemiesInWave;
        
        private bool _isWaveInProgress;
        private bool _waveSpawningComplete;

        public int CurrentWaveIndex => _currentWaveIndex + 1;
        public int TotalWaves => _config.WavesEnemyCount.Count;
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

            if (_autoStartWaves)
                StartWaves();
        }

        private void OnDestroy()
        {
            if (_enemySpawner != null)
                _enemySpawner.EnemyReleased -= OnEnemyReleased;
        }

        public void StartWaves()
        {
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

        private IEnumerator WaveRoutine()
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

        private IEnumerator ExecuteWave(int enemyCount)
        {
            InitializeWave(enemyCount);

            yield return StartCoroutine(SpawnWaveEnemies(enemyCount));
    
            yield return StartCoroutine(WaitForWaveCompletion());
    
            FinalizeWave();
        }

        private void InitializeWave(int enemyCount)
        {
            _isWaveInProgress = true;
            _enemiesSpawnedInWave = 0;
            _enemiesKilledInWave = 0;
            _waveSpawningComplete = false;
            
            _enemySpawner.SetMaxObjects(enemyCount);
            _enemySpawner.ResetCurrentCount();
            _enemySpawner.ForceResetCount();
            
            WaveStarted?.Invoke(_currentWaveIndex);
            EnemiesCountChanged?.Invoke(0, enemyCount, 0);
        }
        
        private IEnumerator SpawnWaveEnemies(int enemyCount)
        {
            WaitForSeconds spawnDelay = new WaitForSeconds(_enemySpawner.GetSpawnInterval());
    
            while (_enemiesSpawnedInWave < enemyCount)
            {
                bool spawnResult = _enemySpawner.ForceSpawn();
                
                if (spawnResult)
                {
                    _enemiesSpawnedInWave++;
                    int remaining = _enemiesSpawnedInWave - _enemiesKilledInWave;
            
                    EnemiesCountChanged?.Invoke(_enemiesSpawnedInWave, enemyCount, remaining);
                }

                yield return spawnDelay;
            }
    
            _waveSpawningComplete = true;
            _enemySpawner.enabled = false;
        }
        
        private IEnumerator WaitForWaveCompletion()
        {
            while (_enemiesKilledInWave < _enemiesSpawnedInWave)
                yield return null;
        }

        private void FinalizeWave()
        {
            _isWaveInProgress = false;
            
            WaveCompleted?.Invoke(_currentWaveIndex);
        }
        
        private void OnEnemyReleased()
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