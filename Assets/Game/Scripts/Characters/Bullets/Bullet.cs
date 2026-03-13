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
        [SerializeField] protected float _damage;
        [SerializeField] protected BulletType _bulletType;
        [SerializeField] protected Rigidbody _rigidbody;

        protected Vector3 _direction;
        
        public float Speed => _speed;
        
        public event Action<IPoolable> Released;

        protected virtual void OnValidate()
        {
            _rigidbody ??= GetComponent<Rigidbody>();
        }

        protected virtual void FixedUpdate()
        {
            MoveBullet();
        }

        protected virtual void OnCollisionEnter(Collision other)
        {
            if (CanCollide(other))
            {
                HandleCollision(other);
                Release();
            }
        }

        protected abstract void MoveBullet();

        protected abstract void HandleCollision(Collision other);

        protected virtual bool CanCollide(Collision other)
        {
            return other.gameObject.TryGetComponent(out Enemy.Enemy _) ||
                   other.gameObject.TryGetComponent(out Wall _) ||
                   other.gameObject.TryGetComponent(out Player.Player _);
        }

        public virtual void SetDirection(Vector3 direction)
        {
            _direction = direction.normalized;
        }

        public virtual void Release()
        {
            Released?.Invoke(this);

            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
        }
    }
}