using UnityEngine;
using System.Collections.Generic;
using Game.Scripts.Upgrades;

namespace Game.Scripts.Characters.Bullets
{
    public class Arrow : Bullet
    {
        private readonly HashSet<int> _hitEnemies = new();
        private readonly List<Collider> _ignored = new();
        private Collider _collider;
        private int _pierce;
        public int TargetsHit => _hitEnemies.Count;
        public override void Initialize(BulletData data)
        {
            ResetHits();
            base.Initialize(data);
            _pierce = data.Pierce;
            if (data.Critical) _currentDamage *= 1.5f;
            _destroyOnCollision = true;
            _collider = GetComponent<Collider>();
        }
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
            var enemy = other.gameObject.GetComponentInParent<Enemy.Enemy>();
            _destroyOnCollision = true;
            if (enemy == null) return;
            int index = _hitEnemies.Count;
            if (!_hitEnemies.Add(enemy.GetInstanceID()))
            {
                _destroyOnCollision = false;
                return;
            }
            // Ignore before damage: a lethal hit may return the enemy to its pool immediately.
            if (_collider != null)
                foreach (var collider in enemy.GetComponentsInChildren<Collider>())
                {
                    Physics.IgnoreCollision(_collider, collider, true);
                    _ignored.Add(collider);
                }
            _destroyOnCollision = index >= _pierce;
            enemy.GetComponent<Health>()?.TakeDamage(_currentDamage * CampaignUpgrades.PierceMultiplier(index));
        }

        private void ResetHits()
        {
            if (_collider != null)
                foreach (var collider in _ignored)
                    if (collider != null) Physics.IgnoreCollision(_collider, collider, false);
            _ignored.Clear();
            _hitEnemies.Clear();
        }

        public override void Release()
        {
            ResetHits();
            base.Release();
        }
        
        protected override bool CanCollide(Collision other)
        {
            if (other.gameObject.TryGetComponent(out Player.Player _))
                return false;
                
            return base.CanCollide(other);
        }
    }
}
