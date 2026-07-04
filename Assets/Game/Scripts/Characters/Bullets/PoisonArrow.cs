using UnityEngine;

namespace Game.Scripts.Characters.Bullets
{
    public class PoisonArrow : Bullet
    {
        [SerializeField] private float _defaultLifeTime = 10f;
        [SerializeField] private float _bounceAngleVariation = 15f;
        [SerializeField] private float _bounceForceMultiplier = 1.1f;
        
        private float _maxLifeTime;
        private float _currentLifeTime;
        private Vector3 _currentVelocity;
        private bool _isInitialized;

        private void OnEnable()
        {
            _currentLifeTime = 0f;
            _currentVelocity = Vector3.zero;
            _isInitialized = false;

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
            
            Vector3 startDirection = _direction != Vector3.zero ? _direction : transform.forward;
            
            float randomAngle = Random.Range(-_bounceAngleVariation * 0.3f, _bounceAngleVariation * 0.3f);
            Quaternion randomRotation = Quaternion.Euler(randomAngle, 0, 0);
            startDirection = (randomRotation * startDirection).normalized ;
            
            _currentVelocity = startDirection* _speed;
            _isInitialized = true;
            
            if (_currentVelocity != Vector3.zero)
                transform.forward = _currentVelocity.normalized;
        }

        protected override void MoveBullet()
        {
            if (_isInitialized == false || _currentVelocity == Vector3.zero)
            {
                Vector3 fallback = _direction != Vector3.zero ? _direction : transform.forward;
                _currentVelocity = fallback.normalized * _speed;
                _isInitialized = true;
            }

            _rigidbody.velocity = _currentVelocity;
            
            if (_currentVelocity != Vector3.zero)
                transform.forward = _currentVelocity.normalized;
        }

        protected override void HandleCollision(Collision other)
        {
            if (other.gameObject.TryGetComponent(out Enemy.Enemy enemy))
                if (enemy.TryGetComponent(out Health health))
                    health.TakeDamage(_currentDamage);

            BounceLikePinball(other);
        }

        /*private void OnCollisionEnter(Collision collision)
        {
            if (CanCollide(collision) == false)
                return;

            HandleCollision(collision);
        }*/

        protected override bool CanCollide(Collision other)
        {
            if (other.gameObject.TryGetComponent(out Player.Player _))
                return false;

            return base.CanCollide(other);
        }

        public override void Release()
        {
            _currentLifeTime = 0f;
            _currentVelocity = Vector3.zero;
            _isInitialized = false;
            
            base.Release();
        }

        private void BounceLikePinball(Collision other)
        {
            ContactPoint contact = other.contacts[0];
            Vector3 normal = contact.normal;
            
            Vector3 penetrationDirection = normal;
            float penetrationDepth = 0.1f;
            
            transform.position += penetrationDirection * penetrationDepth;
            
            Vector3 reflectedVelocity = Vector3.Reflect(_currentVelocity, normal);
            
            float randomAngle = Random.Range(-_bounceAngleVariation, _bounceAngleVariation);
            Quaternion randomRotation = Quaternion.Euler(0, randomAngle * 0.2f, 0);
            reflectedVelocity = randomRotation * reflectedVelocity;
            
            _currentVelocity = reflectedVelocity.normalized * (_speed * _bounceForceMultiplier);
            
            _rigidbody.velocity = _currentVelocity;
            
            if (_currentVelocity != Vector3.zero)
                transform.forward = _currentVelocity.normalized;
        }
    }
}