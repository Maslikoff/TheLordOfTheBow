using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainSpawner : MonoBehaviour
{
    private readonly List<Enemy> _activeEnemies = new();

    [Header("Entity Spawner")]
    [SerializeField] private PoolableSpawner<Bullet> _bulletSpawner;
    [SerializeField] private PoolableSpawner<Enemy> _enemySpawner;

    [Header("Reference")]
    [SerializeField] private InputHandler _inputHandler;
    [SerializeField] private Transform _playerShootPoint;
    [SerializeField] private Transform _spawnPointEnemy;

    private void Start()
    {
        _bulletSpawner.Initialize();
        _enemySpawner.Initialize();
    }

    private void OnEnable()
    {
        _inputHandler.LeftClick += OnLeftClick;
        _inputHandler.RightClick += OnRightClick;
        _inputHandler.Space += OnSpace;
    }

    private void OnDisable()
    {
        _inputHandler.LeftClick -= OnLeftClick;
        _inputHandler.RightClick -= OnRightClick;
        _inputHandler.Space -= OnSpace;
    }

    private void OnLeftClick()
    {
        _bulletSpawner.Spawn(_playerShootPoint.position);
    }

    private void OnRightClick()
    {
        Enemy enemy = _enemySpawner.Spawn(_spawnPointEnemy.position);

        if (enemy != null)
            _activeEnemies.Add(enemy);
    }

    private void OnSpace()
    {
        if (_activeEnemies.Count == 0)
            return;

        int lastIndex = _activeEnemies.Count - 1;
        Enemy lastEnemy = _activeEnemies[lastIndex];
        _activeEnemies.RemoveAt(lastIndex);

        if (lastEnemy != null && lastEnemy.gameObject.activeSelf)
            lastEnemy.Release();
    }
}