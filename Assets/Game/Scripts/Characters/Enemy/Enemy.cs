using System;
using UnityEngine;
using Game.Scripts.ObjectPool;
using Game.Scripts.Spawners;

namespace Game.Scripts.Characters.Enemy
{
    [RequireComponent(typeof(EnemyShoot))]
    [RequireComponent(typeof(EnemyRotation))]
    public abstract class Enemy : MonoBehaviour, IPoolable
    {
        [SerializeField] protected Race _race;
        [SerializeField] protected EnemyShoot _enemyShoot;
        [SerializeField] protected EnemyRotation _enemyRotation;

        public Race Race => _race;
        public Transform PlayerTarget { get; protected set; }
        public BulletSpawner Bullets { get; protected set; }

        public event Action<IPoolable> Released;

        protected virtual void OnEnable()
        {
            if (_enemyShoot != null)
                _enemyShoot.ResetShootState();
        }

        private void OnValidate()
        {
            _enemyShoot ??= GetComponent<EnemyShoot>();
            _enemyRotation ??= GetComponent<EnemyRotation>();
        }

        protected virtual void OnDisable()
        {
            Release();
        }

        public void Initialize(Transform playerTarget, BulletSpawner bulletSpawner)
        {
            PlayerTarget = playerTarget;
            Bullets = bulletSpawner;
            
            if (_enemyRotation != null)
                _enemyRotation.SetTarget(playerTarget);
        }

        public void Release()
        {
            Released?.Invoke(this);
        }
    }
}