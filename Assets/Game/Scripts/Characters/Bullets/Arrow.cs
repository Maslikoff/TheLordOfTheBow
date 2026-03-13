using UnityEngine;

namespace Game.Scripts.Characters.Bullets
{
    public class Arrow : Bullet
    {
        protected override void MoveBullet()
        {
            _rigidbody.velocity = transform.forward * _speed;
        }
        
        protected override void HandleCollision(Collision other)
        {
            if (other.gameObject.TryGetComponent(out Enemy.Enemy enemy))
                if (enemy.TryGetComponent(out Health health))
                    health.TakeDamage(_damage);
        }
        
        protected override bool CanCollide(Collision other)
        {
            if (other.gameObject.TryGetComponent(out Player.Player _))
                return false;
                
            return base.CanCollide(other);
        }
    }
}