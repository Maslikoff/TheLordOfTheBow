using UnityEngine;

namespace Game.Scripts.Characters.Bullets
{
    public class PoisonArrow : Bullet
    {
        private float _poisonDamage;
        private float _poisonDuration;

        private void Awake()
        {
            _destroyOnCollision = true;
        }

        public override void Initialize(BulletData bulletData)
        {
            base.Initialize(bulletData);
            _poisonDamage = bulletData.PoisonDamage;
            _poisonDuration = bulletData.LifeTime;
        }

        protected override void MoveBullet()
        {
            _rigidbody.velocity = transform.forward * _speed;
        }

        protected override void HandleCollision(Collision other)
        {
            if (other.gameObject.TryGetComponent(out Enemy.Enemy _) == false)
                return;

            if (other.gameObject.TryGetComponent(out Health health) == false)
                return;

            health.TakeDamage(_currentDamage);

            if (other.gameObject.TryGetComponent(out PoisonStatus poisonStatus) == false)
                poisonStatus = other.gameObject.AddComponent<PoisonStatus>();

            poisonStatus.Apply(_poisonDamage, _poisonDuration);
        }

        protected override bool CanCollide(Collision other)
        {
            if (other.gameObject.TryGetComponent(out Player.Player _))
                return false;

            return base.CanCollide(other);
        }
    }
}
