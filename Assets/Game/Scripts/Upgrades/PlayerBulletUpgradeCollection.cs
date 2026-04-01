using System;
using System.Collections.Generic;
using Game.Scripts.Characters.Bullets;
using UnityEngine;

namespace Game.Scripts.Upgrades
{
    public class PlayerBulletUpgradeCollection : MonoBehaviour
    {
        private readonly Dictionary<BulletType, PlayerBulletUpgradeEntry> _entriesByType = new();

        [SerializeField] private List<PlayerBulletUpgradeEntry> _entries = new();

        private void Awake()
        {
            _entriesByType.Clear();

            foreach (PlayerBulletUpgradeEntry entry in _entries)
            {
                if (_entriesByType.ContainsKey(entry.BulletType))
                    continue;

                _entriesByType.Add(entry.BulletType, entry);
            }
        }

        public PlayerBulletUpgradeEntry Get(BulletType bulletType) =>
            _entriesByType.GetValueOrDefault(bulletType);

        public bool IsUnlocked(BulletType bulletType) =>
            Get(bulletType).IsUnlocked;

        public void Unlock(BulletType bulletType)
        {
            PlayerBulletUpgradeEntry entry = Get(bulletType);
            entry.Unlock();
        }

        public void AddDamage(BulletType bulletType, float damage)
        {
            PlayerBulletUpgradeEntry entry = Get(bulletType);
            entry.AddDamage(damage);
        }
        
        public void AddLifeTime(BulletType bulletType, float lifeTime)
        {
            PlayerBulletUpgradeEntry entry = Get(bulletType);
            entry.AddLifeTime(lifeTime);
        }
        
        public void AddCount(BulletType bulletType, int value)
        {
            PlayerBulletUpgradeEntry entry = Get(bulletType);
            entry.AddCount(value);
        }
    }
}