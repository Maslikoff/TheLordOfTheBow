using UnityEngine;

namespace Game.Scripts.Characters.Bullets
{
    public class EnemyBullet : Bullet
    {
        [SerializeField] private float _spawnForwardOffset = 1.2f;
        [SerializeField] private float _minVisibleTravelDistance = 0.35f;
        
        private Renderer[] _visualRenderers;
        private Collider _bulletCollider;
        private Collider[] _ownerColliders;
        private Vector3 _visualSpawnPosition;
        private bool _visualsVisible;
        
        private void Awake()
        {
            _visualRenderers = GetComponentsInChildren<Renderer>(true);
            _bulletCollider = GetComponent<Collider>();
        }
        
        public override void Initialize(BulletData bulletData)
        {
            base.Initialize(bulletData);
            
            if (_owner != null)
            {
                EnemyBulletTracker.Register(_owner, this);
                IgnoreOwnerCollisions();
            }
            
            if (_direction != Vector3.zero)
                transform.position += _direction * _spawnForwardOffset;
            
            _visualSpawnPosition = transform.position;
            _visualsVisible = false;
            SetVisualsVisible(false);
            ApplyMovement();
        }
        
        protected override void MoveBullet()
        {
            ApplyMovement();
            UpdateVisuals();
        }

        private void ApplyMovement()
        {
            if (_rigidbody != null && _direction != Vector3.zero)
                _rigidbody.velocity = _direction * _speed;
        }
        
        private void UpdateVisuals()
        {
            if (_visualsVisible)
                return;
            
            float traveled = Vector3.Distance(transform.position, _visualSpawnPosition);
            if (traveled < _minVisibleTravelDistance)
                return;
            
            _visualsVisible = true;
            SetVisualsVisible(true);
        }
        
        private void SetVisualsVisible(bool visible)
        {
            if (_visualRenderers == null)
                return;
            
            foreach (Renderer renderer in _visualRenderers)
            {
                if (renderer != null)
                    renderer.enabled = visible;
            }
        }
        
        private void IgnoreOwnerCollisions()
        {
            if (_bulletCollider == null || _owner == null)
                return;
            
            _ownerColliders = _owner.GetComponentsInChildren<Collider>();
            
            foreach (Collider ownerCollider in _ownerColliders)
            {
                if (ownerCollider != null && ownerCollider.enabled)
                    Physics.IgnoreCollision(_bulletCollider, ownerCollider, true);
            }
        }
        
        private void RestoreOwnerCollisions()
        {
            if (_bulletCollider == null || _ownerColliders == null)
                return;
            
            foreach (Collider ownerCollider in _ownerColliders)
            {
                if (ownerCollider != null)
                    Physics.IgnoreCollision(_bulletCollider, ownerCollider, false);
            }
            
            _ownerColliders = null;
        }

        protected override void HandleCollision(Collision other)
        {
            if (other.gameObject.TryGetComponent(out Health player) == false) 
                return;
            
            player.TakeDamage(_baseDamage);
        }

        protected override bool CanCollide(Collision other) => 
            other.gameObject.TryGetComponent(out Enemy.Enemy _) == false && base.CanCollide(other);
        
        public override void Release()
        {
            SetVisualsVisible(false);
            _visualsVisible = false;
            RestoreOwnerCollisions();
            
            if (_owner != null)
                EnemyBulletTracker.Unregister(_owner, this);
            
            base.Release();
        }
    }
}
