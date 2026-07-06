using System.Collections.Generic;
using Game.Scripts.Characters.Bullets;
using Game.Scripts.Upgrades;
using UnityEngine;

namespace YG
{
    public partial class SavesYG
    {
        [SerializeField] private int _playerLevel = 1;
        [SerializeField] private float _playerExperience = 0f;
        [SerializeField] private int _currentLevelIndex = 0;
        [SerializeField] private List<BulletUpgradeData> _bulletUpgrades = new List<BulletUpgradeData>();

        public int PlayerLevel 
        { 
            get => _playerLevel; 
            private set => _playerLevel = value >= 1 ? value : 1; 
        }
        
        public float PlayerExperience 
        { 
            get => _playerExperience; 
            private set => _playerExperience = value >= 0 ? value : 0; 
        }
        
        public int CurrentLevelIndex 
        { 
            get => _currentLevelIndex; 
            private set => _currentLevelIndex = value >= 0 ? value : 0; 
        }

        public IReadOnlyList<BulletUpgradeData> BulletUpgrades => _bulletUpgrades;

        public void WritePlayerLevel(int level)
        {
            PlayerLevel = level;
        }

        public void WritePlayerExperience(float experience)
        {
            PlayerExperience = experience;
        }

        public void WriteCurrentLevelIndex(int levelIndex)
        {
            CurrentLevelIndex = levelIndex;
        }

        public void WriteBulletUpgradeState(BulletType bulletType, BulletUpgradeState state)
        {
            var existing = _bulletUpgrades.Find(b => b.BulletType == bulletType);
            
            if (existing != null)
            {
                existing.IsUnlocked = state.IsUnlocked;
                existing.DamageBonus = state.DamageBonus;
                existing.LifeTimeBonus = state.LifeTimeBonus;
                existing.CountBonus = state.CountBonus;
            }
            else
            {
                _bulletUpgrades.Add(new BulletUpgradeData
                {
                    BulletType = bulletType,
                    IsUnlocked = state.IsUnlocked,
                    DamageBonus = state.DamageBonus,
                    LifeTimeBonus = state.LifeTimeBonus,
                    CountBonus = state.CountBonus
                });
            }
        }

        public BulletUpgradeState GetBulletUpgradeState(BulletType bulletType)
        {
            var existing = _bulletUpgrades.Find(b => b.BulletType == bulletType);
            
            if (existing != null)
            {
                return new BulletUpgradeState(
                    existing.IsUnlocked,
                    existing.DamageBonus,
                    existing.LifeTimeBonus,
                    existing.CountBonus
                );
            }
            
            return new BulletUpgradeState(false, 0, 0, 0);
        }
        
        public bool TryGetBulletUpgradeState(BulletType bulletType, out BulletUpgradeState state)
        {
            var existing = _bulletUpgrades.Find(b => b.BulletType == bulletType);

            if (existing != null)
            {
                state = new BulletUpgradeState(
                    existing.IsUnlocked,
                    existing.DamageBonus,
                    existing.LifeTimeBonus,
                    existing.CountBonus);
                return true;
            }

            state = default;
            return false;
        }
    }
}