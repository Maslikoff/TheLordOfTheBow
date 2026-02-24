using System;
using System.Collections;
using Game.Scripts.Characters.Bullets;
using Game.Scripts.PoolSystem;
using UnityEngine;

namespace Game.Scripts.Characters.Enemy
{
    public abstract class Enemy : MonoBehaviour, IPoolable
    {
        [SerializeField] protected Bullet _bulletPrefab;
        [SerializeField] protected Race _race;
        [SerializeField] protected Transform _shootPoint;
        [SerializeField] protected float _attackCooldown;
        [SerializeField] protected float _damage;

        private Coroutine _attackCoroutine;

        public Race Race => _race;

        public event Action<IPoolable> Released;

        protected void OnEnable()
        {
            _attackCoroutine = StartCoroutine(AttackRoutine());
        }

        protected virtual void OnDisable()
        {
            if (_attackCoroutine != null)
            {
                StopCoroutine(_attackCoroutine);
                _attackCoroutine = null;
            }
        }

        protected abstract void Attack();

        private IEnumerator AttackRoutine()
        {
            while (enabled)
            {
                Attack();
                yield return new WaitForSeconds(_attackCooldown);
            }
        }
        
        public void Release()
        {
            Released?.Invoke(this);
        }
    }
}