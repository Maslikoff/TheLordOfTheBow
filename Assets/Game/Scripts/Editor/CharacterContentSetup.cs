#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Game.Scripts.Characters.Player;
using Game.Scripts.Shop;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Scripts.Editor
{
    public static class CharacterContentSetup
    {
        private const string PlayerPrefabPath = "Assets/Game/Prefabs/GG.prefab";
        private const string SkinPrefabDirectory = "Assets/Game/Prefabs/Skins";
        private const string EnemyVisualDirectory = "Assets/Game/Prefabs/EnemyVisuals";
        private const string ShopConfigDirectory = "Assets/Game/Configs/Shop";
        private const string ShopIconDirectory = "Assets/Game/Art/Shop";
        private const string CatalogPath = ShopConfigDirectory + "/ShopCatalog.asset";
        private const string RenderDirectory = "Design/Characters/Renders";
        private const string CharacterShaderName = "FlatKit/Stylized Surface With Outline";
        private const string WhiteEmissionTexturePath =
            "Assets/FlatKit/Demos/[Demo] Valley/Valley-Textures/Valley-Soil00-White.png";

        private static readonly Color ToonShadowColor = new Color(0.65f, 0.65f, 0.65f, 1f);
        private static readonly Color ToonOutlineColor = new Color(0.035f, 0.045f, 0.06f, 1f);

        private sealed class CharacterSpec
        {
            public string Key;
            public string ModelPath;
            public string PrefabPath;
            public string ItemPath;
            public string IconPath;
            public string Id;
            public string NameRu;
            public string NameEn;
            public string NameTr;
            public string DescriptionRu;
            public string DescriptionEn;
            public string DescriptionTr;
            public int CoinPrice;
            public bool IsDefault;
            public bool IsIap;
            public string IapProductId;
        }

        private sealed class EnemySpec
        {
            public string Key;
            public string ModelPath;
            public string VisualPrefabPath;
            public string EnemyPrefabPath;
        }

        private static readonly CharacterSpec[] PlayerSpecs =
        {
            Player("Elf", "Elf@Run", "Skin_Default_Elf", "skin_default_elf",
                "Эльф", "Elf", "Elf", 0, true, false, ""),
            Player("ElfFemale", "ElfFemale@Run", "Skin_ElfFemale", "skin_elf_female",
                "Эльфийка", "Elf Maiden", "Elf Kızı", 25, false, false, ""),
            Player("Knight", "Knight@Run", "Skin_Knight", "skin_knight",
                "Рыцарь", "Knight", "Şövalye", 110, false, false, ""),
            Player("Wizard", "Wizard@Run", "Skin_Wizard", "skin_wizard",
                "Маг", "Wizard", "Büyücü", 110, false, false, ""),
            Player("Dwarf", "Dwarf@Run", "Skin_Dwarf", "skin_dwarf",
                "Дварф", "Dwarf", "Cüce", 110, false, false, ""),
            Player("Valkyrie", "Valkyrie@Run", "Skin_Valkyrie", "skin_valkyrie",
                "Валькирия", "Valkyrie", "Valkür", 0, false, true, "skin_valkyrie")
        };

        private static readonly CharacterSpec NinjaSpec = new CharacterSpec
        {
            Key = "Ninja",
            ModelPath = "Assets/Game/Art/Characters/Ninja/Ninja@Run.fbx",
            PrefabPath = "Assets/Game/Prefabs/Skins/Ninja.prefab",
            ItemPath = ShopConfigDirectory + "/Skin_Stub_01.asset",
            IconPath = ShopIconDirectory + "/Icon_Ninja.png",
            Id = "skin_stub_01",
            NameRu = "Ниндзя",
            NameEn = "Ninja",
            NameTr = "Ninja",
            DescriptionRu = "Тихий воин в чёрном. Меняет внешний вид героя.",
            DescriptionEn = "A silent warrior in black. Changes your hero's appearance.",
            DescriptionTr = "Siyahlı sessiz savaşçı. Kahramanının görünümünü değiştirir.",
            CoinPrice = 0,
            IsIap = true,
            IapProductId = "skin_ninja"
        };

        private static readonly EnemySpec[] EnemySpecs =
        {
            Enemy("Goblin", "Goblin@Run", "EnemyGoblin"),
            Enemy("Orc", "Orc@Run", "EnemyOrc"),
            Enemy("Troll", "Troll@Run", "EnemyTroll")
        };

        [MenuItem("Tools/Characters/Setup All Skins And Enemies")]
        public static void SetupAll()
        {
            if (EditorApplication.isPlaying)
                throw new InvalidOperationException("Exit Play Mode before setting up character content.");

            Directory.CreateDirectory(SkinPrefabDirectory);
            Directory.CreateDirectory(EnemyVisualDirectory);
            Directory.CreateDirectory(ShopConfigDirectory);
            Directory.CreateDirectory(ShopIconDirectory);
            Directory.CreateDirectory(RenderDirectory);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            float playerHeight = GetVisualHeight(PlayerPrefabPath);
            foreach (CharacterSpec spec in PlayerSpecs)
            {
                ConfigureModelAndTextures(spec.ModelPath);
                CreateVisualPrefab(spec.Key, spec.ModelPath, spec.PrefabPath, playerHeight);
            }

            ReplaceDefaultPlayerVisual(PlayerSpecs[0].PrefabPath);

            foreach (EnemySpec spec in EnemySpecs)
            {
                float enemyHeight = GetVisualHeight(spec.EnemyPrefabPath);
                ConfigureModelAndTextures(spec.ModelPath);
                CreateVisualPrefab(spec.Key, spec.ModelPath, spec.VisualPrefabPath, enemyHeight);
                ReplaceEnemyVisual(spec);
            }

            ConfigureCatalog();
            ConfigureIconImporters();
            RegisterAddressables();
            RenderShopReferences();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Characters] Player skins, enemy visuals, shop catalog and Addressables configured.");
        }

        [MenuItem("Tools/Characters/Apply FlatKit To Character Materials")]
        private static void ApplyFlatKitToCharacterMaterials()
        {
            if (EditorApplication.isPlaying)
                throw new InvalidOperationException("Exit Play Mode before updating character materials.");

            foreach (CharacterSpec spec in PlayerSpecs.Concat(new[] { NinjaSpec }))
                ConfigureMaterialForModel(spec.Key, spec.ModelPath);

            foreach (EnemySpec spec in EnemySpecs)
                ConfigureMaterialForModel(spec.Key, spec.ModelPath);

            AssetDatabase.SaveAssets();
            Debug.Log("[Characters] FlatKit applied to seven skins and three enemy materials.");
        }

        public static void ConfigureCatalog()
        {
            Directory.CreateDirectory(ShopConfigDirectory);
            var catalog = AssetDatabase.LoadAssetAtPath<ShopCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<ShopCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }

            var items = new List<ShopItemDefinition>();
            foreach (CharacterSpec spec in PlayerSpecs.Take(5))
                items.Add(ConfigureItem(spec));

            items.Add(ConfigureItem(NinjaSpec));
            items.Add(ConfigureItem(PlayerSpecs[5]));
            catalog.SetItems(items);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
        }

        public static void RegisterAddressables()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogWarning("[Characters] Addressables settings are not available yet.");
                return;
            }

            foreach (CharacterSpec spec in PlayerSpecs.Concat(new[] { NinjaSpec }))
            {
                MarkAddressable(settings, spec.PrefabPath, "Remote_Characters", $"Prefabs/Skins/{spec.Key}");
                MarkAddressable(settings, spec.ItemPath, "Remote_Configs", $"Configs/Shop/{Path.GetFileNameWithoutExtension(spec.ItemPath)}");
                if (File.Exists(spec.IconPath))
                    MarkAddressable(settings, spec.IconPath, "Remote_Icons", $"UI/Icons/Shop/{Path.GetFileNameWithoutExtension(spec.IconPath)}");
            }

            foreach (EnemySpec spec in EnemySpecs)
                MarkAddressable(settings, spec.VisualPrefabPath, "Remote_Characters", $"Prefabs/EnemyVisuals/{spec.Key}");

            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Tools/Characters/Render Shop References")]
        public static void RenderShopReferences()
        {
            Directory.CreateDirectory(RenderDirectory);
            foreach (CharacterSpec spec in PlayerSpecs.Concat(new[] { NinjaSpec }))
                RenderPrefab(spec.PrefabPath, Path.Combine(RenderDirectory, spec.Key + ".png"));

            Debug.Log($"[Characters] Shop reference renders saved to {RenderDirectory}.");
        }

        public static Bounds GeometryBounds(GameObject root)
        {
            bool initialized = false;
            Bounds bounds = default;
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer is SkinnedMeshRenderer skinned)
                {
                    var baked = new Mesh();
                    skinned.BakeMesh(baked, true);
                    foreach (Vector3 vertex in baked.vertices)
                    {
                        Vector3 point = skinned.transform.TransformPoint(vertex);
                        if (!initialized)
                        {
                            bounds = new Bounds(point, Vector3.zero);
                            initialized = true;
                        }
                        else bounds.Encapsulate(point);
                    }
                    Object.DestroyImmediate(baked);
                }
                else
                {
                    if (!initialized)
                    {
                        bounds = renderer.bounds;
                        initialized = true;
                    }
                    else bounds.Encapsulate(renderer.bounds);
                }
            }

            return initialized ? bounds : new Bounds(root.transform.position, Vector3.one);
        }

        private static CharacterSpec Player(string key, string modelFile, string itemFile, string id,
            string ru, string en, string tr, int price, bool isDefault, bool isIap, string iapId)
        {
            string art = $"Assets/Game/Art/Characters/{key}";
            return new CharacterSpec
            {
                Key = key,
                ModelPath = $"{art}/{modelFile}.fbx",
                PrefabPath = $"{SkinPrefabDirectory}/{key}.prefab",
                ItemPath = $"{ShopConfigDirectory}/{itemFile}.asset",
                IconPath = $"{ShopIconDirectory}/Icon_{key}.png",
                Id = id,
                NameRu = ru,
                NameEn = en,
                NameTr = tr,
                DescriptionRu = $"Облик «{ru}» для героя.",
                DescriptionEn = $"{en} appearance for the hero.",
                DescriptionTr = $"Kahraman için {tr} görünümü.",
                CoinPrice = price,
                IsDefault = isDefault,
                IsIap = isIap,
                IapProductId = iapId
            };
        }

        private static EnemySpec Enemy(string key, string modelFile, string enemyPrefab)
        {
            return new EnemySpec
            {
                Key = key,
                ModelPath = $"Assets/Game/Art/Characters/Enemies/{key}/{modelFile}.fbx",
                VisualPrefabPath = $"{EnemyVisualDirectory}/{key}.prefab",
                EnemyPrefabPath = $"Assets/Game/Prefabs/Enemys/{enemyPrefab}.prefab"
            };
        }

        private static void ConfigureModelAndTextures(string modelPath)
        {
            if (!File.Exists(modelPath))
                throw new FileNotFoundException("Animated model is missing", modelPath);

            string directory = Path.GetDirectoryName(modelPath)?.Replace('\\', '/');
            string stem = Path.GetFileNameWithoutExtension(modelPath);
            string baseColor = FindTexture(directory, stem + "_basecolor");
            string normal = FindTexture(directory, stem + "_normal");
            string rm = FindTexture(directory, stem + "_rm");

            ConfigureTexture(baseColor, false);
            ConfigureTexture(normal, true);
            ConfigureTexture(rm, false);

            var importer = AssetImporter.GetAtPath(modelPath) as ModelImporter;
            if (importer == null)
                throw new InvalidOperationException($"No ModelImporter for {modelPath}");

            importer.animationType = ModelImporterAnimationType.Generic;
            importer.importAnimation = true;
            importer.importCameras = false;
            importer.importLights = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.globalScale = 100f;
            importer.optimizeGameObjects = false;

            ModelImporterClipAnimation[] clips = importer.defaultClipAnimations;
            if (clips == null || clips.Length == 0)
                clips = importer.clipAnimations;
            foreach (ModelImporterClipAnimation clip in clips)
            {
                clip.name = "Run";
                clip.loopTime = true;
                clip.loopPose = true;
                clip.lockRootRotation = true;
                clip.lockRootHeightY = true;
                clip.lockRootPositionXZ = true;
                clip.keepOriginalOrientation = true;
                clip.keepOriginalPositionY = true;
                clip.keepOriginalPositionXZ = true;
            }
            importer.clipAnimations = clips;
            importer.SaveAndReimport();
        }

        private static void ConfigureTexture(string path, bool normalMap)
        {
            if (string.IsNullOrEmpty(path))
                return;
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
                return;
            importer.textureType = normalMap ? TextureImporterType.NormalMap : TextureImporterType.Default;
            importer.maxTextureSize = 1024;
            importer.mipmapEnabled = true;
            importer.SaveAndReimport();
        }

        private static string FindTexture(string directory, string prefix)
        {
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
                return null;
            return Directory.GetFiles(directory, prefix + ".*", SearchOption.TopDirectoryOnly)
                .Select(path => path.Replace('\\', '/'))
                .FirstOrDefault(path => path.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                                        || path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                                        || path.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase));
        }

        private static void CreateVisualPrefab(string key, string modelPath, string prefabPath, float targetHeight)
        {
            string directory = Path.GetDirectoryName(modelPath)?.Replace('\\', '/');
            string stem = Path.GetFileNameWithoutExtension(modelPath);
            string materialPath = $"{directory}/{key}.mat";
            string controllerPath = $"{directory}/{key}.controller";
            Material material = CreateMaterial(materialPath,
                FindCharacterTexture(directory, stem, key, "_basecolor"),
                FindCharacterTexture(directory, stem, key, "_normal"));
            AnimationClip run = AssetDatabase.LoadAllAssetsAtPath(modelPath).OfType<AnimationClip>()
                .FirstOrDefault(clip => !clip.name.StartsWith("__", StringComparison.Ordinal));
            if (run == null)
                throw new InvalidOperationException($"No animation clip found in {modelPath}");
            AnimatorController controller = CreateController(controllerPath, run);

            var source = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            var prefabRoot = new GameObject(key);
            try
            {
                var model = (GameObject)PrefabUtility.InstantiatePrefab(source);
                model.name = key + "Model";
                model.transform.SetParent(prefabRoot.transform, false);
                foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>(true))
                {
                    var materials = new Material[renderer.sharedMaterials.Length];
                    for (int i = 0; i < materials.Length; i++) materials[i] = material;
                    renderer.sharedMaterials = materials;
                    if (renderer is SkinnedMeshRenderer skinned) skinned.updateWhenOffscreen = true;
                }

                Animator animator = model.GetComponent<Animator>();
                if (animator == null)
                    animator = model.AddComponent<Animator>();
                animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
                run.SampleAnimation(model, 0f);

                Bounds bounds = GeometryBounds(prefabRoot);
                if (bounds.size.y > 0.0001f && targetHeight > 0.0001f)
                    model.transform.localScale *= targetHeight / bounds.size.y;
                bounds = GeometryBounds(prefabRoot);
                model.transform.localPosition += new Vector3(-bounds.center.x, -bounds.min.y, -bounds.center.z);

                Directory.CreateDirectory(Path.GetDirectoryName(prefabPath) ?? SkinPrefabDirectory);
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            }
            finally
            {
                Object.DestroyImmediate(prefabRoot);
            }
        }

        private static Material CreateMaterial(string path, string baseColorPath, string normalPath)
        {
            Shader shader = Shader.Find(CharacterShaderName);
            if (shader == null)
                throw new InvalidOperationException($"{CharacterShaderName} shader was not found.");

            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            else
            {
                material.shader = shader;
            }

            material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(baseColorPath));
            material.SetColor("_BaseColor", Color.white);
            material.SetFloat("_CelPrimaryMode", 1f);
            material.SetColor("_ColorDim", ToonShadowColor);
            material.SetFloat("_SelfShadingSize", 0.65f);
            material.SetFloat("_ShadowEdgeSize", 0.02f);
            material.SetFloat("_Flatness", 1f);
            material.SetFloat("_CelExtraEnabled", 0f);
            material.SetFloat("_SpecularEnabled", 0f);
            material.SetFloat("_RimEnabled", 0f);
            material.SetFloat("_GradientEnabled", 0f);
            material.SetFloat("_BaseMapPremultiply", 0f);
            material.SetFloat("_TextureBlendingMode", 0f);
            material.SetFloat("_TextureImpact", 1f);
            material.SetColor("_OutlineColor", ToonOutlineColor);
            material.SetFloat("_OutlineWidth", 0.01f);
            material.SetFloat("_OutlineScale", 1f);
            material.SetFloat("_OutlineDepthOffset", 0f);
            material.SetFloat("_CameraDistanceImpact", 0f);
            Texture2D whiteEmissionTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(WhiteEmissionTexturePath);
            if (whiteEmissionTexture == null)
                throw new InvalidOperationException($"White emission texture is missing: {WhiteEmissionTexturePath}");

            material.SetTexture("_EmissionMap", whiteEmissionTexture);
            material.SetColor("_EmissionColor", Color.black);

            material.EnableKeyword("_CELPRIMARYMODE_SINGLE");
            material.DisableKeyword("_CELPRIMARYMODE_STEPS");
            material.DisableKeyword("_CELPRIMARYMODE_CURVE");
            material.DisableKeyword("DR_CEL_EXTRA_ON");
            material.DisableKeyword("DR_SPECULAR_ON");
            material.DisableKeyword("DR_RIM_ON");
            material.DisableKeyword("DR_GRADIENT_ON");
            material.DisableKeyword("_BASEMAP_PREMULTIPLY");
            material.EnableKeyword("_TEXTUREBLENDINGMODE_MULTIPLY");
            material.DisableKeyword("_TEXTUREBLENDINGMODE_ADD");
            material.EnableKeyword("_EMISSION");

            Texture2D normal = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);
            material.SetTexture("_BumpMap", normal);
            if (normal != null) material.EnableKeyword("_NORMALMAP");
            else material.DisableKeyword("_NORMALMAP");
            material.enableInstancing = true;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void ConfigureMaterialForModel(string key, string modelPath)
        {
            string directory = Path.GetDirectoryName(modelPath)?.Replace('\\', '/');
            string stem = Path.GetFileNameWithoutExtension(modelPath);
            string materialPath = $"{directory}/{key}.mat";
            CreateMaterial(materialPath,
                FindCharacterTexture(directory, stem, key, "_basecolor"),
                FindCharacterTexture(directory, stem, key, "_normal"));
        }

        private static string FindCharacterTexture(string directory, string modelStem, string characterKey,
            string suffix)
        {
            return FindTexture(directory, modelStem + suffix)
                   ?? FindTexture(directory, characterKey + suffix);
        }

        private static AnimatorController CreateController(string path, AnimationClip run)
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (controller == null)
                controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            AnimatorState state = stateMachine.states.Select(child => child.state)
                .FirstOrDefault(candidate => candidate != null && candidate.name == "Run");
            if (state == null) state = stateMachine.AddState("Run");
            state.motion = run;
            state.speed = 1f;
            stateMachine.defaultState = state;
            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static float GetVisualHeight(string prefabPath)
        {
            var root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                Transform visual = root.transform.Find("Visual");
                if (visual == null) throw new InvalidOperationException($"Visual is missing in {prefabPath}");
                return Mathf.Max(0.1f, GeometryBounds(visual.gameObject).size.y);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ReplaceDefaultPlayerVisual(string visualPrefabPath)
        {
            var root = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            try
            {
                Transform visual = root.transform.Find("Visual");
                if (visual == null) throw new InvalidOperationException("Player Visual is missing.");
                ClearChildren(visual);
                var visualPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(visualPrefabPath);
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(visualPrefab, visual);
                instance.name = "Elf";
                instance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                instance.transform.localScale = Vector3.one;

                var controller = root.GetComponent<PlayerSkinController>() ?? root.AddComponent<PlayerSkinController>();
                var serialized = new SerializedObject(controller);
                serialized.FindProperty("_visualRoot").objectReferenceValue = visual;
                serialized.FindProperty("_defaultModel").objectReferenceValue = instance;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ReplaceEnemyVisual(EnemySpec spec)
        {
            var root = PrefabUtility.LoadPrefabContents(spec.EnemyPrefabPath);
            try
            {
                Transform visual = root.transform.Find("Visual");
                if (visual == null) throw new InvalidOperationException($"Visual is missing in {spec.EnemyPrefabPath}");
                ClearChildren(visual);
                var visualPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(spec.VisualPrefabPath);
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(visualPrefab, visual);
                instance.name = spec.Key;
                instance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                instance.transform.localScale = Vector3.one;
                PrefabUtility.SaveAsPrefabAsset(root, spec.EnemyPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ClearChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(parent.GetChild(i).gameObject);
        }

        private static ShopItemDefinition ConfigureItem(CharacterSpec spec)
        {
            var item = AssetDatabase.LoadAssetAtPath<ShopItemDefinition>(spec.ItemPath);
            if (item == null)
            {
                item = ScriptableObject.CreateInstance<ShopItemDefinition>();
                AssetDatabase.CreateAsset(item, spec.ItemPath);
            }

            item.ApplyStubData(spec.Id, CosmeticCategory.Skin,
                spec.NameRu, spec.NameEn, spec.NameTr,
                spec.DescriptionRu, spec.DescriptionEn, spec.DescriptionTr,
                spec.CoinPrice, spec.IsIap, spec.IapProductId);
            var serialized = new SerializedObject(item);
            serialized.FindProperty("_isDefault").boolValue = spec.IsDefault;
            SetReference(serialized, "_prefabReference", spec.PrefabPath);
            string icon = File.Exists(spec.IconPath) ? spec.IconPath : ShopIconDirectory + "/Icon_Skin.png";
            SetReference(serialized, "_iconReference", icon);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(item);
            return item;
        }

        private static void ConfigureIconImporters()
        {
            foreach (CharacterSpec spec in PlayerSpecs.Concat(new[] { NinjaSpec }))
            {
                if (!File.Exists(spec.IconPath)) continue;
                var importer = AssetImporter.GetAtPath(spec.IconPath) as TextureImporter;
                if (importer == null) continue;
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.maxTextureSize = 512;
                importer.SaveAndReimport();
            }
        }

        private static void SetReference(SerializedObject target, string field, string assetPath)
        {
            SerializedProperty reference = target.FindProperty(field);
            reference.FindPropertyRelative("m_AssetGUID").stringValue = AssetDatabase.AssetPathToGUID(assetPath);
            reference.FindPropertyRelative("m_SubObjectName").stringValue = string.Empty;
        }

        private static void MarkAddressable(UnityEditor.AddressableAssets.Settings.AddressableAssetSettings settings,
            string path, string groupName, string address)
        {
            if (!File.Exists(path)) return;
            var group = settings.FindGroup(groupName);
            if (group == null)
            {
                Debug.LogWarning($"[Characters] Addressables group is missing: {groupName}");
                return;
            }
            var entry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(path), group, false, false);
            entry.SetAddress(address);
            entry.SetLabel(groupName, true, true, false);
        }

        private static void RenderPrefab(string prefabPath, string outputPath)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null) return;
            var preview = new PreviewRenderUtility();
            Texture2D texture = null;
            try
            {
                preview.cameraFieldOfView = 28f;
                preview.camera.clearFlags = CameraClearFlags.SolidColor;
                preview.camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
                preview.camera.allowHDR = false;
                preview.camera.allowMSAA = true;
                preview.lights[0].intensity = 1.25f;
                preview.lights[0].transform.rotation = Quaternion.Euler(35f, 35f, 0f);
                preview.lights[1].intensity = 0.7f;
                preview.lights[1].transform.rotation = Quaternion.Euler(340f, 210f, 0f);
                preview.ambientColor = new Color(0.32f, 0.34f, 0.4f);

                GameObject instance = preview.InstantiatePrefabInScene(prefab);
                AnimationClip run = instance.GetComponentsInChildren<Animator>(true)
                    .SelectMany(animator => animator.runtimeAnimatorController != null
                        ? animator.runtimeAnimatorController.animationClips
                        : Array.Empty<AnimationClip>())
                    .FirstOrDefault();
                run?.SampleAnimation(instance, run.length * 0.18f);
                Bounds bounds = GeometryBounds(instance);
                float height = Mathf.Max(0.5f, bounds.size.y);
                Vector3 target = bounds.center + Vector3.up * height * 0.04f;
                preview.camera.transform.position = target + new Vector3(height * 0.68f, height * 0.18f, height * 2.25f);
                preview.camera.transform.rotation = Quaternion.LookRotation(target - preview.camera.transform.position);
                preview.camera.nearClipPlane = height * 0.01f;
                preview.camera.farClipPlane = height * 10f;

                preview.BeginStaticPreview(new Rect(0, 0, 1024, 1024));
                preview.camera.Render();
                texture = preview.EndStaticPreview();
                File.WriteAllBytes(outputPath, texture.EncodeToPNG());
            }
            finally
            {
                if (texture != null) Object.DestroyImmediate(texture);
                preview.Cleanup();
            }
        }
    }
}
#endif
