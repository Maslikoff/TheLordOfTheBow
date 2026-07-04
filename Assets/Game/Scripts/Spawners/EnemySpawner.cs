using System;
using System.Collections.Generic;
using Game.Scripts.Characters.Enemy;
using Game.Scripts.Characters.Player;
using Game.Scripts.Experience;
using Game.Scripts.ObjectPool;
using Game.Scripts.StateServices;
using UnityEngine;
using VContainer;

namespace Game.Scripts.Spawners
{
    public class EnemySpawner : Spawner<Enemy>
    {
        [SerializeField] private SpawnGrid _spawnGrid;
        [SerializeField] private bool _useGridSpawning = true;
        [SerializeField] private bool _spawnInOrder;

        [Header("Enemy Race Settings")] 
        [SerializeField] private BulletSpawner _enemyBulletSpawner;
        [SerializeField] private Race _singleRace = Race.Goblin;
        [SerializeField] private Race[] _multipleRaces;
        [SerializeField] private bool _useMultipleRaces;
        [SerializeField] private bool _useWeightedRandom = true;

        private EnemyPool _enemyPool;
        private IPlayerProvider _playerProvider;
        private IGameStateService _gameStateService;
        private int _currentRaceIndex;
        private int _currentX;
        private int _currentY;
        
        private bool _isGameReady = false;

        public event Action EnemyReleased;
        
        [Inject]
        private void Construct(IPlayerProvider playerProvider, IGameStateService gameStateService)
        {
            _playerProvider = playerProvider;
            _gameStateService = gameStateService;
        }

        protected override void OnEnable()
        {
            if (_spawnGrid != null)
                _spawnGrid.SpawnEnemyAtPosition += SpawnEnemyAtPosition;
        }

        private void Awake()
        {
            if (_objectPool == null)
                _objectPool = GetComponent<EnemyPool>();
            
            _enemyPool = _objectPool as EnemyPool;
        
            if (_enemyPool == null)
                Debug.LogError("[EnemySpawner] Failed to get EnemyPool component in Awake!");
        }

        private void Start()
        {
            if (_gameStateService == null)
            {
                Debug.LogError("[EnemySpawner] GameStateService is null!");
                return;
            }
            
            _gameStateService.GameStarted += OnGameStarted;
            
            if (_gameStateService.IsGameStarted)
            {
                OnGameStarted();
            }
            else
            {
                _isGameReady = false;
                Debug.Log("[EnemySpawner] Waiting for game start...");
            }
        }

        private void OnGameStarted()
        {
            Debug.Log("[EnemySpawner] Game started event received! Enemies can now spawn.");
            _isGameReady = true;
            
            StartSpawning(); 
        }
        
        protected override void OnDisable()
        {
            base.OnDisable();
            
            if (_spawnGrid != null)
                _spawnGrid.SpawnEnemyAtPosition -= SpawnEnemyAtPosition;
            
            if (_gameStateService != null)
                _gameStateService.GameStarted -= OnGameStarted;
        }

        protected override void Initialize()
        {
            if (_objectPool == null)
                _objectPool = GetComponent<EnemyPool>();
    
            _enemyPool = _objectPool as EnemyPool;
            
            if (_enemyPool == null)
                Debug.LogError("[EnemySpawner] Failed to get EnemyPool component!");
            
        }

        protected override void SpawnObject()
        {
            if (_isGameReady) 
                TrySpawnObject();
        }

        private bool TrySpawnObject()
        {
            if (_useGridSpawning && _spawnGrid != null)
            {
                if (_spawnInOrder)
                    return TrySpawnNextInOrder();

                return TrySpawnRandomInGrid();
            }

            return TrySpawnEnemyAtPosition(GetRandomSpawnPosition());
        }
        
        public void SetEnemyPoolConfig(IReadOnlyList<EnemyRaceConfig> configs)
        {
            if (_enemyPool != null)
                _enemyPool.SetEnemyPoolConfig(configs);
        }

        public void ResetCurrentCount()
        {
            _currentObjectsCount = 0;
        }

        public void ForceResetCount()
        {
            _currentObjectsCount = 0;
        }

        public bool ForceSpawn()
        {
            if (_enemyPool == null && _objectPool != null)
                _enemyPool = _objectPool as EnemyPool;
        
            bool canSpawn = CanSpawn();

            if (canSpawn == false) return false;

            return TrySpawnObject();
        }

        public float GetSpawnInterval() => _spawnInterval;

        private void SpawnEnemyAtPosition(Vector3 position)
        {
            TrySpawnEnemyAtPosition(position);
        }

        private bool TrySpawnEnemyAtPosition(Vector3 position)
        {
            if (_isGameReady == false) 
                return false;
            
            if (_objectPool == null)
            {
                Debug.LogError("[EnemySpawner] ObjectPool is null!");
                return false;
            }

            Enemy enemy = GetEnemyFromPool();

            if (enemy == null)
                return false;

            enemy.transform.position = position;

            if (InitializeEnemy(enemy) == false)
            {
                enemy.Release();
                return false;
            }

            enemy.gameObject.SetActive(true);
            enemy.Released += OnEnemyReleased;

            IncreaseObjectCount();
            return true;
        }

        private Enemy GetEnemyFromPool()
        { 
            if (_enemyPool == null && _objectPool != null)
                _enemyPool = _objectPool as EnemyPool;

            if (_enemyPool == null)
            {
                Debug.LogError("[EnemySpawner] EnemyPool is null! Cannot get enemy.");
                return null;
            }

            Enemy enemy;
            
            if (_useMultipleRaces && _multipleRaces.Length > 0)
                enemy = _enemyPool.GetRandomEnemyByWeight();
            else
                enemy = _enemyPool.GetEnemy(_singleRace);

            return enemy;
        }

        private bool InitializeEnemy(Enemy enemy)
        {
            if (_playerProvider.CurrentPlayer == null)
                return false;

            enemy.Initialize(_playerProvider.CurrentPlayer.transform, _enemyBulletSpawner);

            if (enemy.TryGetComponent(out ExperienceReward experienceReward))
                experienceReward.Initialize(_playerProvider.CurrentPlayer.Experience);

            return true;
        }
        
        private bool TrySpawnNextInOrder()
        {
            if (_spawnGrid == null)
                return false;

            if (_currentY >= _spawnGrid.GridHeight)
            {
                _currentY = 0;
                _currentX = 0;
            }

            Vector3 spawnPosition = _spawnGrid.GetSpawnPosition(_currentX, _currentY);
            bool spawned = TrySpawnEnemyAtPosition(spawnPosition);

            if (spawned == false)
                return false;

            _currentX++;

            if (_currentX >= _spawnGrid.GridWidth)
            {
                _currentX = 0;
                _currentY++;
            }

            return true;
        }

        private bool TrySpawnRandomInGrid()
        {
            if (_spawnGrid == null)
                return false;

            Vector3 spawnPosition = _spawnGrid.GetRandomSpawnPosition();
            return TrySpawnEnemyAtPosition(spawnPosition);
        }

        private void OnEnemyReleased(IPoolable poolable)
        {
            if (poolable is Enemy enemy)
            {
                enemy.Released -= OnEnemyReleased;
                EnemyReleased?.Invoke();

                DecreaseObjectCount();
            }
        }
    }
}