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

        private int _currentX;
        private int _currentY;

        protected override void OnDisable()
        {
            base.OnDisable();
            
            if (_spawnGrid != null)
                _spawnGrid.SpawnEnemyAtPosition -= SpawnEnemyAtPosition;
        }
        
        protected override void Initialize()
        {
            base.Initialize();

            if (_spawnGrid != null)
                _spawnGrid.SpawnEnemyAtPosition += SpawnEnemyAtPosition;
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
        
        private void SpawnEnemyAtPosition(Vector3 position)
        {
            if (_objectPool == null)
            {
                Debug.LogError("ObjectPool is not initialized!");
                return;
            }
            
            Enemy enemy = _objectPool.GetFromPool();
            
            if (enemy != null)
            {
                enemy.transform.position = position;
                enemy.gameObject.SetActive(true);
                
                enemy.Released += OnEnemyReleased;
                
                IncreaseObjectCount();
            }
        }
        
        private void SpawnNextInOrder()
        {
            if (_spawnGrid == null) return;
            
            // Проверяем, не вышли ли за пределы сетки
            if (_currentY >= _spawnGrid.GridHeight)
            {
                _currentY = 0;
                _currentX = 0;
            }
            
            Vector3 spawnPosition = _spawnGrid.GetSpawnPosition(_currentX, _currentY);
            SpawnEnemyAtPosition(spawnPosition);
            
            // Переход к следующей позиции
            _currentX++;
            if (_currentX >= _spawnGrid.GridWidth)
            {
                _currentX = 0;
                _currentY++;
            }
        }

        private void SpawnRandomInGrid()
        {
            if (_spawnGrid == null) return;
            
            Vector3 spawnPosition = _spawnGrid.GetRandomSpawnPosition();
            SpawnEnemyAtPosition(spawnPosition);
        }
        
        private void OnEnemyReleased(IPoolable poolable)
        {
            if (poolable is Enemy enemy)
            {
                enemy.Released -= OnEnemyReleased;
                
                DecreaseObjectCount();
            }
        }
    }
}