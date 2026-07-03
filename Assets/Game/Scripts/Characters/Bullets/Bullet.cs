using System;
using Game.Scripts.Environment;
using Game.Scripts.ObjectPool;
using UnityEngine;

namespace Game.Scripts.Characters.Bullets
{
    [RequireComponent(typeof(Rigidbody))]
    public abstract class Bullet : MonoBehaviour, IPoolable
    {
        [SerializeField] [Min(0)] protected float _speed;
        [SerializeField] protected float _baseDamage;
        [SerializeField] protected Rigidbody _rigidbody;
        [SerializeField] protected bool _destroyOnCollision = true;
        [SerializeField] protected float _spawnGraceTime = 0.1f;

        protected Vector3 _direction;
        protected float _currentDamage;
        protected Transform _owner;
        protected float _spawnTime;
        
        private BulletType _poolType;

        public event Action<IPoolable> Released;
        
        public BulletType PoolType => _poolType;

        protected virtual void OnValidate()
        {
            _rigidbody ??= GetComponent<Rigidbody>();
        }

        protected virtual void FixedUpdate()
        {
            MoveBullet();
        }

        protected void OnCollisionEnter(Collision other)
        {
            if (IsInSpawnGrace())
                return;
            
            if (IsOwnerCollision(other))
                return;
            
            if (CanCollide(other) == false)
                return;
            
            HandleCollision(other);
            
            if (_destroyOnCollision)
                Release();
        }

        public virtual void Initialize(BulletData bulletData)
        {
            _currentDamage = bulletData.Damage;
            _spawnTime = Time.time;
        }

        public void SetPoolType(BulletType poolType) => _poolType = poolType;
        
        public void SetOwner(Transform owner) => _owner = owner;
        
        protected abstract void MoveBullet();
        
        protected abstract void HandleCollision(Collision other);
        
        protected virtual bool CanCollide(Collision other) => other.gameObject.TryGetComponent(out Enemy.Enemy _) || 
                                                              other.gameObject.TryGetComponent(out Wall _) ||
                                                              other.gameObject.TryGetComponent(out Player.Player _);
        public void SetDirection(Vector3 direction)
        {
            _direction = direction.normalized;
            transform.rotation = Quaternion.LookRotation(_direction);
        }
        
        public virtual void Release()
        {
            Released?.Invoke(this);
            
            if (_rigidbody != null)
            {
                _rigidbody.velocity = Vector3.zero;
                _rigidbody.angularVelocity = Vector3.zero;
            }
            
            _owner = null;
        }
        
        protected bool IsInSpawnGrace() => Time.time - _spawnTime < _spawnGraceTime;
        
        protected bool IsOwnerCollision(Collision other)
        {
            if (_owner == null)
                return false;
            
            Transform otherTransform = other.transform;
            
            return otherTransform == _owner || otherTransform.IsChildOf(_owner);
        }
    }
}