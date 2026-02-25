using System;
using System.Collections.Generic;
using Game.Scripts.Characters.Enemy;
using Game.Scripts.PoolSystem;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Scripts.Spawners
{
    public class ArmySpawner : MonoBehaviour
    {
        [Header("Spawners")]
        [SerializeField] private PoolableSpawner<Enemy> _enemySpawner;
        [SerializeField] private SpawnGrid _spawnGrid;
        
        [Header("Enemy Prefabs")]
        [SerializeField] private Enemy _goblinPrefab;
        [SerializeField] private Enemy _orcPrefab;
        [SerializeField] private Enemy _trollPrefab;
        
        [Header("Spawn Settings")]
        [SerializeField] private int _maxEnemisPerWavek;
        [SerializeField] private bool _randomPosition = true;
        
        private List<Vector3> _availableSpawnPositions = new List<Vector3>();
        private List<Enemy> _activeEnemies = new List<Enemy>();
        
        private bool _isInitialized;

        private void Start()
        {
            InitializeSpawner();
        }
        
        public void InitializeSpawner()
        {
            if (_isInitialized) return;
            
            if (_enemySpawner != null && _enemySpawner.IsInitialized == false)
                _enemySpawner.Initialize();
            
            InitializeSpawnPositions();
            _isInitialized = true;
        }

        public Enemy SpawnEnemy(Race race)
        {
            if(_availableSpawnPositions.Count == 0)
                return null;

            Vector3 spawnPosition = GetSpawnPosition();
            Enemy prefab = GetPrefabByRace(race);
            
            if (prefab == null)
            {
                ReturnSpawnPosition(spawnPosition);
                return null;
            }
            
            Enemy enemy = _enemySpawner.Spawn(spawnPosition);
            
            if (enemy != null)
            {
                _activeEnemies.Add(enemy);
                
                if (enemy is IPoolable poolable)
                    poolable.Released += OnEnemyReleased;
            }
            else
            {
                ReturnSpawnPosition(spawnPosition);
            }
            
            return enemy;
        }
        
        public void ReturnSpawnPosition(Vector3 position)
        {
            if (_availableSpawnPositions.Contains(position) == false)
                _availableSpawnPositions.Add(position);
        }

        private void InitializeSpawnPositions()
        {
            _availableSpawnPositions.Clear();

            for (int y = 0; y < _spawnGrid.GridHeight; y++)
            {
                for (int x = 0; x < _spawnGrid.GridWidth; x++)
                {
                    Vector3 position = _spawnGrid.GetSpawnPosition(x, y);
                    _availableSpawnPositions.Add(position);
                }
            }
        }
        
        private Enemy GetPrefabByRace(Race race)
        {
            return race switch
            {
                Race.Goblin => _goblinPrefab,
                Race.Orc => _orcPrefab,
                Race.Troll => _trollPrefab,
                _ => _goblinPrefab
            };
        }
        
        private void OnEnemyReleased(IPoolable poolable)
        {
            if (poolable is Enemy enemy)
            {
                poolable.Released -= OnEnemyReleased;
                
                if (_activeEnemies.Contains(enemy))
                {
                    _activeEnemies.Remove(enemy);
                    ReturnSpawnPosition(enemy.transform.position);
                }
            }
        }
        
        private Vector3 GetSpawnPosition()
        {
            Vector3 spawnPosition;

            if (_randomPosition)
            {
                int randomIndex = Random.Range(0, _availableSpawnPositions.Count);
                spawnPosition = _availableSpawnPositions[randomIndex];
                _availableSpawnPositions.RemoveAt(randomIndex);
            }
            else
            {
                spawnPosition = _availableSpawnPositions[0];
                _availableSpawnPositions.RemoveAt(0);
            }
            
            return spawnPosition;
        }
    }
}