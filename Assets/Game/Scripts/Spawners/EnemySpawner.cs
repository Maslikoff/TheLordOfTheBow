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
            base.OnEnable();
            
            if (_spawnGrid != null)
                _spawnGrid.SpawnEnemyAtPosition += SpawnEnemyAtPosition;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            if (_spawnGrid != null)
                _spawnGrid.SpawnEnemyAtPosition -= SpawnEnemyAtPosition;
        }
        
        protected override void Initialize()
        {
            base.Initialize();
            
            _enemyPool = _objectPool as EnemyPool;
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
        
        public bool ForceSpawn()
        {
            if (CanSpawn() == false) return false;
            
            SpawnObject();
            
            return true;
        }

        public float GetSpawnInterval() => _spawnInterval;
        
        private void SpawnEnemyAtPosition(Vector3 position)
        {
            if (_objectPool == null)
                return;
            
            Enemy enemy = GetEnemyFromPool();

            if (enemy != null)
            {
                enemy.transform.position = position;
                enemy.gameObject.SetActive(true);
                
                enemy.Released += OnEnemyReleased;
                
                IncreaseObjectCount();

                if (_useMultipleRaces && _multipleRaces.Length > 0)
                    _currentRaceIndex = (_currentRaceIndex + 1) % _multipleRaces.Length;
            }
        }
        
        private Enemy GetEnemyFromPool()
        {
            if (_enemyPool == null)
                return _objectPool.GetFromPool();

            if (_useMultipleRaces && _multipleRaces.Length > 0)
                return _enemyPool.GetRandomEnemyByWeight();
            else
                return _enemyPool.GetEnemy(_singleRace);
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