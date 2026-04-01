using UnityEngine;

namespace Game.Scripts.Characters.Bullets
{
    public class EnemyBullet : Bullet
    {
        protected override void MoveBullet()
        {
            if (_rigidbody != null && _direction != Vector3.zero)
                _rigidbody.velocity = _direction * _speed;
        }

        protected override void HandleCollision(Collision other)
        {
            if (other.gameObject.TryGetComponent(out Health player) == false) return;
            
            player.TakeDamage(_baseDamage);
        }

        protected override bool CanCollide(Collision other) => other.gameObject.TryGetComponent(out Enemy.Enemy _) == false &&
                                                               base.CanCollide(other);
    }
}