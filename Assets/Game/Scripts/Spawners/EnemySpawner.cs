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
        private readonly Dictionary<Enemy, Vector2Int> _enemyCells = new();

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

        private bool _isGameReady;

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
                OnGameStarted();
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
            _currentX = 0;
            _currentY = 0;
            _enemyCells.Clear();
            _spawnGrid?.ResetOccupancy();
        }

        public void ForceResetCount()
        {
            _currentObjectsCount = 0;
        }

        public bool ForceSpawn()
        {
            if (_enemyPool == null && _objectPool != null)
                _enemyPool = _objectPool as EnemyPool;

            if (CanSpawn() == false)
                return false;

            return TrySpawnObject();
        }

        public float GetSpawnInterval() => _spawnInterval;

        private void SpawnEnemyAtPosition(Vector3 position)
        {
            TrySpawnEnemyAtPosition(position);
        }

        // Спавн без привязки к сетке (legacy / SpawnGrid event)
        private bool TrySpawnEnemyAtPosition(Vector3 position)
        {
            return TrySpawnEnemyAtPosition(position, -1, -1);
        }

        // Спавн с привязкой к ячейке сетки (gridX, gridY >= 0)
        private bool TrySpawnEnemyAtPosition(Vector3 position, int gridX, int gridY)
        {
            bool useGridCell = gridX >= 0 && gridY >= 0;

            if (_isGameReady == false)
            {
                if (useGridCell)
                    _spawnGrid?.Release(gridX, gridY);

                return false;
            }

            if (_objectPool == null)
            {
                Debug.LogError("[EnemySpawner] ObjectPool is null!");

                if (useGridCell)
                    _spawnGrid?.Release(gridX, gridY);

                return false;
            }

            Enemy enemy = GetEnemyFromPool();

            if (enemy == null)
            {
                if (useGridCell)
                    _spawnGrid?.Release(gridX, gridY);

                return false;
            }

            enemy.transform.position = position;

            if (InitializeEnemy(enemy) == false)
            {
                if (useGridCell)
                    _spawnGrid?.Release(gridX, gridY);

                enemy.Release();
                return false;
            }

            if (useGridCell)
                _enemyCells[enemy] = new Vector2Int(gridX, gridY);

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

            if (_useMultipleRaces && _multipleRaces.Length > 0)
                return _enemyPool.GetRandomEnemyByWeight();

            return _enemyPool.GetEnemy(_singleRace);
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

            if (_spawnGrid.TryGetNextFreeCell(_currentX, _currentY, out int x, out int y) == false)
                return false;

            _currentX = x + 1;

            if (_currentX >= _spawnGrid.GridWidth)
            {
                _currentX = 0;
                _currentY = y + 1;
            }

            return TrySpawnEnemyAtPosition(_spawnGrid.GetSpawnPosition(x, y), x, y);
        }

        private bool TrySpawnRandomInGrid()
        {
            if (_spawnGrid == null || _spawnGrid.TryGetRandomFreeCell(out int x, out int y) == false)
                return false;

            return TrySpawnEnemyAtPosition(_spawnGrid.GetSpawnPosition(x, y), x, y);
        }

        private void OnEnemyReleased(IPoolable poolable)
        {
            if (poolable is not Enemy enemy)
                return;

            enemy.Released -= OnEnemyReleased;

            if (_enemyCells.TryGetValue(enemy, out Vector2Int cell))
            {
                _spawnGrid?.Release(cell.x, cell.y);
                _enemyCells.Remove(enemy);
            }

            EnemyReleased?.Invoke();
            DecreaseObjectCount();
        }
    }
}