using System;
using System.Collections.Generic;
using Game.Scripts.Characters.Bullets;
using Game.Scripts.Characters.Player;
using Game.Scripts.Upgrades;
using YG;

namespace Game.Scripts.Save
{
    public class PlayerProgressService : IPlayerProgressService
    {
        private readonly Dictionary<BulletType, BulletUpgradeState> _bulletStates = new();
        
        private int _playerLevel = 1;
        private float _playerExperience;
        private bool _hasSessionProgress;

        public bool HasSessionProgress => _hasSessionProgress;

        public void CaptureFrom(Player player)
        {
            if (player == null)
                return;

            _playerLevel = player.Experience.CurrentLevel;
            _playerExperience = player.Experience.CurrentExperience;
            _bulletStates.Clear();

            PlayerBulletUpgradeCollection bulletCollection = player.BulletUpgrades;

            foreach (BulletType bulletType in Enum.GetValues(typeof(BulletType)))
            {
                PlayerBulletUpgradeEntry entry = bulletCollection.Get(bulletType);
                if (entry == null)
                    continue;

                _bulletStates[bulletType] = new BulletUpgradeState(
                    entry.IsUnlocked,
                    entry.DamageBonus,
                    entry.LifeTimeBonus,
                    entry.CountBonus);
            }

            _hasSessionProgress = true;
        }

        public void ApplyTo(Player player)
        {
            if (player == null || _hasSessionProgress == false)
                return;

            player.Experience.LoadSaveData(_playerLevel, _playerExperience);

            PlayerBulletUpgradeCollection bulletCollection = player.BulletUpgrades;

            foreach (BulletType bulletType in Enum.GetValues(typeof(BulletType)))
            {
                PlayerBulletUpgradeEntry entry = bulletCollection.Get(bulletType);
                if (entry == null)
                    continue;

                BulletUpgradeState state = _bulletStates.GetValueOrDefault(
                    bulletType,
                    new BulletUpgradeState(false, 0, 0, 0));

                entry.ApplySaveState(state);
            }

            bulletCollection.NotifyLoadedFromSave();
        }

        public void SyncToSaves(SavesYG saves)
        {
            if (saves == null || _hasSessionProgress == false)
                return;

            saves.WritePlayerLevel(_playerLevel);
            saves.WritePlayerExperience(_playerExperience);

            foreach (var pair in _bulletStates)
                saves.WriteBulletUpgradeState(pair.Key, pair.Value);
        }

        public void LoadFromSaves(SavesYG saves)
        {
            if (saves == null)
                return;

            _playerLevel = saves.PlayerLevel;
            _playerExperience = saves.PlayerExperience;
            _bulletStates.Clear();

            foreach (BulletType bulletType in Enum.GetValues(typeof(BulletType)))
            {
                _bulletStates[bulletType] = saves.GetBulletUpgradeState(bulletType);
            }

            _hasSessionProgress = true;
        }
    }
}
