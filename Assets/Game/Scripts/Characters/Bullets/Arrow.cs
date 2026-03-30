using UnityEngine;

namespace Game.Scripts.Characters.Bullets
{
    public class Arrow : Bullet
    {
        [SerializeField] private int _currentArrowCount = 1;
        [SerializeField] private int _maxArrowCount = 3;
        [SerializeField] private float _spreadAngle = 45f;
        
        private float[] _arrowAngles = { 0f };
        
        private void Awake()
        {
            _destroyOnCollision = true;
        }
        
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
        
        public void UpgradeArrowCount()
        {
            if (_currentArrowCount < _maxArrowCount)
            {
                _currentArrowCount++;
                UpdateArrowAngles();
                Debug.Log($"Arrow count upgraded to {_currentArrowCount}");
            }
        }
        
        public void UpgradeDamage(float additionalDamage)
        {
            _damage += additionalDamage;
            Debug.Log($"Arrow damage increased to {_damage} damage");
        }
        
        private void UpdateArrowAngles()
        {
            switch (_currentArrowCount)
            {
                case 1:
                    _arrowAngles = new float[] { 0f };
                    break;
                
                case 2:
                    _arrowAngles = new float[] { -_spreadAngle / 2f, _spreadAngle / 2f };
                    break;
                
                case 3:
                    _arrowAngles = new float[] { -_spreadAngle, 0f, _spreadAngle };
                    break;
            }
        }
    }
}