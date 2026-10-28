#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Game.Scripts.Characters;
using Game.Scripts.Characters.Bullets;
using Game.Scripts.Characters.Player;
using Game.Scripts.Editor;
using Game.Scripts.Levels;
using Game.Scripts.Save;
using Game.Scripts.Shop;
using Game.Scripts.UpgradeCard;
using Game.Scripts.Upgrades;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Tests.Editor
{
    // Menu-driven checks match the project's Assembly-CSharp setup; no runtime test dependencies.
    public static class CampaignIntegrationChecks
    {
        private static int _assertions;
        private static void Check(bool value, string message)
        {
            if (!value) throw new InvalidOperationException(message);
            _assertions++;
        }
        private static void Near(float actual, float expected, string message) => Check(Mathf.Abs(actual - expected) < .001f, $"{message}: {actual} != {expected}");
        private static void Awake(object component) => component.GetType().GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(component, null);

        [MenuItem("Game/Campaign/Run Integration Checks")]
        public static void RunMenu() => Debug.Log(Run());
        public static string Run()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Run checks in Edit mode.");
            _assertions = 0;
            CheckContent();
            CheckMigration();
            CheckRules();
            string path = "Assets/Game/Prefabs/GG.prefab";
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var player = root.GetComponent<Player>();
                Awake(player.PlayerHealth);
                Awake(player.BulletUpgrades);
                Awake(player.Experience);
                Awake(player.Campaign);
                CheckUpgrades(player);
                CheckSnapshots(player);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            string result = $"PASS: {_assertions} campaign assertions (content, progression, upgrades, snapshots, migration).";
            System.IO.File.WriteAllText("Library/CampaignChecks.txt", result);
            return result;
        }

        private static void CheckContent()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>("Assets/Game/Configs/Levels/LevelCatalog.asset");
            Check(catalog.Count == 20, "Catalog count");
            var entries = new SerializedObject(catalog).FindProperty("_levels");
            for (int stage = 1; stage <= 20; stage++)
            {
                string path = $"Assets/Game/Configs/Levels/Level_{stage}.asset";
                var level = AssetDatabase.LoadAssetAtPath<LevelConfig>(path);
                Check(level != null, $"Stage {stage} exists");
                Check(entries.GetArrayElementAtIndex(stage - 1).FindPropertyRelative("m_AssetGUID").stringValue == AssetDatabase.AssetPathToGUID(path), "Catalog order");
                Check(level.SceneNames == CampaignContentSetup.SceneFor(stage), "Scene order");
                Check(EditorBuildSettings.scenes.Any(x => x.enabled && x.path == $"Assets/Game/Scenes/{level.SceneNames}.unity"), "Scene included in build");
                var expected = CampaignContentSetup.Compositions[stage - 1].Split('/');
                Check(level.WaveConfig.Waves.Count == expected.Length, "Wave count");
                for (int w = 0; w < expected.Length; w++)
                {
                    var wave = level.WaveConfig.Waves[w];
                    Check(wave.IsValid, "Valid wave");
                    Check($"{wave.Goblins},{wave.Orcs},{wave.Trolls}" == expected[w], "Exact wave composition");
                    var order = wave.CreateSpawnOrder();
                    Check(order.Count == wave.Total, "Full spawn order");
                    Check(order.Count(x => (int)x == 0) == wave.Goblins && order.Count(x => (int)x == 1) == wave.Orcs && order.Count(x => (int)x == 2) == wave.Trolls, "No random race substitutions");
                    if (stage == 7 && w == 2) Check((int)order.Last() == 2, "First troll is last");
                }
                Check(level.AvailableUpgrades.Count == 14, "12 upgrades and 2 fallback cards");
                Check(level.AvailableUpgrades.OfType<CampaignUpgradeCard>().Select(x => x.Id).Distinct().Count() == 14, "Unique card IDs");
                var settings = AddressableAssetSettingsDefaultObject.Settings;
                Check(settings.FindAssetEntry(AssetDatabase.AssetPathToGUID(path)) != null, "Addressable stage");
                foreach (var card in level.AvailableUpgrades)
                    Check(settings.FindAssetEntry(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(card))) != null, "Addressable card");
                foreach (var race in level.EnemyRaceConfigs)
                    Check(!string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(race.PrefabReference.AssetGUID)), "Valid enemy prefab");
            }
            var siege = AssetDatabase.LoadAssetAtPath<LevelConfig>("Assets/Game/Configs/Levels/Level_15.asset");
            Near(siege.WaveConfig.Waves[2].HealAfter, .25f, "Siege healing");
        }

        private static void CheckMigration()
        {
            var saves = new SaveData();
            saves.WritePlayerLevel(11);
            saves.WritePlayerExperience(123);
            saves.WriteCurrentLevelIndex(8);
            saves.WriteCoins(87);
            saves.WriteBulletUpgradeState(BulletType.FireArrow, new BulletUpgradeState(true, 20, 12));
            saves.WriteWaveCheckpoint(8, 2);
            var migrated = saves.EnsureCampaign();
            Check(migrated.StageIndex == 8 && migrated.Cycle == 0, "Migration position");
            Check(migrated.Committed.Level == 11 && migrated.Committed.Experience == 123 && migrated.Committed.Coins == 87, "Migration player state");
            Check(migrated.Committed.Bullets[0].LifeTimeBonus == 12, "Legacy bonuses not clamped");
            Check(migrated.Committed.LegacyBonuses.Count == 1, "Legacy baseline retained");
            Check(!saves.TryGetWaveCheckpoint(8, out _), "Old wave checkpoints cleared");
            Check(ReferenceEquals(migrated, saves.EnsureCampaign()), "Migration is idempotent");
            var roundtrip = JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(saves));
            Check(roundtrip.Campaign.Committed.Level == 11, "Save JSON roundtrip");
        }

        private static void CheckRules()
        {
            Check(CampaignRules.Next(19, 0) == (2, 1), "20 to 3");
            Check(CampaignRules.Next(19, 4) == (2, 5), "Repeated cycles");
            Check(CampaignRules.Score(2, 1) == 21, "Monotonic score");
            Near(CampaignRules.EnemyHealth(2), 1.7f, "HP scaling");
            Near(CampaignRules.EnemyDamage(2), 1.3f, "Damage scaling");
            Near(CampaignRules.Experience(2), 1.5f, "XP scaling");
            Near(CampaignUpgrades.ArrowMultiplier(2), .7f, "Two arrow damage");
            Near(CampaignUpgrades.ArrowMultiplier(3), .55f, "Three arrow damage");
            Near(CampaignUpgrades.PierceMultiplier(2), .36f, "Third target damage");
        }

        private static void CheckUpgrades(Player player)
        {
            var upgrades = player.Campaign;
            upgrades.BeginStage(1, true);
            Check(!upgrades.CanApply(CampaignUpgradeKind.FireDamage), "Fire locked at stage 1");
            upgrades.BeginStage(4, true);
            Check(player.BulletUpgrades.IsUnlocked(BulletType.FireArrow), "Automatic fire unlock");
            Check(!player.BulletUpgrades.IsUnlocked(BulletType.PoisonArrow), "Poison still locked");
            upgrades.BeginStage(7, true);
            Check(player.BulletUpgrades.IsUnlocked(BulletType.PoisonArrow), "Automatic poison unlock");
            var arrow = player.BulletUpgrades.Get(BulletType.Arrow);
            float damage = arrow.Damage;
            upgrades.Apply(CampaignUpgradeKind.ArrowDamage);
            upgrades.Apply(CampaignUpgradeKind.ArrowDamage);
            Near(arrow.Damage, damage + .2f * arrow.BaseDamage, "Additive base damage");
            Check(upgrades.Apply(CampaignUpgradeKind.ArrowCount), "First fan rank");
            Check(!upgrades.Apply(CampaignUpgradeKind.ArrowCount), "Second fan rank locked");
            for (int i = 0; i < 4; i++) Check(upgrades.Apply(CampaignUpgradeKind.Reload), "Reload rank");
            Near(upgrades.ReloadMultiplier, .8f, "Reload cap");
            Check(!upgrades.Apply(CampaignUpgradeKind.Reload), "Reload max filtered");
            upgrades.BeginStage(13, true);
            Check(upgrades.Apply(CampaignUpgradeKind.Shield), "First shield rank");
            Check(!upgrades.Apply(CampaignUpgradeKind.Shield), "Second shield gated");
            float hp = player.PlayerHealth.CurrentCount;
            player.PlayerHealth.TakeDamage(10);
            Near(player.PlayerHealth.CurrentCount, hp, "Shield blocks hit");
            Check(upgrades.ShieldCharges == 0, "Shield charge consumed");
            player.PlayerHealth.TakeDamage(10);
            Near(player.PlayerHealth.CurrentCount, hp - 10, "No free shield recharge");
            upgrades.BeginStage(18, true);
            Check(upgrades.Apply(CampaignUpgradeKind.ArrowCount) && arrow.Count == 3, "Second fan rank unlocked");
            Check(upgrades.Apply(CampaignUpgradeKind.Pierce) && upgrades.Apply(CampaignUpgradeKind.Pierce), "Piercing ranks");
            Check(!upgrades.Apply(CampaignUpgradeKind.Pierce), "Piercing cap");
            for (int i = 0; i < 4; i++) upgrades.Apply(CampaignUpgradeKind.Critical);
            Near(upgrades.CriticalChance, .2f, "Critical chance cap");
            Check(upgrades.Rerolls == 2 && upgrades.SpendReroll() && upgrades.Rerolls == 1, "Reroll charge");
            var fire = player.BulletUpgrades.Get(BulletType.FireArrow);
            fire.AddLifeTime(12);
            Check(!upgrades.CanApply(CampaignUpgradeKind.FireLifetime), "Oversized legacy lifetime filtered");
            Near(fire.LifeTimeBonus, 12, "Oversized legacy value preserved");
        }

        private static void CheckSnapshots(Player player)
        {
            var currency = new CurrencyService();
            currency.Load(100);
            player.Campaign.Apply(CampaignUpgradeKind.Health);
            var snapshot = PlayerProgressSnapshot.Capture(player, currency);
            var clone = snapshot.Clone();
            player.Campaign.Apply(CampaignUpgradeKind.Health);
            player.Campaign.Apply(CampaignUpgradeKind.ArrowDamage);
            player.Campaign.SpendReroll();
            currency.Add(25);
            snapshot.Apply(player, currency);
            Near(player.PlayerHealth.MaxCount, clone.MaxHealth, "Health rollback");
            Near(player.PlayerHealth.CurrentCount, clone.Health, "Current health rollback");
            Check(player.Campaign.Rerolls == clone.Rerolls && currency.Coins == 100, "Rerolls/coins rollback");
            Check(JsonUtility.ToJson(snapshot) == JsonUtility.ToJson(clone), "Immutable snapshot");
            var campaign = new CampaignSaveData { Checkpoint = snapshot, CheckpointStage = 2, CheckpointCycle = 1 };
            Check(campaign.HasCheckpoint(2, 1) && !campaign.HasCheckpoint(2, 0), "Checkpoint cycle isolation");
            campaign.ClearCheckpoint();
            Check(!campaign.HasCheckpoint(2, 1), "Checkpoint clear");
        }
    }
}
#endif
