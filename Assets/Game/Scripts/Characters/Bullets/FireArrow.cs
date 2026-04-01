using UnityEngine;

namespace Game.Scripts.Characters.Bullets
{
    public class FireArrow : Bullet
    {
        [SerializeField] private float _defaultLifeTime = 10f;
        [SerializeField] private float _horizontalDrift = 1.5f;

        private float _maxLifeTime;
        private float _currentLifeTime;
        private Vector3 _currentVelocity;
        private float _currentHorizontalOffset;

        private void OnEnable()
        {
            _currentLifeTime = 0f;
            _currentVelocity = Vector3.zero;
            _currentHorizontalOffset = 0f;

            _destroyOnCollision = false;
        }

        protected override void FixedUpdate()
        {
            _currentLifeTime += Time.fixedDeltaTime;

            if (_currentLifeTime >= _maxLifeTime)
            {
                Release();
                return;
            }

            MoveBullet();
        }

        public override void Initialize(BulletData bulletData)
        {
            base.Initialize(bulletData);
            
            _maxLifeTime = bulletData.LifeTime > 0 ? bulletData.LifeTime : _defaultLifeTime;
        }

        protected override void MoveBullet()
        {
            if (_currentVelocity == Vector3.zero)
            {
                Vector3 startDirection = transform.forward;
                startDirection.x = 0;
                _currentVelocity = startDirection.normalized * _speed;
            }

            _rigidbody.velocity = _currentVelocity;
        }

        protected override void HandleCollision(Collision other)
        {
            if (other.gameObject.TryGetComponent(out Enemy.Enemy enemy))
                if (enemy.TryGetComponent(out Health health))
                    health.TakeDamage(_currentDamage);

            BounceVertically(other);
        }

        protected override bool CanCollide(Collision other)
        {
            if (other.gameObject.TryGetComponent(out Player.Player _))
                return false;

            return base.CanCollide(other);
        }

        public override void Release()
        {
            _currentLifeTime = 0f;
            
            base.Release();
        }

        private void BounceVertically(Collision other)
        {
            ContactPoint contact = other.contacts[0];
            Vector3 normal = contact.normal;

            Vector3 verticalVelocity = new Vector3(0, _currentVelocity.y, _currentVelocity.z);
            Vector3 reflectedVertical = Vector3.Reflect(verticalVelocity, normal);

            float horizontalShift = Random.Range(-_horizontalDrift, _horizontalDrift);
            
            _currentHorizontalOffset += horizontalShift;
            _currentHorizontalOffset = Mathf.Clamp(_currentHorizontalOffset, -5f, 5f);

            Vector3 newVelocity = new Vector3(_currentHorizontalOffset, reflectedVertical.y, reflectedVertical.z);

            _currentVelocity = newVelocity.normalized * _speed;

            if (_currentVelocity != Vector3.zero)
                transform.forward = _currentVelocity.normalized;
        }
    }
}