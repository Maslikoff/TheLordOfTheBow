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
        [SerializeField] private float _poisonDamage;
        [SerializeField] private int _count = 1;
        [SerializeField] private int _maxCount = 3;
        
        private float _defaultDamage;
        private float _defaultLifeTime;
        private float _defaultPoisonDamage;
        private int _defaultCount;
        private bool _defaultsCaptured;

        public BulletType BulletType => _bulletType;
        public bool IsUnlocked => _isUnlocked;
        public float Damage => _damage;
        public float LifeTime => _lifeTime;
        public float PoisonDamage => _poisonDamage;
        public int Count => _count;
        public float BaseDamage => _defaultDamage;
        public float BaseLifeTime => _defaultLifeTime;
        public float BasePoisonDamage => _defaultPoisonDamage;
        
        public float DamageBonus => _damage - _defaultDamage;
        public float LifeTimeBonus => _lifeTime - _defaultLifeTime;
        public float PoisonDamageBonus => _poisonDamage - _defaultPoisonDamage;
        public int CountBonus => _count - _defaultCount;

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

        public void AddPoisonDamage(float poisonDamage)
        {
            _poisonDamage += poisonDamage;
        }

        public void AddCount(int count)
        {
            _count = Mathf.Clamp(_count + count, 1, _maxCount);
        }
        
        public void ApplySaveState(BulletUpgradeState state)
        {
            _isUnlocked = state.IsUnlocked;
            _damage = _defaultDamage + state.DamageBonus;
            _lifeTime = _defaultLifeTime + state.LifeTimeBonus;
            _poisonDamage = _defaultPoisonDamage + state.PoisonDamageBonus;
            _count = Mathf.Clamp(_defaultCount + state.CountBonus, 1, _maxCount);
        }
        
        public void CaptureDefaults()
        {
            if (_defaultsCaptured)
                return;
            
            _defaultDamage = _damage;
            _defaultLifeTime = _lifeTime;
            _defaultPoisonDamage = _poisonDamage;
            _defaultCount = _count;
            _defaultsCaptured = true;
        }
    }
}
