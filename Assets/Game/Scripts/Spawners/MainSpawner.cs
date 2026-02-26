using System.Collections.Generic;
using Game.Scripts.Characters;
using Game.Scripts.Characters.Bullets;
using Game.Scripts.Characters.Enemy;
using Game.Scripts.PoolSystem;
using UnityEngine;

namespace Game.Scripts.Spawners
{
    public class MainSpawner : MonoBehaviour
    {
        private readonly List<Enemy> _activeEnemies = new();
    
        [Header("Entity Spawner")]
        [SerializeField] private PoolableSpawner<Bullet> _bulletSpawner;
        [SerializeField] private PoolableSpawner<Enemy> _enemySpawner;
        
        [Header("Additional Bullet Spawners")]
        [SerializeField] private PoolableSpawner<Bullet>[] _additionalBulletSpawners;
    
        [Header("Shoot Settings")]
        [SerializeField] private ShootEntity _shootController;
    
        [Header("Reference")]
        [SerializeField] private SpawnGrid _spawnGrid;
        [SerializeField] private Transform _playerShootPoint;
        [SerializeField] private Transform _spawnPointEnemy;
        
        private Dictionary<string, PoolableSpawner<Bullet>> _bulletSpawners;
    
        private void Start()
        {
            InitializeBulletSpawners();
            //_bulletSpawner.Initialize();
            _enemySpawner.Initialize();
    
            _spawnGrid.SpawnAllEnemies();
        }
    
        private void OnEnable()
        {
            _spawnGrid.SpawnEnemyAtPosition += OnSpawnEnemyAtPosition;
            _shootController.ShotFired += OnShoot;
        }
    
        private void OnDisable()
        {
            _spawnGrid.SpawnEnemyAtPosition -= OnSpawnEnemyAtPosition;
            _shootController.ShotFired -= OnShoot;
        }
        
        private void InitializeBulletSpawners()
        {
            _bulletSpawners = new Dictionary<string, PoolableSpawner<Bullet>>();
            
            if (_bulletSpawner != null)
            {
                _bulletSpawners.Add("player_bullet", _bulletSpawner);
                _bulletSpawner.Initialize();
                Debug.Log("Added player_bullet spawner");
            }
            
            if (_additionalBulletSpawners != null)
            {
                for (int i = 0; i < _additionalBulletSpawners.Length; i++)
                {
                    var spawner = _additionalBulletSpawners[i];
                    
                    if (spawner != null)
                    {
                        string poolId = $"bullet_pool_{i}";
                        
                        _bulletSpawners.Add(poolId, spawner);
                        spawner.Initialize();
                        Debug.Log($"Added bullet spawner: {poolId}");
                    }
                }
            }
        }
    
        private void OnShoot()
        {
            if (_bulletSpawners.TryGetValue("player_bullet", out var spawner))
            {
                spawner.Spawn(_playerShootPoint.position);
            }
        }
    
        private void OnSpawnEnemyAtPosition(Vector3 spawnPosition)
        {
            Enemy enemy = _enemySpawner.Spawn(spawnPosition);
    
            if (enemy != null)
                _activeEnemies.Add(enemy);
        }
    }
}
