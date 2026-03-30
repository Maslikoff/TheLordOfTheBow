using UnityEngine;

namespace Game.Scripts.Characters.Bullets
{
    public class PoisonArrow : Bullet
    {
        [SerializeField] private float _lifeTime = 10f;
        [SerializeField] private float _verticalDrift = 1.5f;
        [SerializeField] private float _randomAngleRange = 30f;

        private float _currentLifeTime;
        private Vector3 _currentVelocity;
        private float _currentVerticalOffset;

        private void OnEnable()
        {
            _currentLifeTime = 0f;
            _currentVelocity = Vector3.zero;
            _currentVerticalOffset = 0f;

            _destroyOnCollision = false;
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            _currentLifeTime += Time.fixedDeltaTime;

            if (_currentLifeTime >= _lifeTime)
            {
                Release();
                return;
            }

            MoveBullet();
        }

        protected override void MoveBullet()
        {
            if (_currentVelocity == Vector3.zero)
            {
                Vector3 startDirection = transform.forward;
                startDirection.y = 0;
                
                float randomYAngle = Random.Range(-_randomAngleRange, _randomAngleRange);
                Quaternion randomRotation = Quaternion.Euler(0, randomYAngle, 0);
                startDirection = randomRotation * startDirection;
                
                _currentVelocity = startDirection.normalized * _speed;
            }

            _rigidbody.velocity = _currentVelocity;
        }

        protected override void HandleCollision(Collision other)
        {
            if (other.gameObject.TryGetComponent(out Enemy.Enemy enemy))
                if (enemy.TryGetComponent(out Health health))
                    health.TakeDamage(_damage);

            BounceHorizontally(other);
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
            _currentVerticalOffset = 0f;
            
            base.Release();
        }
        
        public void UpgradeLifeTime(float additionalTime)
        {
            _lifeTime += additionalTime;
            Debug.Log($"Poison arrow life time increased to {_lifeTime} seconds");
        }
        
        public void UpgradeDamage(float additionalDamage)
        {
            _damage += additionalDamage;
            Debug.Log($"Poison arrow damage increased to {_damage} damage");
        }
        
        private void BounceHorizontally(Collision other)
        {
            ContactPoint contact = other.contacts[0];
            Vector3 normal = contact.normal;
            Vector3 horizontalVelocity = new Vector3(_currentVelocity.x, 0, 0);
            Vector3 reflectedHorizontal = Vector3.Reflect(horizontalVelocity, normal);
            
            float verticalShift = Random.Range(-_verticalDrift, _verticalDrift);
            
            _currentVerticalOffset += verticalShift;
            _currentVerticalOffset = Mathf.Clamp(_currentVerticalOffset, -5f, 5f);
            
            Vector3 newVelocity = new Vector3(reflectedHorizontal.x, _currentVerticalOffset, reflectedHorizontal.z);
            
            _currentVelocity = newVelocity.normalized * _speed;
            
            if (_currentVelocity != Vector3.zero)
                transform.forward = _currentVelocity.normalized;
        }
    }
}