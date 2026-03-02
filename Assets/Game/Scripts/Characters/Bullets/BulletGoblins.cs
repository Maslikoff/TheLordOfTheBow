using UnityEngine;

namespace Game.Scripts.Characters.Bullets
{
    public class BulletGoblins : Bullet
    {
        protected override void MoveBullet()
        {
            if (_rigidbody != null && _direction != Vector3.zero)
            {
                _rigidbody.velocity = _direction * _speed;
                Debug.Log($"Moving bullet: {_direction} * {_speed} = {_direction * _speed}");
            }
            else
            {
                Debug.LogError($"Bullet not moving: direction={_direction}, rb={_rigidbody != null}");
            }
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