using System.Collections.Generic;
using Game.Scripts.Characters.Bullets;
using Game.Scripts.Upgrades;
using UnityEngine;

namespace Game.Scripts.Levels
{
    public class LevelSessionService
    {
        private readonly Dictionary<BulletType, BulletUpgradeState> _snapshotUpgrades = new();
        private readonly Dictionary<BulletType, BulletUpgradeState> _preDeathUpgrades = new();

        public int SnapshotLevel { get; private set; }
        public float SnapshotExperience { get; private set; }
        public bool HasSnapshot { get; private set; }

        public int PreDeathLevel { get; private set; }
        public float PreDeathExperience { get; private set; }
        public bool HasPreDeathState { get; private set; }

        public bool HasUpgradeSnapshot { get; private set; }
        public bool HasPreDeathUpgrades { get; private set; }

        public void CaptureSnapshot(Experience.Experience experience, PlayerBulletUpgradeCollection upgrades = null)
        {
            if (experience != null)
            {
                SnapshotLevel = experience.CurrentLevel;
                SnapshotExperience = experience.CurrentExperience;
                HasSnapshot = true;
            }

            CaptureUpgradeSnapshot(upgrades);
            ClearPreDeathState();

            Debug.Log($"[LevelSession] Снимок входа: Lvl {SnapshotLevel}, XP {SnapshotExperience}");
        }

        public void CaptureUpgradeSnapshot(PlayerBulletUpgradeCollection upgrades)
        {
            if (upgrades == null)
                return;

            _snapshotUpgrades.Clear();
            foreach (KeyValuePair<BulletType, BulletUpgradeState> pair in upgrades.CaptureAllStates())
                _snapshotUpgrades[pair.Key] = pair.Value;

            HasUpgradeSnapshot = _snapshotUpgrades.Count > 0;
        }

        public void CapturePreDeathState(Experience.Experience experience, PlayerBulletUpgradeCollection upgrades = null)
        {
            if (experience != null)
            {
                PreDeathLevel = experience.CurrentLevel;
                PreDeathExperience = experience.CurrentExperience;
                HasPreDeathState = true;
            }

            CapturePreDeathUpgrades(upgrades);

            Debug.Log($"[LevelSession] XP до смерти: Lvl {PreDeathLevel}, XP {PreDeathExperience}");
        }

        public void CapturePreDeathUpgrades(PlayerBulletUpgradeCollection upgrades)
        {
            if (upgrades == null)
                return;

            _preDeathUpgrades.Clear();
            foreach (KeyValuePair<BulletType, BulletUpgradeState> pair in upgrades.CaptureAllStates())
                _preDeathUpgrades[pair.Key] = pair.Value;

            HasPreDeathUpgrades = _preDeathUpgrades.Count > 0;
        }

        public void RollbackExperience(Experience.Experience experience)
        {
            if (experience == null || HasSnapshot == false)
                return;

            experience.LoadSaveData(SnapshotLevel, SnapshotExperience);

            Debug.Log($"[LevelSession] Откат к входу: Lvl {SnapshotLevel}, XP {SnapshotExperience}");
        }

        public void RollbackUpgrades(PlayerBulletUpgradeCollection upgrades)
        {
            if (upgrades == null || HasUpgradeSnapshot == false)
                return;

            upgrades.ApplyAllStates(_snapshotUpgrades);

            Debug.Log("[LevelSession] Откат прокачки к входу на уровень");
        }

        public void RestorePreDeathExperience(Experience.Experience experience)
        {
            if (experience == null || HasPreDeathState == false)
                return;

            experience.LoadSaveData(PreDeathLevel, PreDeathExperience);

            Debug.Log($"[LevelSession] Восстановлен XP до смерти: Lvl {PreDeathLevel}, XP {PreDeathExperience}");
        }

        public void RestorePreDeathUpgrades(PlayerBulletUpgradeCollection upgrades)
        {
            if (upgrades == null || HasPreDeathUpgrades == false)
                return;

            upgrades.ApplyAllStates(_preDeathUpgrades);

            Debug.Log("[LevelSession] Восстановлена прокачка до смерти");
        }

        public void ClearPreDeathState()
        {
            HasPreDeathState = false;
            PreDeathLevel = 0;
            PreDeathExperience = 0f;
            ClearPreDeathUpgrades();
        }

        public void ClearPreDeathUpgrades()
        {
            HasPreDeathUpgrades = false;
            _preDeathUpgrades.Clear();
        }
    }
}
