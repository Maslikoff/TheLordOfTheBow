using System;
using System.Collections;
using System.Collections.Generic;
using Game.Scripts.Characters.Enemy;
using Game.Scripts.Spawners;
using UnityEngine;

namespace Game.Scripts.Wave
{
    public class WaveSystem : MonoBehaviour
    {
        [SerializeField] private EnemySpawner _enemySpawner;
        [SerializeField] private WaveConfig _config;
        private Coroutine _routine;
        private int _index = -1;
        private int _spawned;
        private int _killed;
        private bool _running;
        private bool _failed;
        public Func<bool> CanContinue { get; set; }
        public int CurrentWaveIndex => _index + 1;
        public int TotalWaves => _config?.Waves?.Count ?? 0;
        public bool IsWaveInProgress => _running;
        public bool HasFailed => _failed;
        public int EnemiesRemaining => Mathf.Max(0, _spawned - _killed);
        public int TotalEnemiesInWave => _index >= 0 && _index < TotalWaves ? _config.Waves[_index].Total : 0;
        public int UiWaveIndex => TotalWaves == 0 ? 0 : Mathf.Clamp(_index + 1, 1, TotalWaves);
        public int UiWaveEnemyTotal => TotalWaves == 0 ? 0 : _config.Waves[Mathf.Clamp(_index, 0, TotalWaves - 1)].Total;
        public event Action<int> WaveStarted;
        public event Action<int> WaveCompleted;
        public event Action<float> HealingGranted;
        public event Action<int, int, int> EnemiesCountChanged;
        public event Action AllWavesCompleted;
        public event Action<string> WaveFailed;

        private void Awake()
        {
            if (_enemySpawner == null) return;
            _enemySpawner.EnemyKilled += OnEnemyKilled;
            _enemySpawner.EnemyRemovedWithoutDeath += OnUnexpectedRemoval;
        }

        private void OnDestroy()
        {
            if (_enemySpawner == null) return;
            _enemySpawner.EnemyKilled -= OnEnemyKilled;
            _enemySpawner.EnemyRemovedWithoutDeath -= OnUnexpectedRemoval;
        }

        public void SetLevelWaveConfig(WaveConfig config) => _config = config;
        public void SetEnemyPoolConfig(IReadOnlyList<EnemyRaceConfig> configs) => _enemySpawner.SetEnemyPoolConfig(configs);
        public bool IsConfigValid(WaveConfig config)
        {
            if (config?.Waves == null || config.Waves.Count == 0) return false;
            foreach (var wave in config.Waves) if (wave == null || !wave.IsValid) return false;
            return true;
        }

        public void StartWaves(int startWaveIndex = 0)
        {
            StopWaves();
            _failed = false;
            if (_enemySpawner == null || !IsConfigValid(_config))
            {
                Fail("Invalid wave configuration or missing enemy spawner.");
                return;
            }
            _routine = StartCoroutine(Run(Mathf.Clamp(startWaveIndex, 0, TotalWaves - 1)));
        }

        public void StopWaves()
        {
            _running = false;
            if (_routine != null) StopCoroutine(_routine);
            _routine = null;
        }

        private IEnumerator Run(int first)
        {
            for (_index = first; _index < TotalWaves; _index++)
            {
                var wave = _config.Waves[_index];
                var order = wave.CreateSpawnOrder();
                foreach (var race in order)
                    if (!_enemySpawner.SupportsRace(race))
                    {
                        Fail($"Missing prefab/pool for {race} in wave {_index + 1}.");
                        yield break;
                    }
                _spawned = _killed = 0;
                _running = true;
                _enemySpawner.SetMaxObjects(wave.MaxAlive);
                WaveStarted?.Invoke(_index);
                NotifyCounts();
                float nextSpawn = Time.time;
                while (_killed < wave.Total && !_failed)
                {
                    if (Time.timeScale <= 0 || (CanContinue != null && !CanContinue()))
                    {
                        yield return null;
                        continue;
                    }
                    if (_spawned < order.Count && Time.time >= nextSpawn)
                    {
                        var result = _enemySpawner.TrySpawnRace(order[_spawned]);
                        if (result == EnemySpawnResult.InvalidConfiguration)
                        {
                            Fail($"Enemy spawn failed in wave {_index + 1}: invalid pool/grid.");
                            yield break;
                        }
                        if (result == EnemySpawnResult.Spawned)
                        {
                            _spawned++;
                            nextSpawn = Time.time + wave.SpawnInterval;
                            NotifyCounts();
                        }
                    }
                    yield return null;
                }
                if (_failed) yield break;
                while (CanContinue != null && !CanContinue()) yield return null;
                _running = false;
                if (wave.HealAfter > 0) HealingGranted?.Invoke(wave.HealAfter);
                WaveCompleted?.Invoke(_index);
                if (_index < TotalWaves - 1) yield return new WaitForSeconds(_config.TimeBetweenWaves);
            }
            _index = TotalWaves - 1;
            _routine = null;
            AllWavesCompleted?.Invoke();
        }

        private void OnEnemyKilled()
        {
            if (!_running) return;
            _killed++;
            NotifyCounts();
        }

        private void OnUnexpectedRemoval()
        {
            if (_running) Fail("An active enemy was removed without being defeated. Wave aborted.");
        }

        private void NotifyCounts() => EnemiesCountChanged?.Invoke(_spawned, TotalEnemiesInWave, EnemiesRemaining);
        private void Fail(string message)
        {
            _failed = true;
            _running = false;
            Debug.LogError("[WaveSystem] " + message);
            WaveFailed?.Invoke(message);
        }

        public void ResetToPreStartState()
        {
            StopWaves();
            _index = -1;
            _spawned = _killed = 0;
            _failed = false;
            EnemiesCountChanged?.Invoke(0, UiWaveEnemyTotal, 0);
        }
    }
}
