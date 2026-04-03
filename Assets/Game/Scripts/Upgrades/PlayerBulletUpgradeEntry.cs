using System;
using Game.Scripts.Characters.Bullets;
using UnityEngine;

namespace Game.Scripts.Upgrades
{
    [Serializable]
    public class PlayerBulletUpgradeEntry
    {
        [SerializeField] private BulletType _bulletType;
        [SerializeField] private bool _isUnlocked;
        [SerializeField] private float _damage = 1f;
        [SerializeField] private float _lifeTime = 10f;
        [SerializeField] private int _count = 1;
        [SerializeField] private int _maxCount = 3;

        public BulletType BulletType => _bulletType;
        public bool IsUnlocked => _isUnlocked;
        public float Damage => _damage;
        public float LifeTime => _lifeTime;
        public int Count => _count;

        public void Unlock()
        {
            _isUnlocked = true;
            Debug.Log($"Unlocked {_bulletType}");
        }
        
        public void AddDamage(float damage)
        {
            _damage += damage;
        }
        
        public void AddLifeTime(float lifeTime)
        {
            _lifeTime += lifeTime;
        }

        public void AddCount(int count)
        {
            _count = Mathf.Clamp(_count + count, 1, _maxCount);
        }
    }
}