using System;
using System.Collections.Generic;
using Game.Scripts.Characters.Bullets;
using Game.Scripts.Save;
using UniRx;
using UnityEngine;

namespace Game.Scripts.Upgrades
{
    public class PlayerBulletUpgradeCollection : MonoBehaviour
    {
        private readonly Dictionary<BulletType, PlayerBulletUpgradeEntry> _entriesByType = new();

        [SerializeField] private List<PlayerBulletUpgradeEntry> _entries = new();

        public event Action<BulletType> BulletUnlocked;
        public event Action UpgradesLoaded;
        
        private void Awake()
        {
            _entriesByType.Clear();

            foreach (PlayerBulletUpgradeEntry entry in _entries)
            {
                if (_entriesByType.ContainsKey(entry.BulletType))
                    continue;

                entry.CaptureDefaults();
                _entriesByType.Add(entry.BulletType, entry);
            }
        }

        public PlayerBulletUpgradeEntry Get(BulletType bulletType) =>
            _entriesByType.GetValueOrDefault(bulletType);

        public bool IsUnlocked(BulletType bulletType) =>
            Get(bulletType)?.IsUnlocked ?? false;

        public void NotifyLoadedFromSave()
        {
            foreach (PlayerBulletUpgradeEntry entry in _entries)
            {
                if (entry != null && entry.IsUnlocked)
                    BulletUnlocked?.Invoke(entry.BulletType);
            }
            
            UpgradesLoaded?.Invoke();
        }

        public void Unlock(BulletType bulletType)
        {
            PlayerBulletUpgradeEntry entry = Get(bulletType);
            if (entry == null)
                return;
            
            entry.Unlock();
            
            BulletUnlocked?.Invoke(bulletType);
            
            MessageBroker.Default.Publish(new M_SaveRequested());
        }

        public void AddDamage(BulletType bulletType, float damage)
        {
            PlayerBulletUpgradeEntry entry = Get(bulletType);
            if (entry == null)
                return;
            
            entry.AddDamage(damage);
            
            MessageBroker.Default.Publish(new M_SaveRequested());
        }
        
        public void AddLifeTime(BulletType bulletType, float lifeTime)
        {
            PlayerBulletUpgradeEntry entry = Get(bulletType);
            if (entry == null)
                return;
            
            entry.AddLifeTime(lifeTime);
            
            MessageBroker.Default.Publish(new M_SaveRequested());
        }
        
        public void AddCount(BulletType bulletType, int value)
        {
            PlayerBulletUpgradeEntry entry = Get(bulletType);
            if (entry == null)
                return;
            
            entry.AddCount(value);
            
            MessageBroker.Default.Publish(new M_SaveRequested());
        }
    }
}