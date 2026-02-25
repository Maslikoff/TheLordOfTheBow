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
    
        [Header("Shoot Settings")]
        [SerializeField] private ShootEntity _shootController;
    
        [Header("Reference")]
        [SerializeField] private SpawnGrid _spawnGrid;
        [SerializeField] private Transform _playerShootPoint;
        [SerializeField] private Transform _spawnPointEnemy;
    
        private void Start()
        {
            _bulletSpawner.Initialize();
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
    
        private void OnShoot()
        {
            _bulletSpawner.Spawn(_playerShootPoint.position);
        }
    
        private void OnSpawnEnemyAtPosition(Vector3 spawnPosition)
        {
            Enemy enemy = _enemySpawner.Spawn(spawnPosition);
    
            if (enemy != null)
                _activeEnemies.Add(enemy);
        }
    }
}
