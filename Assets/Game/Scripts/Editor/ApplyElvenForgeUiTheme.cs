#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using Game.Scripts.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Editor
{
    public static class ApplyElvenForgeUiTheme
    {
        public const string ThemePath = "Assets/Game/Configs/UI/ElvenForgeUiTheme.asset";
        const string DisplayFontPath = "Assets/Game/Fonts/Climate_Crisis/ClimateCrisis-Regular-VariableFont_YEAR SDF.asset";
        const string BodyFontPath = "Assets/Game/Fonts/Jura/Jura-VariableFont_wght SDF.asset";
        const string SpriteDir = "Assets/Game/Art/UI/KenneyRpg/";
        const string GeneratedDir = SpriteDir + "Generated/";

        // Keep empty: Tutorial UI is hand-authored and must not be overwritten by theme bake.
        static readonly string[] ScenePaths = { };

        // Keep empty: UI prefabs are hand-authored; theme bake must not overwrite them.
        static readonly string[] PrefabPaths = { };

        static readonly HashSet<string> TitleNames = new HashSet<string>
        {
            "TitleText", "Title", "LVLText", "NextLVLText", "LevelText", "LevelTitle"
        };

        static readonly HashSet<string> TitleNameContains = new HashSet<string>
        {
            "победа", "проигрыш", "пауза", "уровень", "win", "lose", "pause", "level"
        };

        [MenuItem("Tools/UI/Ensure Elven Forge Theme Asset")]
        public static ElvenForgeUiTheme EnsureThemeAsset()
        {
            EnsureFolder("Assets/Game/Configs/UI");
            EnsureFolder(GeneratedDir.TrimEnd('/'));

            ConfigureKenneyImports();
            EnsureStitchedBar("slider_track", "barBack_horizontalLeft", "barBack_horizontalMid", "barBack_horizontalRight");
            EnsureStitchedBar("slider_fill", "barGreen_horizontalLeft", "barGreen_horizontalMid", "barGreen_horizontalRight");
            EnsureStitchedBar("slider_fill_gold", "barYellow_horizontalLeft", "barYellow_horizontalMid", "barYellow_horizontalRight");
            AssetDatabase.Refresh();

            var theme = AssetDatabase.LoadAssetAtPath<ElvenForgeUiTheme>(ThemePath);
            if (theme == null)
            {
                theme = ScriptableObject.CreateInstance<ElvenForgeUiTheme>();
                AssetDatabase.CreateAsset(theme, ThemePath);
            }

            theme.DisplayFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(DisplayFontPath);
            theme.BodyFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(BodyFontPath);

            theme.PanelModal = LoadSprite("panel_brown");
            theme.PanelParchment = LoadSprite("panel_beigeLight");
            theme.PanelHud = LoadSprite("panelInset_beige");
            theme.ButtonPrimary = LoadSprite("buttonLong_brown");
            theme.ButtonSecondary = LoadSprite("buttonLong_beige");
            theme.ButtonDanger = LoadSprite("buttonLong_grey");
            theme.ButtonReward = LoadSprite("buttonLong_blue");
            theme.ButtonIcon = LoadSprite("buttonSquare_brown");
            theme.ChipCoin = LoadSprite("buttonRound_beige");
            theme.SliderTrack = LoadGeneratedSprite("slider_track");
            theme.SliderFill = LoadGeneratedSprite("slider_fill");
            theme.SliderFillGold = LoadGeneratedSprite("slider_fill_gold");

            // Kenney warm beige/brown palette
            theme.Stone = ElvenForgeUiTheme.Hex("2A2118");
            theme.StoneMid = ElvenForgeUiTheme.Hex("3D3228");
            theme.StoneLight = ElvenForgeUiTheme.Hex("5A4A3A");
            theme.Parchment = ElvenForgeUiTheme.Hex("E8DCC8");
            theme.DimOverlay = new Color(0f, 0f, 0f, 0.55f);
            theme.Gold = ElvenForgeUiTheme.Hex("C9A227");
            theme.Mithril = ElvenForgeUiTheme.Hex("B8C4D0");
            theme.Teal = ElvenForgeUiTheme.Hex("5B8FA8");
            theme.Ember = ElvenForgeUiTheme.Hex("D4A574");
            theme.Moss = ElvenForgeUiTheme.Hex("6B8F5A");
            theme.Health = ElvenForgeUiTheme.Hex("7CB86A");
            theme.Forest = ElvenForgeUiTheme.Hex("3D5A32");
            theme.Danger = ElvenForgeUiTheme.Hex("8B4A3A");
            theme.Reward = ElvenForgeUiTheme.Hex("C9A227");
            theme.TextLight = ElvenForgeUiTheme.Hex("F5EFE0");
            theme.TextMuted = ElvenForgeUiTheme.Hex("A89880");
            theme.TextDark = ElvenForgeUiTheme.Hex("2A241C");

            EditorUtility.SetDirty(theme);
            AssetDatabase.SaveAssets();
            return theme;
        }

        static Sprite LoadSprite(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(SpriteDir + name + ".png");
        }

        static Sprite LoadGeneratedSprite(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(GeneratedDir + name + ".png");
        }

        static void ConfigureKenneyImports()
        {
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { SpriteDir.TrimEnd('/') });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith(".png") == false)
                    continue;

                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null)
                    continue;

                bool dirty = false;
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    dirty = true;
                }

                if (importer.spriteImportMode != SpriteImportMode.Single)
                {
                    importer.spriteImportMode = SpriteImportMode.Single;
                    dirty = true;
                }

                if (importer.mipmapEnabled)
                {
                    importer.mipmapEnabled = false;
                    dirty = true;
                }

                if (importer.alphaIsTransparency == false)
                {
                    importer.alphaIsTransparency = true;
                    dirty = true;
                }

                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                if (settings.spriteMeshType != SpriteMeshType.FullRect)
                {
                    settings.spriteMeshType = SpriteMeshType.FullRect;
                    dirty = true;
                }

                if (settings.spriteGenerateFallbackPhysicsShape)
                {
                    settings.spriteGenerateFallbackPhysicsShape = false;
                    dirty = true;
                }

                importer.SetTextureSettings(settings);

                var border = GuessBorder(Path.GetFileNameWithoutExtension(path), importer);
                if (importer.spriteBorder != border)
                {
                    importer.spriteBorder = border;
                    dirty = true;
                }

                if (dirty)
                    importer.SaveAndReimport();
            }
        }

        static Vector4 GuessBorder(string name, TextureImporter importer)
        {
            // x=left, y=bottom, z=right, w=top
            if (name.StartsWith("panel"))
                return new Vector4(16, 16, 16, 16);
            if (name.StartsWith("buttonLong"))
                return new Vector4(16, 12, 16, 12);
            if (name.StartsWith("buttonSquare"))
                return new Vector4(10, 10, 10, 10);
            if (name.StartsWith("buttonRound"))
                return new Vector4(12, 12, 12, 12);
            if (name.StartsWith("slider_"))
                return new Vector4(9, 4, 9, 4);
            if (name.StartsWith("bar"))
                return new Vector4(4, 4, 4, 4);
            if (name.StartsWith("icon") || name.StartsWith("arrow") || name.StartsWith("cursor"))
                return Vector4.zero;

            importer.GetSourceTextureWidthAndHeight(out int w, out int h);
            int b = Mathf.Clamp(Mathf.Min(w, h) / 4, 4, 16);
            return new Vector4(b, b, b, b);
        }

        static void EnsureStitchedBar(string outputName, string leftName, string midName, string rightName)
        {
            string outPath = GeneratedDir + outputName + ".png";
            string leftPath = SpriteDir + leftName + ".png";
            string midPath = SpriteDir + midName + ".png";
            string rightPath = SpriteDir + rightName + ".png";

            var leftTex = LoadReadableTexture(leftPath);
            var midTex = LoadReadableTexture(midPath);
            var rightTex = LoadReadableTexture(rightPath);
            if (leftTex == null || midTex == null || rightTex == null)
            {
                Debug.LogError($"[KenneyRpg] Failed to stitch {outputName}: missing source textures.");
                return;
            }

            int midCopies = 2;
            int width = leftTex.width + midTex.width * midCopies + rightTex.width;
            int height = Mathf.Max(leftTex.height, midTex.height, rightTex.height);
            var result = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var clear = new Color[width * height];
            for (int i = 0; i < clear.Length; i++)
                clear[i] = Color.clear;
            result.SetPixels(clear);

            Blit(result, leftTex, 0);
            int x = leftTex.width;
            for (int i = 0; i < midCopies; i++)
            {
                Blit(result, midTex, x);
                x += midTex.width;
            }

            Blit(result, rightTex, x);
            result.Apply();

            File.WriteAllBytes(Path.GetFullPath(outPath), result.EncodeToPNG());
            Object.DestroyImmediate(leftTex);
            Object.DestroyImmediate(midTex);
            Object.DestroyImmediate(rightTex);
            Object.DestroyImmediate(result);

            AssetDatabase.ImportAsset(outPath);
            var importer = AssetImporter.GetAtPath(outPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.spriteBorder = new Vector4(9, 4, 9, 4);
                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteMeshType = SpriteMeshType.FullRect;
                settings.spriteGenerateFallbackPhysicsShape = false;
                importer.SetTextureSettings(settings);
                importer.SaveAndReimport();
            }
        }

        static Texture2D LoadReadableTexture(string assetPath)
        {
            var bytes = File.ReadAllBytes(Path.GetFullPath(assetPath));
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (tex.LoadImage(bytes) == false)
            {
                Object.DestroyImmediate(tex);
                return null;
            }

            return tex;
        }

        static void Blit(Texture2D dest, Texture2D src, int destX)
        {
            for (int y = 0; y < src.height; y++)
            {
                for (int x = 0; x < src.width; x++)
                    dest.SetPixel(destX + x, y, src.GetPixel(x, y));
            }
        }

        [MenuItem("Tools/UI/Apply Elven Forge Theme")]
        public static void ApplyAll()
        {
            var theme = EnsureThemeAsset();
            if (theme.DisplayFont == null || theme.BodyFont == null)
            {
                Debug.LogError("[KenneyRpg] Missing Climate Crisis / Jura TMP fonts.");
                return;
            }

            int changed = 0;
            foreach (var path in PrefabPaths)
            {
                if (File.Exists(Path.GetFullPath(path)) == false && AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
                {
                    Debug.LogWarning($"[KenneyRpg] Prefab missing: {path}");
                    continue;
                }

                var root = PrefabUtility.LoadPrefabContents(path);
                foreach (var t in root.GetComponentsInChildren<Transform>(true))
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
                changed += ApplyToRoot(root, theme, path);
                PrefabUtility.SaveAsPrefabAsset(root, path);
                PrefabUtility.UnloadPrefabContents(root);
            }

            var active = EditorSceneManager.GetActiveScene().path;
            foreach (var scenePath in ScenePaths)
            {
                if (AssetDatabase.LoadAssetAtPath<Object>(scenePath) == null)
                {
                    Debug.LogWarning($"[KenneyRpg] Scene missing, skip: {scenePath}");
                    continue;
                }

                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                foreach (var root in scene.GetRootGameObjects())
                    changed += ApplyToRoot(root, theme, scenePath);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }

            if (string.IsNullOrEmpty(active) == false && AssetDatabase.LoadAssetAtPath<Object>(active) != null)
                EditorSceneManager.OpenScene(active, OpenSceneMode.Single);

            AssetDatabase.SaveAssets();
            Debug.Log($"[KenneyRpg] Theme applied. Touched ~{changed} components.");
        }

        public static int ApplyToRoot(GameObject root, ElvenForgeUiTheme theme, string context)
        {
            int n = 0;
            foreach (var tmp in root.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                if (ApplyTmp(tmp, theme))
                    n++;
            }

            foreach (var img in root.GetComponentsInChildren<Image>(true))
            {
                if (ApplyImage(img, theme))
                    n++;
            }

            return n;
        }

        static bool ApplyTmp(TextMeshProUGUI tmp, ElvenForgeUiTheme theme)
        {
            bool changed = false;
            bool isTitle = IsTitle(tmp);
            var font = isTitle ? theme.DisplayFont : theme.BodyFont;
            if (font != null && tmp.font != font)
            {
                tmp.font = font;
                changed = true;
            }

            var name = tmp.gameObject.name.ToLowerInvariant();
            var parent = tmp.transform.parent != null ? tmp.transform.parent.name.ToLowerInvariant() : "";
            Color target = theme.TextLight;

            if (isTitle)
                target = theme.Gold;
            else if (name.Contains("desc") || name.Contains("status") || parent.Contains("wave"))
                target = theme.TextMuted;
            else if (parent.Contains("reward") || name.Contains("reward") || name.Contains("опыт"))
                target = theme.TextDark;
            else if (tmp.fontSize >= 48)
                target = theme.Gold;

            if (ColorsDiffer(tmp.color, target))
            {
                tmp.color = target;
                changed = true;
            }

            if (changed)
                EditorUtility.SetDirty(tmp);
            return changed;
        }

        static bool ApplyImage(Image img, ElvenForgeUiTheme theme)
        {
            var go = img.gameObject;
            var name = go.name.ToLowerInvariant();
            var parent = go.transform.parent != null ? go.transform.parent.name.ToLowerInvariant() : "";
            var path = GetPath(go.transform).ToLowerInvariant();

            // Skip icons that should stay white for tinting / addressables
            if (name == "icon" || name.Contains("arrowimage") || name.Contains("handle") || name.Contains("knob")
                || name.Contains("placeholder") || name.Contains("skull") || name.Contains("star"))
                return false;

            Sprite sprite = null;
            Color color = Color.white;
            bool useSlice = true;

            if (name.Contains("shoppanel") || (name == "image" && parent.Contains("shoppanel")))
            {
                color = theme.DimOverlay;
                useSlice = false;
            }
            else if (IsPanelBackground(name, parent, path) || (name.Contains("window") && path.Contains("shop")))
            {
                sprite = theme.PanelModal;
            }
            else if (name == "bg" && parent.Contains("upgradecard"))
            {
                sprite = theme.PanelParchment;
            }
            else if (name.Contains("shopitemcard") || (name.Contains("card") && path.Contains("shop")) || name.Contains("iconarea"))
            {
                sprite = theme.PanelModal;
            }
            else if (IsRewardButton(name, parent, path))
            {
                sprite = theme.ButtonReward;
            }
            else if (IsPrimaryButton(name, parent, path) || name.Contains("shopbutton") || name.Contains("buybutton"))
            {
                sprite = theme.ButtonPrimary;
            }
            else if (IsDangerButton(name, parent, path) || name.Contains("closebutton"))
            {
                sprite = theme.ButtonDanger;
            }
            else if (IsSecondaryButton(name, parent, path))
            {
                sprite = theme.ButtonSecondary;
            }
            else if (name.Contains("pausebutton") || name.Contains("language") || name == "close")
            {
                sprite = theme.ButtonIcon;
            }
            else if (name.Contains("coincounter") || name.Contains("panelcoincounter") || name.Contains("chip"))
            {
                sprite = theme.ChipCoin;
            }
            else if (name.StartsWith("tab") && path.Contains("shop"))
            {
                sprite = theme.ButtonSecondary;
            }
            else if (name.Contains("fill") && (path.Contains("xp") || path.Contains("slider") || path.Contains("lvl") || path.Contains("exp") || path.Contains("progress")))
            {
                sprite = path.Contains("load") ? theme.SliderFillGold : theme.SliderFill;
            }
            else if (name.Contains("background") && (path.Contains("slider") || path.Contains("xp") || path.Contains("progress")))
            {
                sprite = theme.SliderTrack;
            }
            else if (name.Contains("panelup") || name.Contains("waveui") || (name == "panel" && path.Contains("canvas")))
            {
                sprite = theme.PanelHud;
            }
            else if (name.Contains("loadscreen") || (path.Contains("loadcanvas") && (name.Contains("background") || name == "image")))
            {
                sprite = theme.PanelModal;
            }
            else
            {
                return false;
            }

            bool changed = false;
            if (sprite != null && img.sprite != sprite)
            {
                img.sprite = sprite;
                changed = true;
            }

            if (useSlice && sprite != null && img.type != Image.Type.Sliced)
            {
                img.type = Image.Type.Sliced;
                img.pixelsPerUnitMultiplier = 1f;
                changed = true;
            }

            if (ColorsDiffer(img.color, color))
            {
                img.color = color;
                changed = true;
            }

            if (changed)
                EditorUtility.SetDirty(img);
            return changed;
        }

        static bool IsTitle(TextMeshProUGUI tmp)
        {
            var name = tmp.gameObject.name;
            if (TitleNames.Contains(name))
                return true;

            var lower = name.ToLowerInvariant();
            foreach (var key in TitleNameContains)
            {
                if (lower.Contains(key))
                    return true;
            }

            var parent = tmp.transform.parent != null ? tmp.transform.parent.name.ToLowerInvariant() : "";
            if (parent.Contains("winpanel") || parent.Contains("losepanel") || parent.Contains("pausepanel"))
            {
                if (lower.Contains("title") || tmp.fontSize >= 42)
                    return true;
            }

            if (parent.Contains("startmenu") && tmp.fontSize >= 40)
                return true;

            return false;
        }

        static bool IsPanelBackground(string name, string parent, string path)
        {
            if (name.Contains("winpanel") || name.Contains("losepanel") || name.Contains("pausepanel"))
                return true;
            if (name.Contains("upgradepanel") || name.Contains("upgradechoice"))
                return true;
            if ((name == "image" || name == "background" || name == "panel" || name == "bg") &&
                (parent.Contains("win") || parent.Contains("lose") || parent.Contains("pause") || parent.Contains("upgrade")))
                return true;
            if ((path.Contains("pausepanel") || path.Contains("winpanel") || path.Contains("losepanel")) &&
                (name == "bg" || name == "background" || name == "image" || name == "panel"))
                return true;
            return false;
        }

        static bool IsPrimaryButton(string name, string parent, string path)
        {
            if (name.Contains("resumebutton") || name.Contains("buttonnextlevel") || name.Contains("nextlevel"))
                return true;
            if (name.Contains("skipbutton") == false && name.Contains("продолжить"))
                return true;
            if ((name.Contains("button") || parent.Contains("button")) &&
                (name.Contains("next") || name.Contains("resume") || name.Contains("дальше") || name.Contains("продолжить")))
                return true;
            return false;
        }

        static bool IsSecondaryButton(string name, string parent, string path)
        {
            if (name.Contains("restart") || name.Contains("заново") || name.Contains("снова") || name.Contains("skip"))
                return true;
            if (name.Contains("buttonrestart"))
                return true;
            if ((name.Contains("button") || parent.Contains("button")) &&
                (name.Contains("again") || name.Contains("retry")))
                return true;
            return false;
        }

        static bool IsDangerButton(string name, string parent, string path)
        {
            return name.Contains("danger") || (name.Contains("close") && path.Contains("shop"));
        }

        static bool IsRewardButton(string name, string parent, string path)
        {
            if (name.Contains("reward") || name.Contains("reaward"))
                return true;
            if (name.Contains("adbutton") || name.Contains("rewardbutton") || name.StartsWith("ad_"))
                return true;
            if (parent.Contains("reward") || path.Contains("/reward") || path.Contains("reaward"))
                return true;
            return false;
        }

        static bool ColorsDiffer(Color a, Color b)
        {
            return Mathf.Abs(a.r - b.r) > 0.01f
                || Mathf.Abs(a.g - b.g) > 0.01f
                || Mathf.Abs(a.b - b.b) > 0.01f
                || Mathf.Abs(a.a - b.a) > 0.01f;
        }

        static string GetPath(Transform t)
        {
            var stack = new Stack<string>();
            while (t != null)
            {
                stack.Push(t.name);
                t = t.parent;
            }
            return string.Join("/", stack);
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;
            string parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
            string name = Path.GetFileName(path);
            if (string.IsNullOrEmpty(parent) == false && AssetDatabase.IsValidFolder(parent) == false)
                EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
#endif
