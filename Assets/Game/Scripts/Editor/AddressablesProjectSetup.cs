#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace Game.Scripts.Editor.AddressablesSetup
{
    public static class AddressablesProjectSetup
    {
        private const string RemoteLoadPath =
            "https://storage.yandexcloud.net/BUCKET_NAME/[BuildTarget]";
        private const string RemoteBuildPath = "ServerData/[BuildTarget]";

        private static readonly (string group, string label, bool remote)[] Groups =
        {
            ("Remote_Configs", "Remote_Configs", true),
            ("Remote_Characters", "Remote_Characters", true),
            ("Remote_Combat", "Remote_Combat", true),
            ("Remote_World", "Remote_World", true),
            ("Remote_FX", "Remote_FX", true),
            ("Remote_Icons", "Remote_Icons", true),
            ("Remote_Audio", "Remote_Audio", true),
            ("Remote_Smoke", "Remote_Smoke", true),
        };

        [MenuItem("Game/Addressables/Setup Groups And Mark Assets")]
        public static void Setup()
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            ConfigureRemoteProfile(settings);

            Dictionary<string, AddressableAssetGroup> groups = new();
            foreach ((string groupName, string label, bool remote) in Groups)
            {
                AddressableAssetGroup group = GetOrCreateGroup(settings, groupName, remote);
                EnsureLabel(settings, label);
                groups[groupName] = group;
            }

            Mark(settings, groups["Remote_Smoke"], "Assets/Game/Configs/Addressables/SmokeTest.txt",
                "Smoke/TestAsset", "Remote_Smoke");

            for (int i = 1; i <= 20; i++)
            {
                Mark(settings, groups["Remote_Configs"],
                    $"Assets/Game/Configs/Levels/Level_{i}.asset",
                    $"Configs/Levels/Level_{i}", "Remote_Configs");
            }

            foreach (string path in Directory.GetFiles("Assets/Game/Prefabs/UpgradCard", "*.asset"))
            {
                string file = path.Replace('\\', '/');
                string name = Path.GetFileNameWithoutExtension(file);
                Mark(settings, groups["Remote_Configs"], file, $"Configs/Upgrades/{name}", "Remote_Configs");
            }

            Mark(settings, groups["Remote_Characters"], "Assets/Game/Prefabs/GG.prefab",
                "Prefabs/Player/GG", "Remote_Characters");
            Mark(settings, groups["Remote_Characters"], "Assets/Game/Prefabs/Enemys/EnemyGoblin.prefab",
                "Prefabs/Enemies/Goblin", "Remote_Characters");
            Mark(settings, groups["Remote_Characters"], "Assets/Game/Prefabs/Enemys/EnemyOrc.prefab",
                "Prefabs/Enemies/Orc", "Remote_Characters");
            Mark(settings, groups["Remote_Characters"], "Assets/Game/Prefabs/Enemys/EnemyTroll.prefab",
                "Prefabs/Enemies/Troll", "Remote_Characters");
            Mark(settings, groups["Remote_Characters"], "Assets/Game/Configs/EXP/PlayerLevelXpConfig.asset",
                "Configs/EXP/PlayerLevelXpConfig", "Remote_Characters");

            string[] combatPrefabs =
            {
                "Assets/Game/Prefabs/Bullets/Arrow.prefab",
                "Assets/Game/Prefabs/Bullets/FireArrow.prefab",
                "Assets/Game/Prefabs/Bullets/PoisonBullet.prefab",
                "Assets/Game/Prefabs/Bullets/GoblinBullet.prefab",
                "Assets/Game/Prefabs/Bullets/OrcBullet.prefab",
                "Assets/Game/Prefabs/Bullets/TrollBullet.prefab",
                "Assets/Game/Prefabs/PickUpObject/PickHealth.prefab",
            };

            foreach (string path in combatPrefabs)
            {
                string name = Path.GetFileNameWithoutExtension(path);
                Mark(settings, groups["Remote_Combat"], path, $"Prefabs/Combat/{name}", "Remote_Combat");
            }

            Mark(settings, groups["Remote_World"], "Assets/Game/Prefabs/Grounds/Section_1_Ground.prefab",
                "Prefabs/World/Section_1_Ground", "Remote_World");
            Mark(settings, groups["Remote_World"], "Assets/Game/Prefabs/Grounds/Section_2_Ground.prefab",
                "Prefabs/World/Section_2_Ground", "Remote_World");
            Mark(settings, groups["Remote_World"], "Assets/Game/Prefabs/Grounds/Section_3_Ground.prefab",
                "Prefabs/World/Section_3_Ground", "Remote_World");

            Mark(settings, groups["Remote_FX"], "Assets/Game/Prefabs/Effects/GenericDeath.prefab",
                "Prefabs/FX/GenericDeath", "Remote_FX");
            Mark(settings, groups["Remote_FX"], "Assets/Game/Prefabs/Effects/HealOnceBurst.prefab",
                "Prefabs/FX/HealOnceBurst", "Remote_FX");

            Mark(settings, groups["Remote_Icons"], "Assets/Game/Art/IMG_0603.PNG",
                "UI/Icons/UpgradeSheet", "Remote_Icons");

            MarkShopContent(settings, groups["Remote_Configs"], groups["Remote_Icons"]);
            CharacterContentSetup.RegisterAddressables();

            MarkClip(settings, groups["Remote_Audio"], "Assets/Game/Sounds/bgmusic.mp3", "Audio/BGM");
            MarkClip(settings, groups["Remote_Audio"], "Assets/Game/Sounds/Button.mp3", "Audio/ButtonClick");
            MarkClip(settings, groups["Remote_Audio"], "Assets/Game/Sounds/Heal.mp3", "Audio/Heal");
            MarkClip(settings, groups["Remote_Audio"], "Assets/Game/Sounds/Upgrade.mp3", "Audio/Upgrade");

            MarkEtfxAudio(settings, groups["Remote_Audio"]);

            SetAssetDatabasePlayMode();

            settings.SetDirty(AddressableAssetSettings.ModificationEvent.BatchModification, null, true, true);
            AssetDatabase.SaveAssets();
            Debug.Log("[Addressables] Groups configured. Play Mode = Use Asset Database. " +
                      "Replace <BUCKET_NAME> in Remote.LoadPath, then Build > New Build for WebGL player.");
        }

        [MenuItem("Game/Addressables/Use Asset Database Play Mode")]
        public static void SetAssetDatabasePlayMode()
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(true);

            for (int i = 0; i < settings.DataBuilders.Count; i++)
            {
                ScriptableObject builder = settings.DataBuilders[i];
                if (builder == null)
                    continue;

                // BuildScriptFastMode = "Use Asset Database (fastest)"
                if (builder.GetType().Name.IndexOf("FastMode", StringComparison.Ordinal) < 0)
                    continue;

                settings.ActivePlayModeDataBuilderIndex = i;
                EditorUtility.SetDirty(settings);
                Debug.Log($"[Addressables] Play Mode Script → {builder.name} (Asset Database). " +
                          "Do not use 'Use Existing Build' until you have built Addressables content.");
                return;
            }

            Debug.LogWarning("[Addressables] FastMode / Asset Database builder not found.");
        }

        private static void ConfigureRemoteProfile(AddressableAssetSettings settings)
        {
            settings.BuildRemoteCatalog = true;
            settings.OverridePlayerVersion = "";

            AddressableAssetProfileSettings profile = settings.profileSettings;
            string profileId = settings.activeProfileId;

            EnsureProfileVar(profile, profileId, "Remote.BuildPath", RemoteBuildPath);
            EnsureProfileVar(profile, profileId, "Remote.LoadPath", RemoteLoadPath);
            // Angle brackets in the old placeholder are invalid in Windows build paths.
            string loadPath = profile.GetValueByName(profileId, "Remote.LoadPath");
            if (loadPath.Contains("<BUCKET_NAME>"))
                profile.SetValue(profileId, "Remote.LoadPath", loadPath.Replace("<BUCKET_NAME>", "BUCKET_NAME"));
            settings.RemoteCatalogBuildPath.SetVariableByName(settings, "Remote.BuildPath");
            settings.RemoteCatalogLoadPath.SetVariableByName(settings, "Remote.LoadPath");
        }

        private static void EnsureProfileVar(AddressableAssetProfileSettings profile, string profileId,
            string name, string value)
        {
            if (profile.GetVariableNames().Contains(name) == false)
                profile.CreateValue(name, value);

            if (string.IsNullOrEmpty(profile.GetValueByName(profileId, name)))
                profile.SetValue(profileId, name, value);
        }

        private static AddressableAssetGroup GetOrCreateGroup(AddressableAssetSettings settings, string name,
            bool remote)
        {
            AddressableAssetGroup group = settings.FindGroup(name);
            if (group == null)
            {
                group = settings.CreateGroup(name, false, false, true, null,
                    typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
            }

            BundledAssetGroupSchema bundled = group.GetSchema<BundledAssetGroupSchema>();
            if (bundled == null)
                bundled = group.AddSchema<BundledAssetGroupSchema>();

            bundled.BuildPath.SetVariableByName(settings, remote ? "Remote.BuildPath" : "Local.BuildPath");
            bundled.LoadPath.SetVariableByName(settings, remote ? "Remote.LoadPath" : "Local.LoadPath");
            bundled.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;
            bundled.UseAssetBundleCrc = false;
            bundled.UseAssetBundleCrcForCachedBundles = false;
            bundled.IncludeInBuild = true;

            ContentUpdateGroupSchema update = group.GetSchema<ContentUpdateGroupSchema>();
            if (update == null)
                update = group.AddSchema<ContentUpdateGroupSchema>();
            update.StaticContent = false;

            return group;
        }

        private static void EnsureLabel(AddressableAssetSettings settings, string label)
        {
            if (settings.GetLabels().Contains(label) == false)
                settings.AddLabel(label);
        }

        private static void Mark(AddressableAssetSettings settings, AddressableAssetGroup group, string assetPath,
            string address, string label)
        {
            if (File.Exists(assetPath) == false)
            {
                Debug.LogWarning($"[Addressables] Missing asset: {assetPath}");
                return;
            }

            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group, false, false);
            entry.SetAddress(address);
            entry.SetLabel(label, true, true, false);
        }

        private static void MarkShopContent(AddressableAssetSettings settings, AddressableAssetGroup configs,
            AddressableAssetGroup icons)
        {
            const string shopConfigsDir = "Assets/Game/Configs/Shop";
            if (Directory.Exists(shopConfigsDir))
            {
                foreach (string path in Directory.GetFiles(shopConfigsDir, "*.asset"))
                {
                    string file = path.Replace('\\', '/');
                    string name = Path.GetFileNameWithoutExtension(file);
                    Mark(settings, configs, file, $"Configs/Shop/{name}", "Remote_Configs");
                }
            }

            const string shopIconsDir = "Assets/Game/Art/Shop";
            if (Directory.Exists(shopIconsDir) == false)
                return;

            foreach (string path in Directory.GetFiles(shopIconsDir, "*.png"))
            {
                string file = path.Replace('\\', '/');
                string name = Path.GetFileNameWithoutExtension(file);
                Mark(settings, icons, file, $"UI/Icons/Shop/{name}", "Remote_Icons");
            }
        }

        private static void MarkClip(AddressableAssetSettings settings, AddressableAssetGroup group, string path,
            string address)
        {
            Mark(settings, group, path, address, "Remote_Audio");
        }

        private static void MarkEtfxAudio(AddressableAssetSettings settings, AddressableAssetGroup group)
        {
            string[] candidates =
            {
                "Assets/Epic Toon FX/Sound/etfx_spawn.wav",
                "Assets/Epic Toon FX/Sound/etfx_explosion_sharp.wav",
                "Assets/Epic Toon FX/Sound/etfx_explosion_slime.wav",
            };

            foreach (string path in candidates)
            {
                if (File.Exists(path))
                    Mark(settings, group, path, $"Audio/{Path.GetFileNameWithoutExtension(path)}", "Remote_Audio");
            }
        }
    }
}
#endif
