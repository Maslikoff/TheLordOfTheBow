using UnityEngine;

namespace Game.Scripts.Characters.Bullets
{
    public class BulletGoblins : Bullet
    {
        protected override void MoveBullet()
        {
            if (_rigidbody != null && _direction != Vector3.zero)
                _rigidbody.velocity = _direction * _speed;
        }

        protected override void HandleCollision(Collision other)
        {
            if (other.gameObject.TryGetComponent(out Health.Health player))
            {
                player.TakeDamage(_damage);
                Release();
            }
        }

        protected override bool CanCollide(Collision other)
        {
            if (other.gameObject.TryGetComponent(out Enemy.Enemy _))
                return false;

            return base.CanCollide(other);
        }
    }
}