using System;
using Game.Scripts.Characters.Enemy;
using Game.Scripts.ObjectPool;
using UnityEngine;

namespace Game.Scripts.Spawners
{
    public class EnemySpawner : Spawner<Enemy>
    {
        [SerializeField] private SpawnGrid _spawnGrid;
        [SerializeField] private bool _useGridSpawning = true;
        [SerializeField] private bool _spawnInOrder = false;

        [Header("Enemy Race Settings")] 
        [SerializeField] private Race _singleRace = Race.Goblin;
        [SerializeField] private Race[] _multipleRaces;
        [SerializeField] private bool _useMultipleRaces = false;
        [SerializeField] private bool _useWeightedRandom = true;

        private EnemyPool _enemyPool;
        private int _currentRaceIndex = 0;
        private int _currentX;
        private int _currentY;

        public event Action EnemyReleased;

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

        protected override void OnDisable()
        {
            if (_spawnGrid != null)
                _spawnGrid.SpawnEnemyAtPosition -= SpawnEnemyAtPosition;
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
            if (_useGridSpawning && _spawnGrid != null)
            {
                if (_spawnInOrder)
                    SpawnNextInOrder();
                else
                    SpawnRandomInGrid();
            }
            else
            {
                SpawnEnemyAtPosition(GetRandomSpawnPosition());
            }
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

            SpawnObject();

            return true;
        }

        public float GetSpawnInterval() => _spawnInterval;

        private void SpawnEnemyAtPosition(Vector3 position)
        {
            if (_objectPool == null)
            {
                Debug.LogError("[EnemySpawner] ObjectPool is null!");
                return;
            }

            Enemy enemy = GetEnemyFromPool();

            if (enemy != null)
            {
                if (enemy.gameObject.activeSelf)
                {
                    
                    enemy.Release();
                    enemy = GetEnemyFromPool();

                    if (enemy == null)
                    {
                        Debug.LogError("[EnemySpawner] Failed to get enemy after release!");
                        return;
                    }
                }

                enemy.transform.position = position;
                enemy.gameObject.SetActive(true);

                enemy.Released += OnEnemyReleased;

                IncreaseObjectCount();
            }
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


        private void SpawnNextInOrder()
        {
            if (_spawnGrid == null)
                return;

            if (_currentY >= _spawnGrid.GridHeight)
            {
                _currentY = 0;
                _currentX = 0;
            }

            Vector3 spawnPosition = _spawnGrid.GetSpawnPosition(_currentX, _currentY);
            SpawnEnemyAtPosition(spawnPosition);

            _currentX++;

            if (_currentX >= _spawnGrid.GridWidth)
            {
                _currentX = 0;
                _currentY++;
            }
        }

        private void SpawnRandomInGrid()
        {
            if (_spawnGrid == null)
                return;

            Vector3 spawnPosition = _spawnGrid.GetRandomSpawnPosition();
            
            SpawnEnemyAtPosition(spawnPosition);
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