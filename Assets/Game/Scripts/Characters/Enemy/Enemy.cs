using System;
using System.Collections;
using Game.Scripts.Characters.Bullets;
using Game.Scripts.Services;
using UnityEngine;
using VContainer;
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

        protected PlayerReferenceService _playerReferenceService;
        private Coroutine _attackCoroutine;

        public Race EnemyRace => _race;

        public event Action<IPoolable> Released;

        protected void OnEnable()
        {
            _attackCoroutine = StartCoroutine(AttackRoutine());
        }

        protected void Update()
        {
            RotateTowardsPlayer();
        }

        protected virtual void OnDisable()
        {
            if (_attackCoroutine != null)
            {
                StopCoroutine(_attackCoroutine);
                _attackCoroutine = null;
            }
        }
        
        [Inject]
        public void Construct(PlayerReferenceService playerReferenceService)
        {
            _playerReferenceService = playerReferenceService;
        }

        protected abstract void Attack();

        protected void RotateTowardsPlayer()
        {
            if (_playerReferenceService?.PlayerTransform == null)
                return;

            Vector3 direction = _playerReferenceService.PlayerTransform.position - transform.position;
            direction.y = 0;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
            }
        }

        public void Release()
        {
            Released?.Invoke(this);
        }

        private IEnumerator AttackRoutine()
        {
            while (enabled)
            {
                Attack();
                yield return new WaitForSeconds(_attackCooldown);
            }
        }
    }
}