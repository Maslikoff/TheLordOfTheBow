using UnityEngine;

namespace Game.Scripts.Characters.Bullets
{
    public class BulletGoblins: Bullet
    {
        protected override void MoveBullet()
        {
            _rigidbody.velocity = transform.forward * _speed;
        }

        protected override void HandleCollision(Collision other)
        {
            if (other.gameObject.TryGetComponent(out Health.Health player))
                player.TakeDamage(_damage);
        }
        
        protected override bool CanCollide(Collision other)
        {
            if (other.gameObject.TryGetComponent(out Enemy.Enemy _))
                return false;
                
            return base.CanCollide(other);
        }
    }
}