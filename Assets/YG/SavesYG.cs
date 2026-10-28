using System.Collections.Generic;
using Game.Scripts.Characters.Bullets;
using Game.Scripts.Save;
using Game.Scripts.Upgrades;
using UnityEngine;

namespace YG
{
    public partial class SavesYG
    {
        public CampaignSaveData Campaign;

        public CampaignSaveData EnsureCampaign()
        {
            if (Campaign != null && Campaign.Version >= CampaignSaveData.CurrentVersion) return Campaign;
            var state = new PlayerProgressSnapshot
            {
                Level = PlayerLevel, Experience = PlayerExperience, Coins = Coins,
                UnlockedStage = Mathf.Clamp(CurrentLevelIndex + 1, 1, 20)
            };
            foreach (var b in _bulletUpgrades)
            {
                var copy = new BulletUpgradeData { BulletType = b.BulletType, IsUnlocked = b.IsUnlocked,
                    DamageBonus = b.DamageBonus, LifeTimeBonus = b.LifeTimeBonus, CountBonus = b.CountBonus,
                    PoisonDamageBonus = b.PoisonDamageBonus };
                state.Bullets.Add(copy);
                state.LegacyBonuses.Add(copy);
            }
            Campaign = new CampaignSaveData { StageIndex = Mathf.Clamp(CurrentLevelIndex, 0, 19),
                Cycle = 0, UnlockedStage = state.UnlockedStage, Committed = state };
            _waveCheckpoints.Clear();
            return Campaign;
        }
        [SerializeField] private int _playerLevel = 1;
        [SerializeField] private float _playerExperience = 0f;
        [SerializeField] private int _currentLevelIndex = 0;
        [SerializeField] private List<BulletUpgradeData> _bulletUpgrades = new();
        [SerializeField] private List<LevelWaveCheckpoint> _waveCheckpoints = new();
        [SerializeField] private int _coins;
        [SerializeField] private List<string> _ownedSkins = new();
        [SerializeField] private List<string> _ownedProjectiles = new();
        [SerializeField] private List<string> _ownedHats = new();
        [SerializeField] private string _equippedSkin = string.Empty;
        [SerializeField] private string _equippedProjectile = string.Empty;
        [SerializeField] private string _equippedHat = string.Empty;

        public int PlayerLevel 
        { 
            get => _playerLevel; 
            private set => _playerLevel = value >= 1 ? value : 1; 
        }

        public int Coins
        {
            get => _coins;
            private set => _coins = value >= 0 ? value : 0;
        }

        public IReadOnlyList<string> OwnedSkins => _ownedSkins;
        public IReadOnlyList<string> OwnedProjectiles => _ownedProjectiles;
        public IReadOnlyList<string> OwnedHats => _ownedHats;
        public string EquippedSkin => _equippedSkin ?? string.Empty;
        public string EquippedProjectile => _equippedProjectile ?? string.Empty;
        public string EquippedHat => _equippedHat ?? string.Empty;
        
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
                existing.PoisonDamageBonus = state.PoisonDamageBonus;
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
                    PoisonDamageBonus = state.PoisonDamageBonus,
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
                    existing.CountBonus,
                    existing.PoisonDamageBonus
                );
            }
            
            return new BulletUpgradeState(false, 0, 0, 0, 0);
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
                    existing.CountBonus,
                    existing.PoisonDamageBonus);
                return true;
            }

            state = default;
            return false;
        }
        
        public void WriteWaveCheckpoint(int levelIndex, int waveIndex)
        {
            levelIndex = Mathf.Max(0, levelIndex);
            waveIndex = Mathf.Max(0, waveIndex);

            LevelWaveCheckpoint existing = _waveCheckpoints.Find(x => x.LevelIndex == levelIndex);
            
            if (existing != null)
            {
                existing.WaveIndex = waveIndex;
                return;
            }

            _waveCheckpoints.Add(new LevelWaveCheckpoint
            {
                LevelIndex = levelIndex,
                WaveIndex = waveIndex
            });
        }

        public bool TryGetWaveCheckpoint(int levelIndex, out int waveIndex)
        {
            levelIndex = Mathf.Max(0, levelIndex);

            LevelWaveCheckpoint existing = _waveCheckpoints.Find(x => x.LevelIndex == levelIndex);
            
            if (existing != null)
            {
                waveIndex = Mathf.Max(0, existing.WaveIndex);
                return true;
            }

            waveIndex = 0;
            return false;
        }

        public void ClearWaveCheckpoint(int levelIndex)
        {
            _waveCheckpoints.RemoveAll(x => x.LevelIndex == Mathf.Max(0, levelIndex));
        }

        public void WriteCoins(int coins)
        {
            Coins = coins;
        }

        public void WriteCosmeticInventory(
            List<string> ownedSkins,
            List<string> ownedProjectiles,
            List<string> ownedHats,
            string equippedSkin,
            string equippedProjectile,
            string equippedHat)
        {
            _ownedSkins = ownedSkins ?? new List<string>();
            _ownedProjectiles = ownedProjectiles ?? new List<string>();
            _ownedHats = ownedHats ?? new List<string>();
            _equippedSkin = equippedSkin ?? string.Empty;
            _equippedProjectile = equippedProjectile ?? string.Empty;
            _equippedHat = equippedHat ?? string.Empty;
        }
    }
}
