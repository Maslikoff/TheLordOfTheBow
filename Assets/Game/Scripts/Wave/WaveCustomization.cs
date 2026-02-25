using System;
using System.Collections;
using System.Collections.Generic;
using Game.Scripts.Characters.Enemy;
using Game.Scripts.PoolSystem;
using Game.Scripts.Spawners;
using UnityEngine;

namespace Game.Scripts.Wave
{
    public class WaveCustomization: MonoBehaviour
    {
         [Header("Wave Configurations")]
        [SerializeField] private List<WaveConfig> _waves = new List<WaveConfig>();
        
        [Header("Auto Progress")]
        [SerializeField] private bool _autoStartNextWave = true;
        [SerializeField] private float _delayBetweenWaves = 3f;
        
        [Header("Spawning")]
        [SerializeField] private ArmySpawner _armySpawner;
        [SerializeField] private Transform _enemiesParent;
        
        public event Action<int> WaveStarted;
        public event Action<int> WaveCompleted;
        public event Action AllWavesCompleted;
        public event Action<int, int> EnemySpawned; 
        
        private int _currentWaveIndex = -1;
        private int _enemiesSpawnedInWave;
        private int _enemiesAlive;
        private Coroutine _waveCoroutine;
        
        public int CurrentWave => _currentWaveIndex + 1;
        public int TotalWaves => _waves.Count;
        public bool IsSpawning => _waveCoroutine != null;
        
        private void Start()
        {
            if (_armySpawner == null)
                _armySpawner = GetComponent<ArmySpawner>();
                
            if (_enemiesParent == null)
                _enemiesParent = transform;
        }
        
        public void StartWaves()
        {
            if (_waves.Count == 0)
                return;
            
            _currentWaveIndex = -1;
            StartNextWave();
        }
        
        public void StartNextWave()
        {
            if (_currentWaveIndex + 1 >= _waves.Count)
            {
                AllWavesCompleted?.Invoke();
                
                return;
            }
            
            if (_waveCoroutine != null)
                StopCoroutine(_waveCoroutine);
                
            _waveCoroutine = StartCoroutine(WaveCoroutine(_waves[++_currentWaveIndex]));
        }
        
        private IEnumerator WaveCoroutine(WaveConfig wave)
        {
            wave.ValidateProbabilities();
            
            _enemiesSpawnedInWave = 0;
            _enemiesAlive = 0;
            
            Debug.Log($"Starting wave {_currentWaveIndex + 1}: {wave.WaveName}");
            WaveStarted?.Invoke(_currentWaveIndex);
            
            if (wave.SpawnAllAtOnce)
            {
                SpawnEnemies(wave, wave.EnemyCount);
            }
            else
            {
                for (int i = 0; i < wave.EnemyCount; i++)
                {
                    SpawnEnemies(wave, 1);
                    
                    _enemiesSpawnedInWave++;
                    EnemySpawned?.Invoke(_enemiesSpawnedInWave, wave.EnemyCount);
                    
                    yield return new WaitForSeconds(wave.SpawnInterval);
                }
            }
            
            yield return new WaitUntil(() => _enemiesAlive <= 0);
            
            Debug.Log($"Wave {_currentWaveIndex + 1} completed!");
            
            WaveCompleted?.Invoke(_currentWaveIndex);
            
            _waveCoroutine = null;
            
            if (_autoStartNextWave)
            {
                yield return new WaitForSeconds(_delayBetweenWaves);
                
                StartNextWave();
            }
        }
        
        private void SpawnEnemies(WaveConfig wave, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Race race = wave.GetRandomRace();
                Enemy enemy = _armySpawner.SpawnEnemy(race);
                
                if (enemy != null)
                {
                    enemy.transform.SetParent(_enemiesParent);
                    
                    // Подписываемся на смерть врага
                    if (enemy is IPoolable poolable)
                    {
                        poolable.Released += OnEnemyReleased;
                    }
                    
                    _enemiesAlive++;
                }
            }
        }
        
        private void OnEnemyReleased(IPoolable enemy)
        {
            if (enemy is Enemy enemyComponent)
            {
                enemyComponent.Released -= OnEnemyReleased;
                _enemiesAlive = Mathf.Max(0, _enemiesAlive - 1);
            }
        }
        
        private void OnDestroy()
        {
            if (_waveCoroutine != null)
                StopCoroutine(_waveCoroutine);
        }
    }
}