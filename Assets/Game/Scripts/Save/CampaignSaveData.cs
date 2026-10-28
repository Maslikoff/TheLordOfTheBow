using System;
using System.Collections.Generic;
using Game.Scripts.Characters.Player;
using Game.Scripts.Shop;
using Game.Scripts.Upgrades;
using UnityEngine;

namespace Game.Scripts.Save
{
    [Serializable]
    public class PlayerProgressSnapshot
    {
        public int Level = 1;
        public float Experience;
        public float MaxHealth = 100;
        public float Health = 100;
        public int Coins;
        public int ShieldCharges;
        public int Rerolls;
        public int UnlockedStage = 1;
        public List<UpgradeRank> Ranks = new();
        public List<BulletUpgradeData> Bullets = new();
        public List<BulletUpgradeData> LegacyBonuses = new();

        public PlayerProgressSnapshot Clone() => JsonUtility.FromJson<PlayerProgressSnapshot>(JsonUtility.ToJson(this));
        public static PlayerProgressSnapshot Capture(Player player, ICurrencyService currency)
        {
            var state = new PlayerProgressSnapshot
            {
                Level = player.Experience.CurrentLevel, Experience = player.Experience.CurrentExperience,
                MaxHealth = player.PlayerHealth.MaxCount, Health = player.PlayerHealth.CurrentCount,
                Coins = currency.Coins, ShieldCharges = player.Campaign.ShieldCharges,
                Rerolls = player.Campaign.Rerolls, UnlockedStage = player.Campaign.UnlockedStage,
                Ranks = player.Campaign.CaptureRanks()
            };
            foreach (var pair in player.BulletUpgrades.CaptureAllStates())
                state.Bullets.Add(new BulletUpgradeData { BulletType = pair.Key,
                    IsUnlocked = pair.Value.IsUnlocked, DamageBonus = pair.Value.DamageBonus,
                    LifeTimeBonus = pair.Value.LifeTimeBonus, CountBonus = pair.Value.CountBonus,
                    PoisonDamageBonus = pair.Value.PoisonDamageBonus });
            return state;
        }

        public void Apply(Player player, ICurrencyService currency, bool restoreHealth = true)
        {
            player.Experience.RestoreTo(Level, Experience);
            foreach (var bullet in Bullets)
                player.BulletUpgrades.Get(bullet.BulletType)?.ApplySaveState(new BulletUpgradeState(
                    bullet.IsUnlocked, bullet.DamageBonus, bullet.LifeTimeBonus, bullet.CountBonus, bullet.PoisonDamageBonus));
            player.BulletUpgrades.NotifyLoadedFromSave();
            player.PlayerHealth.Restore(MaxHealth, restoreHealth ? Health : player.PlayerHealth.CurrentCount);
            player.Campaign.Restore(Ranks, ShieldCharges, Rerolls, UnlockedStage);
            currency.Load(Coins);
        }
    }

    [Serializable]
    public class CampaignSaveData
    {
        public const int CurrentVersion = 1;
        public int Version = CurrentVersion;
        public int StageIndex;
        public int Cycle;
        public int UnlockedStage = 1;
        public PlayerProgressSnapshot Committed;
        public PlayerProgressSnapshot Entry;
        public PlayerProgressSnapshot Checkpoint;
        public int CheckpointStage = -1;
        public int CheckpointCycle = -1;
        public int WaveIndex;
        public bool HasCheckpoint(int stage, int cycle) => Checkpoint != null && CheckpointStage == stage && CheckpointCycle == cycle;
        public void ClearCheckpoint()
        {
            Entry = null;
            Checkpoint = null;
            CheckpointStage = CheckpointCycle = -1;
            WaveIndex = 0;
        }
    }
}
