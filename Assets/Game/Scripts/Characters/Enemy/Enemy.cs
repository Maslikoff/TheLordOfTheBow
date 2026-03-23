using System;
using UnityEngine;
using Game.Scripts.ObjectPool;
using Game.Scripts.Spawners;
using Game.Scripts.UI;

namespace Game.Scripts.Characters.Enemy
{
    [RequireComponent(typeof(EnemyShoot))]
    [RequireComponent(typeof(EnemyRotation))]
    [RequireComponent(typeof(Health))]
    public abstract class Enemy : MonoBehaviour, IPoolable
    {
        [SerializeField] protected Race _race;
        [SerializeField] protected EnemyShoot _enemyShoot;
        [SerializeField] protected EnemyRotation _enemyRotation;
        [SerializeField] protected Health _health;
        [SerializeField] protected DamagePopup _damagePopup;
        
        public Race RaceEnemy => _race;
        public Transform PlayerTarget { get; protected set; }
        public BulletSpawner Bullets { get; protected set; }

        public event Action<IPoolable> Released;

        protected virtual void OnEnable()
        {
            if (_enemyShoot != null)
                _enemyShoot.ResetShootState();
            
            if (_damagePopup != null)
                _damagePopup.ResetPopup();
            
            if (_health != null)
            {
                _health.DamageTaken += OnDamageTaken;
                _health.Death += Release;
            }
        }

        private void OnValidate()
        {
            _enemyShoot ??= GetComponent<EnemyShoot>();
            _enemyRotation ??= GetComponent<EnemyRotation>();
            _health ??= GetComponent<Health>();
        }

        protected virtual void OnDisable()
        {
            if (_health != null)
            {
                _health.DamageTaken -= OnDamageTaken;
                _health.Death -= Release;
            }
            
            if (_damagePopup != null)
                _damagePopup.ResetPopup();
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
        
        private void OnDamageTaken(float damage)
        {
            if (_damagePopup != null)
                _damagePopup.ShowDamage(damage);
        }
    }
}