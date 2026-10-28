#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Game.Scripts.Characters.Player;
using Game.Scripts.Levels;
using Game.Scripts.UI;
using Game.Scripts.UpgradeCard;
using Game.Scripts.Upgrades;
using TMPro;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Scripts.Editor
{
    public static class CampaignContentSetup
    {
        public static readonly string[] Compositions =
        {
            "2,0,0/4,0,0", "3,0,0/3,1,0/3,2,0", "4,0,0/4,1,0/4,2,0",
            "3,1,0/3,2,0/4,1,0/4,2,0", "3,2,0/3,3,0/2,4,0",
            "4,1,0/4,2,0/4,3,0", "4,1,0/3,2,0/3,2,1/0,3,2",
            "4,2,0/3,3,0/3,2,1/2,3,2", "4,2,0/4,3,0/3,3,1/3,3,2",
            "5,0,0/6,0,0/6,1,0/6,2,0/7,2,0", "3,3,0/3,3,1/3,3,2/2,4,2",
            "0,3,1/0,3,3", "4,2,0/4,3,0/4,3,1/4,3,2/5,3,2",
            "3,2,2/2,3,3/2,2,4", "4,1,0/4,2,0/3,3,1/4,3,1/4,3,2/3,4,2",
            "2,3,1/2,3,2/0,3,4", "4,2,1/4,3,1/4,3,2/5,3,2/4,4,2",
            "4,3,1/4,3,2/3,4,2/4,4,2", "3,3,2/3,3,3/2,4,3",
            "4,2,1/4,3,1/3,4,2/4,3,3/4,4,4"
        };
        private static readonly string[] Names = { "Первые выстрелы", "Проверка навыков", "Окраина", "Огненный след",
            "Тяжёлый отряд", "Городские ворота", "Тяжёлая поступь", "Гарнизон", "Возвращение", "Наплыв",
            "Опасные улицы", "Тяжёлый дозор", "Натиск", "Великаны", "Осада", "Ударный отряд",
            "Последний караван", "Контратака", "Перед штурмом", "Последний рубеж" };
        public static SceneNames SceneFor(int stage) => stage <= 2 ? SceneNames.Level_Tutorial :
            new[] { 3, 4, 5, 9, 10, 13, 14, 17, 19 }.Contains(stage) ? SceneNames.Level_Dust : SceneNames.Level_City;
        private static readonly string[] CardNames =
        {
            "Сильный выстрел|Strong shot|Güçlü atış", "Выносливость|Endurance|Dayanıklılık",
            "Веер стрел|Arrow fan|Ok yelpazesi", "Сила огня|Fire power|Ateş gücü",
            "Долгое пламя|Lasting flame|Kalıcı alev", "Быстрая перезарядка|Quick reload|Hızlı doldurma",
            "Ядовитый наконечник|Poison tip|Zehirli uç", "Сильный яд|Potent poison|Güçlü zehir",
            "Пробивание|Piercing|Delici ok", "Стойкий яд|Lasting poison|Kalıcı zehir",
            "Критический выстрел|Critical shot|Kritik atış", "Щит|Shield|Kalkan",
            "Лечение|Healing|İyileşme", "Монеты|Coins|Altın"
        };
        private static readonly string[] Descriptions =
        {
            "+10% базового урона обычной стрелы|+10% base arrow damage|+%10 temel ok hasarı",
            "+10% базового здоровья и столько же лечения|+10% base health; heal the added HP|+%10 temel can; eklenen canı iyileştir",
            "+1 стрела; урон каждой: 70% / 55% при 2 / 3 стрелах|+1 arrow; each deals 70% / 55% with 2 / 3 arrows|+1 ok; 2 / 3 okta her biri %70 / %55 hasar verir",
            "+10% базового урона огненной стрелы|+10% base fire arrow damage|+%10 temel ateş oku hasarı",
            "+15% базового времени жизни огненной стрелы|+15% base fire arrow lifetime|+%15 temel ateş oku ömrü",
            "−5% базовой перезарядки всех стрел|−5% base reload time for all arrows|Tüm okların temel doldurma süresi −%5",
            "+10% базового урона при попадании ядовитой стрелы|+10% base poison arrow impact damage|+%10 temel zehirli ok vuruş hasarı",
            "+20% базового периодического урона яда|+20% base poison damage over time|+%20 temel sürekli zehir hasarı",
            "+1 пробитая цель; урон следующим целям: 60%, 36%|+1 pierced target; following targets take 60%, 36%|+1 delinen hedef; sonraki hedefler %60, %36 hasar alır",
            "+20% базовой длительности яда|+20% base poison duration|+%20 temel zehir süresi",
            "+5% шанса критического попадания; урон ×1,5|+5% critical chance; damage ×1.5|+%5 kritik şansı; hasar ×1,5",
            "+1 блокируемое попадание за этап|Block +1 hit per stage|Bölüm başına +1 vuruş engelle",
            "Восстановить 25% максимального здоровья|Restore 25% maximum health|Azami canın %25 kadarını iyileştir",
            "+25 монет|+25 coins|+25 altın"
        };

        [MenuItem("Game/Campaign/Build 20 Stage Campaign")]
        public static void Build()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play mode first.");
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null || settings.FindGroup("Remote_Configs") == null) throw new InvalidOperationException("Configure Addressables first.");
            var cards = new List<CampaignUpgradeCard>();
            for (int i = 0; i < CardNames.Length; i++)
            {
                var kind = (CampaignUpgradeKind)i;
                string path = $"Assets/Game/Prefabs/UpgradCard/Campaign_{kind}.asset";
                var card = AssetDatabase.LoadAssetAtPath<CampaignUpgradeCard>(path);
                if (card == null) { card = ScriptableObject.CreateInstance<CampaignUpgradeCard>(); AssetDatabase.CreateAsset(card, path); }
                var so = new SerializedObject(card);
                so.FindProperty("_kind").enumValueIndex = i;
                var names = CardNames[i].Split('|');
                var descriptions = Descriptions[i].Split('|');
                string[] suffix = { "", "En", "Tr" };
                for (int language = 0; language < 3; language++)
                {
                    so.FindProperty("_cardName" + suffix[language]).stringValue = names[language];
                    so.FindProperty("_description" + suffix[language]).stringValue = descriptions[language];
                }
                string template = kind switch
                {
                    CampaignUpgradeKind.Health or CampaignUpgradeKind.Shield or CampaignUpgradeKind.Heal => "MaxHealthUpgradeCard",
                    CampaignUpgradeKind.FireDamage or CampaignUpgradeKind.FireLifetime => "FireArrowDamagetUpgradeCard",
                    CampaignUpgradeKind.PoisonImpact or CampaignUpgradeKind.PoisonDamage or CampaignUpgradeKind.PoisonDuration => "PoisonArrowDamagetUpgradeCard",
                    CampaignUpgradeKind.ArrowCount => "ArrowCountUpgradeCard", _ => "ArrowDamagetUpgradeCard"
                };
                var original = AssetDatabase.LoadAssetAtPath<Upgrades.UpgradeCard>($"Assets/Game/Prefabs/UpgradCard/{template}.asset");
                var source = new SerializedObject(original);
                so.CopyFromSerializedProperty(source.FindProperty("_cardIconReference"));
                so.CopyFromSerializedProperty(source.FindProperty("_cardColor"));
                so.ApplyModifiedPropertiesWithoutUndo();
                cards.Add(card);
                Register(path, $"Configs/Upgrades/Campaign_{kind}");
            }

            var levels = new List<LevelConfig>();
            for (int stage = 1; stage <= 20; stage++)
            {
                string path = $"Assets/Game/Configs/Levels/Level_{stage}.asset";
                var level = AssetDatabase.LoadAssetAtPath<LevelConfig>(path);
                if (level == null) { level = ScriptableObject.CreateInstance<LevelConfig>(); AssetDatabase.CreateAsset(level, path); }
                var so = new SerializedObject(level);
                so.FindProperty("_stageName").stringValue = Names[stage - 1];
                so.FindProperty("_sceneNames").intValue = (int)SceneFor(stage);
                var waveConfig = so.FindProperty("_waveConfig");
                waveConfig.FindPropertyRelative("_timeBetweemWaves").floatValue = 3;
                var waves = waveConfig.FindPropertyRelative("_waves");
                var compositions = Compositions[stage - 1].Split('/');
                waves.arraySize = compositions.Length;
                for (int w = 0; w < compositions.Length; w++)
                {
                    var counts = compositions[w].Split(',').Select(int.Parse).ToArray();
                    var wave = waves.GetArrayElementAtIndex(w);
                    wave.FindPropertyRelative("Goblins").intValue = counts[0];
                    wave.FindPropertyRelative("Orcs").intValue = counts[1];
                    wave.FindPropertyRelative("Trolls").intValue = counts[2];
                    wave.FindPropertyRelative("MaxAlive").intValue = stage <= 2 ? 3 : stage <= 6 ? 4 : stage <= 12 ? 6 : 8;
                    wave.FindPropertyRelative("SpawnInterval").floatValue = stage <= 2 ? 1.2f : stage <= 6 ? 1f : stage <= 12 ? .8f : .7f;
                    wave.FindPropertyRelative("TrollLast").boolValue = stage == 7 && w == 2;
                    wave.FindPropertyRelative("HealAfter").floatValue = stage == 15 && w == 2 ? .25f : 0;
                }
                var races = so.FindProperty("_enemyRaceConfigs");
                races.arraySize = 3;
                string[] prefabs = { "EnemyGoblin", "EnemyOrc", "EnemyTroll" };
                for (int race = 0; race < 3; race++)
                {
                    var item = races.GetArrayElementAtIndex(race);
                    item.FindPropertyRelative("_race").intValue = race;
                    item.FindPropertyRelative("_poolSize").intValue = 8;
                    item.FindPropertyRelative("_spawnWeight").floatValue = 1;
                    item.FindPropertyRelative("_prefabReference").FindPropertyRelative("m_AssetGUID").stringValue = AssetDatabase.AssetPathToGUID($"Assets/Game/Prefabs/Enemys/{prefabs[race]}.prefab");
                }
                var available = so.FindProperty("_availableUpgrades");
                available.arraySize = cards.Count;
                for (int i = 0; i < cards.Count; i++) available.GetArrayElementAtIndex(i).objectReferenceValue = cards[i];
                so.ApplyModifiedPropertiesWithoutUndo();
                levels.Add(level);
                Register(path, $"Configs/Levels/Level_{stage}");
            }
            var catalog = new SerializedObject(AssetDatabase.LoadAssetAtPath<LevelCatalog>("Assets/Game/Configs/Levels/LevelCatalog.asset"));
            var entries = catalog.FindProperty("_levels");
            entries.arraySize = 20;
            for (int i = 0; i < 20; i++)
                entries.GetArrayElementAtIndex(i).FindPropertyRelative("m_AssetGUID").stringValue = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(levels[i]));
            catalog.ApplyModifiedPropertiesWithoutUndo();
            var buildScenes = EditorBuildSettings.scenes.Where(x => AssetDatabase.LoadAssetAtPath<SceneAsset>(x.path) != null).ToList();
            foreach (string name in new[] { "Boot", "Level_Tutorial", "Level_Dust", "Level_City" })
            {
                string path = $"Assets/Game/Scenes/{name}.unity";
                int index = buildScenes.FindIndex(x => x.path == path);
                if (index < 0) buildScenes.Add(new EditorBuildSettingsScene(path, true));
                else buildScenes[index].enabled = true;
            }
            EditorBuildSettings.scenes = buildScenes.ToArray();
            ConfigurePlayer();
            AssetDatabase.SaveAssets();
            Debug.Log("[Campaign] Built 20 stages and 14 cards (12 upgrades + 2 fallback rewards).");
        }

        private static void Register(string path, string address)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var entry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(path), settings.FindGroup("Remote_Configs"));
            entry.address = address;
            entry.SetLabel("Remote_Configs", true, true);
            EditorUtility.SetDirty(settings);
        }

        private static void ConfigurePlayer()
        {
            const string path = "Assets/Game/Prefabs/GG.prefab";
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                if (root.GetComponent<CampaignUpgrades>() == null) root.AddComponent<CampaignUpgrades>();
                var shoot = new SerializedObject(root.GetComponent<PlayerShoot>());
                var settings = shoot.FindProperty("_bulletShootSettings");
                settings.GetArrayElementAtIndex(0).FindPropertyRelative("_shootDelay").floatValue = 1.2f;
                shoot.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        [MenuItem("Game/Campaign/Update Campaign UI")]
        public static void UpdateSceneUI()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play mode first.");
            foreach (string name in new[] { "Level_Tutorial", "Level_Dust", "Level_City" })
            {
                string path = $"Assets/Game/Scenes/{name}.unity";
                var scene = SceneManager.GetSceneByPath(path);
                bool wasLoaded = scene.isLoaded;
                if (!wasLoaded) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                try
                {
                    var panel = scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<UpgradeChoicePanel>(true)).Single();
                    var so = new SerializedObject(panel);
                    var button = so.FindProperty("_rerollButton");
                    if (button.objectReferenceValue == null)
                    {
                        var skip = (UnityEngine.UI.Button)so.FindProperty("_skipButton").objectReferenceValue;
                        if (skip == null) throw new InvalidOperationException("Missing upgrade skip button in " + name);
                        var reroll = UnityEngine.Object.Instantiate(skip, skip.transform.parent);
                        reroll.name = "CampaignReroll";
                        reroll.onClick = new UnityEngine.UI.Button.ButtonClickedEvent();
                        var rect = (RectTransform)reroll.transform;
                        rect.anchoredPosition += new Vector2(0, 85);
                        var label = reroll.GetComponentInChildren<TextMeshProUGUI>(true);
                        if (label != null) label.text = "Переброс (1)";
                        button.objectReferenceValue = reroll;
                        so.ApplyModifiedPropertiesWithoutUndo();
                    }
                    var ui = scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<WaveUI>(true)).Single();
                    var wave = new SerializedObject(ui);
                    var text = (TextMeshProUGUI)wave.FindProperty("_waveNumberText").objectReferenceValue;
                    text.enableAutoSizing = true;
                    text.fontSizeMin = 12;
                    text.fontSizeMax = 26;
                    text.enableWordWrapping = false;
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                }
                finally { if (!wasLoaded) EditorSceneManager.CloseScene(scene, true); }
            }
        }
    }
}
#endif
