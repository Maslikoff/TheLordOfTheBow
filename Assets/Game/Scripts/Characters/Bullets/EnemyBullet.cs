using UnityEngine;

namespace Game.Scripts.Characters.Bullets
{
    public class EnemyBullet : Bullet
    {
        public override void Initialize(BulletData bulletData)
        {
            base.Initialize(bulletData);
            
            if (_owner != null)
                EnemyBulletTracker.Register(_owner, this);
        }
        
        protected override void MoveBullet()
        {
            if (_rigidbody != null && _direction != Vector3.zero)
                _rigidbody.velocity = _direction * _speed;
        }

        protected override void HandleCollision(Collision other)
        {
            if (other.gameObject.TryGetComponent(out Health player) == false) 
                return;
            
            player.TakeDamage(_baseDamage);
        }

        protected override bool CanCollide(Collision other) => 
            other.gameObject.TryGetComponent(out Enemy.Enemy _) == false && base.CanCollide(other);
        
        public override void Release()
        {
            if (_owner != null)
                EnemyBulletTracker.Unregister(_owner, this);
            
            base.Release();
        }
    }
}