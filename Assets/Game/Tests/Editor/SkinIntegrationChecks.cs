#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Scripts.AddressablesContent;
using Game.Scripts.Characters;
using Game.Scripts.Characters.Player;
using Game.Scripts.Editor;
using Game.Scripts.Shop;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;
using UnityEngine.AddressableAssets;
using YG;
using Object = UnityEngine.Object;

namespace Game.Tests.Editor
{
    // Menu-driven integration checks because the game's scripts live in Assembly-CSharp.
    // Uses isolated saves in Edit Mode and a fake inventory in Play Mode; never purchases on the user's account.
    public static class SkinIntegrationChecks
    {
        private const string AsyncRequestPath = "Library/SkinAsyncChecks.request";
        private const string AsyncResultPath = "Library/SkinAsyncChecks.result.txt";
        private static bool _asyncRunnerStarted;

        [InitializeOnLoadMethod]
        private static void ResumeRequestedAsyncChecks()
        {
            EditorApplication.delayCall += TryRunRequestedAsyncChecks;
        }

        [MenuItem("Tools/Shop/Validate Async Skin Switching")]
        public static void RequestAsyncChecks()
        {
            File.WriteAllText(AsyncRequestPath, DateTime.UtcNow.ToString("O"));
            TryRunRequestedAsyncChecks();
        }

        private static void TryRunRequestedAsyncChecks()
        {
            if (_asyncRunnerStarted || !File.Exists(AsyncRequestPath))
                return;

            if (!EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = true;
                return;
            }

            _asyncRunnerStarted = true;
            File.Delete(AsyncRequestPath);
            RunRequestedAsyncChecks().Forget();
        }

        private static async UniTaskVoid RunRequestedAsyncChecks()
        {
            try
            {
                await RunAsyncChecks();
                File.WriteAllText(AsyncResultPath, AsyncResult);
            }
            catch (Exception exception)
            {
                AsyncResult = "FAIL: " + exception;
                File.WriteAllText(AsyncResultPath, AsyncResult);
                Debug.LogException(exception);
            }
            finally
            {
                _asyncRunnerStarted = false;
                EditorApplication.isPlaying = false;
            }
        }

        public static string AsyncResult { get; private set; } = "Not run";
        public static string ActualPrefabResult { get; private set; } = "Not run";

        private static readonly string[] CharacterMaterialPaths =
        {
            "Assets/Game/Art/Characters/Enemies/Goblin/Goblin.mat",
            "Assets/Game/Art/Characters/Enemies/Orc/Orc.mat",
            "Assets/Game/Art/Characters/Enemies/Troll/Troll.mat",
            "Assets/Game/Art/Characters/Elf/Elf.mat",
            "Assets/Game/Art/Characters/ElfFemale/ElfFemale.mat",
            "Assets/Game/Art/Characters/Knight/Knight.mat",
            "Assets/Game/Art/Characters/Wizard/Wizard.mat",
            "Assets/Game/Art/Characters/Dwarf/Dwarf.mat",
            "Assets/Game/Art/Characters/Ninja/Ninja.mat",
            "Assets/Game/Art/Characters/Valkyrie/Valkyrie.mat"
        };

