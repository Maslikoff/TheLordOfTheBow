using System;
using System.Collections;
using Game.Scripts.Characters.Bullets;
using UnityEngine;
using Zenject;
using IPoolable = Game.Scripts.PoolSystem.IPoolable;

namespace Game.Scripts.Characters.Enemy
{
    public abstract class Enemy : MonoBehaviour, IPoolable
    {
        [SerializeField] protected Bullet _bulletPrefab;
        [SerializeField] protected Race _race;
        [SerializeField] protected Transform _shootPoint;
        [SerializeField] protected float _attackCooldown;
        [SerializeField] protected float _damage;
        [SerializeField] protected float _rotationSpeed = 5f;

        protected  Transform _playerTransform;
        private Coroutine _attackCoroutine;
        private Coroutine _rotationCoroutine;

        public Race EnemyRace => _race;

        public event Action<IPoolable> Released;

        protected void OnEnable()
        {
            _attackCoroutine = StartCoroutine(AttackRoutine());
            _rotationCoroutine = StartCoroutine(RotationRoutine());
        }

        protected virtual void OnDisable()
        {
            if (_attackCoroutine != null)
            {
                StopCoroutine(_attackCoroutine);
                _attackCoroutine = null;
            }
            
            if (_rotationCoroutine != null)
            {
                StopCoroutine(_rotationCoroutine);
                _rotationCoroutine = null;
            }
        }

        protected abstract void Attack();
        
        protected void RotateTowardsPlayer()
        {
            if (_playerTransform != null)
            {
                Vector3 direction = _playerTransform.position - transform.position;
                direction.y = 0;
                
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
                }
            }
        }

        public void Release()
        {
            Released?.Invoke(this);
        }

        [Inject]
        public void Construct(IPlayerProvider playerProvider)
        {
            _playerTransform = playerProvider.GetPlayerTransform();
        }

        private IEnumerator AttackRoutine()
        {
            while (enabled)
            {
                Attack();
                yield return new WaitForSeconds(_attackCooldown);
            }
        }
        
        private IEnumerator RotationRoutine()
        {
            while (enabled)
            {
                RotateTowardsPlayer();
                yield return null;
            }
        }
    }
}