        private static void Check(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

        [MenuItem("Tools/Shop/Validate Skin Inventory And Assets")]
        public static void InventoryAndAssets()
        {
            Check(!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode,
                "Run in Edit Mode.");
            var sdkField = typeof(YG2).GetField("_SDKEnabled",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            Check(sdkField != null, "YG2 SDK state field");
            bool originalSdkEnabled = (bool)sdkField.GetValue(null);
            var original = YG2.saves;
            YG2.saves = new SavesYG();
            // Enter Play Mode can leave this static set when domain reload is disabled.
            // Keep test persistence local even when the menu is run afterwards.
            sdkField.SetValue(null, false);
            try
            {
                var catalog = AssetDatabase.LoadAssetAtPath<ShopCatalog>("Assets/Game/Configs/Shop/ShopCatalog.asset");
                var ninja = catalog.FindById("skin_stub_01");
                var elf = catalog.Items.Single(i => i.IsDefault && i.Category == CosmeticCategory.Skin);
                var elfFemale = catalog.FindById("skin_elf_female");
                var knight = catalog.FindById("skin_knight");
                var wizard = catalog.FindById("skin_wizard");
                var dwarf = catalog.FindById("skin_dwarf");
                var valkyrie = catalog.FindById("skin_valkyrie");
                Check(catalog.Items.Count == 7 && catalog.Items.All(i => i.Category == CosmeticCategory.Skin), "Skin-only catalog");
                Check(elf.Id == "skin_default_elf" && elf.CoinPrice == 0 && !elf.IsIap, "Free default Elf");
                Check(elfFemale != null && elfFemale.CoinPrice == 25 && !elfFemale.IsIap, "Elf female price");
                Check(knight != null && knight.CoinPrice == 110 && !knight.IsIap, "Knight price");
                Check(wizard != null && wizard.CoinPrice == 110 && !wizard.IsIap, "Wizard price");
                Check(dwarf != null && dwarf.CoinPrice == 110 && !dwarf.IsIap, "Dwarf price");
                Check(ninja != null && ninja.IsIap && ninja.IapProductId == "skin_ninja", "Ninja IAP/legacy id");
                Check(valkyrie != null && valkyrie.IsIap && valkyrie.IapProductId == "skin_valkyrie", "Valkyrie IAP");

                var currency = new CurrencyService();
                var inventory = new CosmeticInventoryService(currency);
                var provider = new ShopCatalogProvider();
                provider.SetCatalog(catalog);
                var payments = new TestPayments();
                var shop = new ShopService(provider, inventory, payments);
                shop.Initialize();

                currency.Load(24);
                shop.Purchase(elfFemale);
                Check(currency.Coins == 24 && !inventory.Owns(elfFemale.Id), "24 coins must not buy Elf female");
                currency.Load(25);
                shop.Purchase(elfFemale);
                Check(currency.Coins == 0 && inventory.Owns(elfFemale.Id) && inventory.GetEquippedId(CosmeticCategory.Skin) == elfFemale.Id, "25 coins buys and equips Elf female");
                shop.Purchase(elfFemale);
                Check(currency.Coins == 0 && inventory.GetOwnedIds(CosmeticCategory.Skin).Count == 1, "Repeat coin purchase");

                currency.Load(109);
                shop.Purchase(knight);
                Check(currency.Coins == 109 && !inventory.Owns(knight.Id), "109 coins must not buy Knight");
                currency.Load(110);
                shop.Purchase(knight);
                Check(currency.Coins == 0 && inventory.Owns(knight.Id), "110 coins buys Knight");

                currency.Load(777);
                shop.Purchase(ninja);
                Check(currency.Coins == 777 && inventory.Owns(ninja.Id) && inventory.GetEquippedId(CosmeticCategory.Skin) == ninja.Id,
                    "Ninja IAP unlocks and equips without coins");
                Check(payments.LastProductId == "skin_ninja", "Ninja product id");
                int purchaseCount = payments.BuyCount;
                shop.Purchase(ninja);
                Check(payments.BuyCount == purchaseCount, "Owned IAP is not bought twice");
                shop.Purchase(valkyrie);
                Check(currency.Coins == 777 && inventory.Owns(valkyrie.Id)
                      && inventory.GetEquippedId(CosmeticCategory.Skin) == valkyrie.Id, "Valkyrie IAP unlocks and equips");

                Check(shop.Equip(elf) && inventory.GetEquippedId(CosmeticCategory.Skin) == "" && currency.Coins == 777, "Free default selection");
                Check(shop.Equip(ninja), "Re-equip owned ninja");
                Check(!inventory.TryEquip(CosmeticCategory.Skin, "unowned"), "Unowned equip");
                var saved = JsonUtility.FromJson<SavesYG>(JsonUtility.ToJson(YG2.saves));
                var restored = new CosmeticInventoryService(currency);
                restored.Load(saved.OwnedSkins, saved.OwnedProjectiles, saved.OwnedHats, saved.EquippedSkin, saved.EquippedProjectile, saved.EquippedHat);
                Check(restored.Owns("skin_stub_01") && restored.GetEquippedId(CosmeticCategory.Skin) == ninja.Id, "Save round-trip/legacy id");
                Check(!inventory.TryBuyWithCoins(elf), "Default cannot be purchased");

                foreach (ShopItemDefinition item in catalog.Items)
                {
                    string itemPath = AssetDatabase.GetAssetPath(item);
                    string prefabPath = AssetDatabase.GUIDToAssetPath(item.PrefabReference.AssetGUID);
                    string iconPath = AssetDatabase.GUIDToAssetPath(item.IconReference.AssetGUID);
                    foreach (string path in new[] { itemPath, prefabPath, iconPath })
                        Check(AddressableAssetSettingsDefaultObject.Settings.FindAssetEntry(AssetDatabase.AssetPathToGUID(path)) != null,
                            "Not addressable: " + path);

                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    var animator = prefab.GetComponentInChildren<Animator>(true);
                    Check(animator != null && !animator.applyRootMotion
                          && animator.runtimeAnimatorController.animationClips.All(c => c.isLooping), "Loop/no root motion: " + item.Id);
                    Check(prefab.GetComponentInChildren<SkinnedMeshRenderer>(true).sharedMaterial.mainTexture != null, "Texture: " + item.Id);
                }

                var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Prefabs/GG.prefab");
                Check(playerPrefab.GetComponent<PlayerSkinController>() != null, "Player wiring");
                Check(playerPrefab.transform.Find("Visual/Elf") != null, "New default Elf visual");

                foreach (string enemyPath in new[]
                         {
                             "Assets/Game/Prefabs/Enemys/EnemyGoblin.prefab",
                             "Assets/Game/Prefabs/Enemys/EnemyOrc.prefab",
                             "Assets/Game/Prefabs/Enemys/EnemyTroll.prefab"
                         })
                {
                    var enemy = AssetDatabase.LoadAssetAtPath<GameObject>(enemyPath);
                    Transform visual = enemy.transform.Find("Visual");
                    Animator animator = visual.GetComponentInChildren<Animator>(true);
                    Check(visual != null && visual.childCount == 1, "Enemy visual replacement: " + enemyPath);
                    Check(enemy.GetComponent<Game.Scripts.Characters.Enemy.Enemy>() != null, "Enemy behavior retained: " + enemyPath);
                    Check(animator != null && !animator.applyRootMotion
                          && animator.runtimeAnimatorController.animationClips.All(c => c.isLooping), "Enemy animation: " + enemyPath);
                    Check(visual.GetComponentInChildren<SkinnedMeshRenderer>(true).sharedMaterial.mainTexture != null, "Enemy texture: " + enemyPath);

                    var feedback = enemy.GetComponent<HitFeedback>();
                    Check(feedback != null, "Enemy hit feedback: " + enemyPath);
                    var feedbackData = new SerializedObject(feedback);
                    Check(feedbackData.FindProperty("_feedbackType").enumValueIndex == (int)HitFeedbackType.Enemy,
                        "Enemy feedback type: " + enemyPath);
                    Check(Mathf.Approximately(feedbackData.FindProperty("_flashDuration").floatValue, 0.12f),
                        "Enemy flash duration: " + enemyPath);
                    Check(feedbackData.FindProperty("_flashCount").intValue == 1,
                        "Enemy flash count: " + enemyPath);
                }

                foreach (string materialPath in CharacterMaterialPaths)
                    ValidateFlatKitMaterial(materialPath);

                var shopUi = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Prefabs/UI/ShopUI.prefab");
                Transform[] shopTransforms = shopUi.GetComponentsInChildren<Transform>(true);
                Check(!shopTransforms.Single(t => t.name == "TabProjectiles").gameObject.activeSelf, "Projectile tab hidden");
                Check(!shopTransforms.Single(t => t.name == "TabHats").gameObject.activeSelf, "Hat tab hidden");
                Check(!Directory.GetFiles("Assets/Game/Art/Characters", "*.obj", SearchOption.AllDirectories).Any(), "No static OBJ runtime assets");
                Debug.Log("[SkinChecks] PASS: seven skins, prices, IAP, save compatibility, visuals, enemies, hidden tabs and Addressables.");
            }
            finally
            {
                YG2.saves = original;
                sdkField.SetValue(null, originalSdkEnabled);
            }
        }

        public static async UniTask RunAsyncChecks()
        {
            Check(EditorApplication.isPlaying, "Async checks require Play Mode.");
            AsyncResult = "Running";
            var root = new GameObject("SkinTestPlayer");
            var visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform);
            var elf = new GameObject("DefaultElf");
            elf.transform.SetParent(visual.transform);
            var prefab = new GameObject("TestSkinPrefab");
            var controller = root.AddComponent<PlayerSkinController>();
            var serialized = new SerializedObject(controller);
            serialized.FindProperty("_visualRoot").objectReferenceValue = visual.transform;
            serialized.FindProperty("_defaultModel").objectReferenceValue = elf;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            var inventory = new TestInventory();
            var content = new TestContent();
            var provider = new ShopCatalogProvider();
            provider.SetCatalog(AssetDatabase.LoadAssetAtPath<ShopCatalog>("Assets/Game/Configs/Shop/ShopCatalog.asset"));
            try
            {
                await controller.InitializeAsync(inventory, provider, content);
                inventory.TryEquip(CosmeticCategory.Skin, "skin_stub_01");
                Check(elf.activeSelf, "Old model remains during load");
                inventory.TryEquip(CosmeticCategory.Skin, "");
                content.Pending.Dequeue().TrySetResult(prefab);
                Check(elf.activeSelf && visual.transform.childCount == 1 && content.Released == 1, "Stale load must not replace elf");
                inventory.TryEquip(CosmeticCategory.Skin, "skin_stub_01");
                content.Pending.Dequeue().TrySetException(new Exception("Expected test failure"));
                Check(elf.activeSelf, "Load failure keeps model");
                inventory.TryEquip(CosmeticCategory.Skin, "skin_stub_01");
                content.Pending.Dequeue().TrySetResult(prefab);
                Check(!elf.activeSelf && visual.transform.Find("EquippedSkin") != null, "Retry succeeds");
                inventory.TryEquip(CosmeticCategory.Skin, "");
                await UniTask.DelayFrame(2);
                Check(elf.activeSelf && visual.transform.childCount == 1 && content.Released == 2, "Switch back releases prefab");
                inventory.TryEquip(CosmeticCategory.Skin, "skin_stub_01");
                Object.Destroy(root);
                await UniTask.DelayFrame(2);
                content.Pending.Dequeue().TrySetResult(prefab);
                Check(content.Released == 3, "Destroyed player releases late result");
                await RunHitFeedbackChecks();
                AsyncResult = "PASS: skin switching, balanced releases and FlatKit hit feedback";
                Debug.Log("[SkinChecks] " + AsyncResult);
            }
            catch (Exception exception) { AsyncResult = "FAIL: " + exception; Debug.LogException(exception); }
            finally { if (root != null) Object.Destroy(root); Object.Destroy(prefab); }
        }

        public static async UniTask RunActualPrefabChecks()
        {
            Check(EditorApplication.isPlaying, "Actual prefab checks require Play Mode.");
            ActualPrefabResult = "Running";
            GameObject root = null;
            try
            {
                var catalog = AssetDatabase.LoadAssetAtPath<ShopCatalog>("Assets/Game/Configs/Shop/ShopCatalog.asset");
                var provider = new ShopCatalogProvider();
                provider.SetCatalog(catalog);
                var inventory = new TestInventory(catalog.Items.Where(i => !i.IsDefault).Select(i => i.Id));
                var content = new AddressablesContentService();
                await content.InitializeAsync();

                root = new GameObject("ActualSkinPrefabTest");
                var visual = new GameObject("Visual");
                visual.transform.SetParent(root.transform);
                var defaultElf = new GameObject("DefaultElf");
                defaultElf.transform.SetParent(visual.transform);
                var controller = root.AddComponent<PlayerSkinController>();
                var serialized = new SerializedObject(controller);
                serialized.FindProperty("_visualRoot").objectReferenceValue = visual.transform;
                serialized.FindProperty("_defaultModel").objectReferenceValue = defaultElf;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                await controller.InitializeAsync(inventory, provider, content);

                var appliedId = typeof(PlayerSkinController).GetField("_appliedId",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                Check(appliedId != null, "PlayerSkinController applied id field");
                foreach (ShopItemDefinition item in catalog.Items.Where(i => !i.IsDefault))
                {
                    inventory.TryEquip(CosmeticCategory.Skin, item.Id);
                    for (int frame = 0; frame < 600 && (string)appliedId.GetValue(controller) != item.Id; frame++)
                        await UniTask.Yield();

                    Check((string)appliedId.GetValue(controller) == item.Id, "Timed out loading " + item.Id);
                    Transform equipped = visual.transform.Find("EquippedSkin");
                    Check(equipped != null, "Missing instantiated visual for " + item.Id);
                    var renderer = equipped.GetComponentInChildren<SkinnedMeshRenderer>(true);
                    var animator = equipped.GetComponentInChildren<Animator>(true);
                    Check(renderer != null && renderer.sharedMaterial != null && renderer.sharedMaterial.mainTexture != null,
                        "Runtime material/texture for " + item.Id);
                    Check(animator != null && !animator.applyRootMotion, "Runtime animator for " + item.Id);
                }

                inventory.TryEquip(CosmeticCategory.Skin, string.Empty);
                await UniTask.DelayFrame(3);
                Check(defaultElf.activeSelf && visual.transform.childCount == 1, "Return to default after real prefab loads");
                ActualPrefabResult = "PASS: all six purchasable skin prefabs loaded, instantiated, animated and released through Addressables";
                Debug.Log("[SkinChecks] " + ActualPrefabResult);
            }
            catch (Exception exception)
            {
                ActualPrefabResult = "FAIL: " + exception;
                Debug.LogException(exception);
            }
            finally
            {
                if (root != null) Object.Destroy(root);
            }
        }

        private static void ValidateFlatKitMaterial(string materialPath)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            Check(material != null, "Missing character material: " + materialPath);
            Check(material.shader != null && material.shader.name == "FlatKit/Stylized Surface With Outline",
                "FlatKit outline shader: " + materialPath);
            Check(material.GetTexture("_BaseMap") != null, "Base texture: " + materialPath);
            Check(material.GetTexture("_BumpMap") != null, "Normal texture: " + materialPath);
            Check(material.IsKeywordEnabled("_CELPRIMARYMODE_SINGLE"), "Single cel keyword: " + materialPath);
            Check(material.IsKeywordEnabled("_NORMALMAP"), "Normal map keyword: " + materialPath);
            Check(material.IsKeywordEnabled("_EMISSION"), "Emission keyword: " + materialPath);
            Check(material.GetTexture("_EmissionMap") != null, "Emission map: " + materialPath);
            Check(Mathf.Approximately(material.GetFloat("_SelfShadingSize"), 0.65f),
                "Cel shadow size: " + materialPath);
            Check(Mathf.Approximately(material.GetFloat("_ShadowEdgeSize"), 0.02f),
                "Cel edge size: " + materialPath);
            Check(Mathf.Approximately(material.GetFloat("_TextureImpact"), 1f),
                "Texture impact: " + materialPath);
            Check(Mathf.Approximately(material.GetFloat("_OutlineWidth"), 0.01f),
                "Outline width: " + materialPath);
            Check(material.GetColor("_EmissionColor") == Color.black,
                "Default emission color: " + materialPath);
        }

        private static async UniTask RunHitFeedbackChecks()
        {
            GameObject root = null;
            try
            {
                root = new GameObject("HitFeedbackRuntimeCheck");
                var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
                visual.name = "Visual";
                visual.transform.SetParent(root.transform, false);
                var renderer = visual.GetComponent<MeshRenderer>();
                renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(
                    "Assets/Game/Art/Characters/Enemies/Goblin/Goblin.mat");

                var health = root.AddComponent<Health>();
                var feedback = root.AddComponent<HitFeedback>();
                var serialized = new SerializedObject(feedback);
                serialized.FindProperty("_feedbackType").enumValueIndex = (int)HitFeedbackType.Enemy;
                serialized.FindProperty("_flashDuration").floatValue = 0.12f;
                serialized.FindProperty("_flashCount").intValue = 1;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                feedback.RefreshRenderers();

                health.TakeDamage(1f);
                await UniTask.Yield();
                Check(EmissionIsFlashing(renderer), "Damage starts a white emission flash");

                await UniTask.Delay(TimeSpan.FromSeconds(0.07f));
                health.TakeDamage(1f);
                await UniTask.Delay(TimeSpan.FromSeconds(0.08f));
                Check(EmissionIsFlashing(renderer), "Repeated damage restarts the flash timer");

                await UniTask.Delay(TimeSpan.FromSeconds(0.06f));
                Check(!EmissionIsFlashing(renderer), "Flash restores the original emission");

                health.TakeDamage(1f);
                await UniTask.Yield();
                root.SetActive(false);
                Check(!EmissionIsFlashing(renderer), "Disabling a pooled object clears the flash");
            }
            finally
            {
                if (root != null)
                    Object.Destroy(root);
            }
        }

        private static bool EmissionIsFlashing(Renderer renderer)
        {
            var properties = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(properties, 0);
            Color emission = properties.GetColor(Shader.PropertyToID("_EmissionColor"));
            return emission.r > 1f && emission.g > 1f && emission.b > 1f;
        }

        private sealed class TestPayments : IPaymentsService
        {
            public event Action<string> PurchaseSucceeded;
            public event Action<string> PurchaseFailed { add { } remove { } }
            public string LastProductId { get; private set; }
            public int BuyCount { get; private set; }
            public void Initialize() { }
            public void Buy(string id)
            {
                LastProductId = id;
                BuyCount++;
                PurchaseSucceeded?.Invoke(id);
            }
            public void ConsumePendingPurchases() { }
        }

        private sealed class TestInventory : ICosmeticInventoryService
        {
            public event Action Changed;
            private string _id = "";
            private readonly HashSet<string> _owned;

            public TestInventory(IEnumerable<string> owned = null)
            {
                _owned = new HashSet<string>(owned ?? new[] { "skin_stub_01" });
            }

            public bool Owns(string id) => _owned.Contains(id);
            public string GetEquippedId(CosmeticCategory category) => _id;
            public IReadOnlyCollection<string> GetOwnedIds(CosmeticCategory category) => _owned;
            public bool TryEquip(CosmeticCategory category, string id)
            {
                if (!string.IsNullOrEmpty(id) && !_owned.Contains(id)) return false;
                _id = id;
                Changed?.Invoke();
                return true;
            }
            public bool TryBuyWithCoins(ShopItemDefinition item) => throw new NotSupportedException();
            public void UnlockFromIap(ShopItemDefinition item) => throw new NotSupportedException();
            public void Load(IReadOnlyList<string> s, IReadOnlyList<string> p, IReadOnlyList<string> h, string es, string ep, string eh) => throw new NotSupportedException();
            public void LogCurrentEquipment() { }
        }

        private sealed class TestContent : IAddressablesContentService
        {
            public readonly Queue<UniTaskCompletionSource<GameObject>> Pending = new();
            public int Released;
            public bool IsInitialized => true;
            public UniTask InitializeAsync(CancellationToken token = default) => UniTask.CompletedTask;
            public UniTask<GameObject> LoadGameObjectAsync(AssetReferenceGameObject reference, CancellationToken token = default)
            {
                var completion = new UniTaskCompletionSource<GameObject>();
                Pending.Enqueue(completion);
                return completion.Task;
            }
            public UniTask<T> LoadAssetAsync<T>(string address, CancellationToken token = default) where T : Object => throw new NotSupportedException();
            public UniTask<T> LoadAssetAsync<T>(AssetReference reference, CancellationToken token = default) where T : Object => throw new NotSupportedException();
            public UniTask<IList<T>> LoadByLabelAsync<T>(string label, CancellationToken token = default) where T : Object => throw new NotSupportedException();
            public UniTask DownloadDependenciesAsync(string key, CancellationToken token = default) => UniTask.CompletedTask;
            public void Release(AssetReference reference) => throw new NotSupportedException();
            public void Release(Object asset) => ++Released;
        }
    }
}
#endif
