#if UNITY_EDITOR
using System.IO;
using Game.Scripts.Levels;
using Game.Scripts.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Editor
{
    public static class PauseUiSetup
    {
        const string PrefabPath = "Assets/Game/Prefabs/UI/PauseUI.prefab";
        const string Level1Path = "Assets/Game/Scenes/Level_1.unity";
        const string TutorialPath = "Assets/Game/Scenes/Level_Tutorial.unity";

        [MenuItem("Tools/Pause/Build Pause UI Prefab And Wire Scenes")]
        public static void BuildAndWire()
        {
            EnsureFolder("Assets/Game/Prefabs/UI");

            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) == null)
            {
                Debug.LogError("[PauseUiSetup] PauseUI prefab missing at " + PrefabPath
                    + ". Create it once from an existing PauseUI instance before wiring Tutorial.");
                return;
            }

            // Level_1 is intentionally not modified.
            WireScene(TutorialPath, replaceExistingSceneObjects: true);

            AssetDatabase.SaveAssets();
            Debug.Log("[PauseUiSetup] Done");
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            string parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
            string name = Path.GetFileName(path);
            if (string.IsNullOrEmpty(parent) == false && AssetDatabase.IsValidFolder(parent) == false)
                EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        private static void BuildPrefabFromLevel1()
        {
            var scene = EditorSceneManager.OpenScene(Level1Path, OpenSceneMode.Single);

            var existingRoot = GameObject.Find("PauseUI");
            if (existingRoot != null)
            {
                PrefabUtility.SaveAsPrefabAsset(existingRoot, PrefabPath);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                return;
            }

            var pauseButton = FindNamed("PauseButton");
            var pausePanel = FindNamed("PausePanel");
            if (pauseButton == null || pausePanel == null)
            {
                Debug.LogError("[PauseUiSetup] PauseButton or PausePanel not found in Level_1.");
                return;
            }

            var root = new GameObject("PauseUI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var rootRt = root.GetComponent<RectTransform>();
            StretchFull(rootRt);
            rootRt.localScale = Vector3.one;

            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1200;

            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            ReparentKeepWorld(pauseButton.transform, root.transform);
            ReparentKeepWorld(pausePanel.transform, root.transform);

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void WireScene(string scenePath, bool replaceExistingSceneObjects)
        {
            if (File.Exists(scenePath) == false)
            {
                Debug.LogWarning("[PauseUiSetup] Scene missing: " + scenePath);
                return;
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
            {
                Debug.LogError("[PauseUiSetup] Prefab missing: " + PrefabPath);
                return;
            }

            GameObject pauseUi = GameObject.Find("PauseUI");
            if (pauseUi == null)
            {
                DestroyNamed("PauseButton");
                DestroyNamed("PausePanel");

                pauseUi = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                pauseUi.name = "PauseUI";
            }
            else if (replaceExistingSceneObjects && PrefabUtility.GetCorrespondingObjectFromSource(pauseUi) == null)
            {
                Object.DestroyImmediate(pauseUi);
                DestroyNamed("PauseButton");
                DestroyNamed("PausePanel");

                pauseUi = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                pauseUi.name = "PauseUI";
            }
            else if (PrefabUtility.GetCorrespondingObjectFromSource(pauseUi) == null)
            {
                // Level_1 after extraction: connect instance to prefab.
                PrefabUtility.SaveAsPrefabAssetAndConnect(pauseUi, PrefabPath, InteractionMode.AutomatedAction);
                pauseUi = GameObject.Find("PauseUI");
            }

            WireLevelPanels(pauseUi);
            EnsureAutoInject(pauseUi);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void EnsureAutoInject(GameObject pauseUi)
        {
            var scopes = Object.FindObjectsOfType<Game.Scripts.DI.GameLifetimeScope>(true);
            if (scopes.Length == 0 || pauseUi == null)
                return;

            var so = new SerializedObject(scopes[0]);
            var list = so.FindProperty("autoInjectGameObjects");
            if (list == null || list.isArray == false)
                return;

            for (int i = 0; i < list.arraySize; i++)
            {
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == pauseUi)
                    return;
            }

            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = pauseUi;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(scopes[0]);
        }

        private static void WireLevelPanels(GameObject pauseUi)
        {
            var pauseButton = pauseUi.transform.Find("PauseButton")?.GetComponent<Button>();
            var pausePanel = pauseUi.transform.Find("PausePanel")?.gameObject;
            var pausePanelCg = pausePanel != null ? pausePanel.GetComponent<CanvasGroup>() : null;

            Button resume = null;
            Button restart = null;
            if (pausePanel != null)
            {
                foreach (var button in pausePanel.GetComponentsInChildren<Button>(true))
                {
                    if (button.gameObject.name == "ResumeButton")
                        resume = button;
                    else if (button.gameObject.name == "RestartButton")
                        restart = button;
                }
            }

            var tapToStart = Object.FindObjectOfType<TapToStartZone>(true);
            var panels = Object.FindObjectsOfType<LevelPanels>(true);
            if (panels.Length == 0)
            {
                Debug.LogWarning("[PauseUiSetup] LevelPanels not found.");
                return;
            }

            var so = new SerializedObject(panels[0]);
            so.FindProperty("_pausePanelCanvasGroup").objectReferenceValue = pausePanelCg;
            so.FindProperty("_pausePanel").objectReferenceValue = pausePanel;
            so.FindProperty("_buttonPause").objectReferenceValue = pauseButton;
            so.FindProperty("_buttonPauseResumeLevel").objectReferenceValue = resume;
            so.FindProperty("_buttonPauseRestartLevel").objectReferenceValue = restart;
            so.FindProperty("_tapToStart").objectReferenceValue = tapToStart;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(panels[0]);
        }

        private static GameObject FindNamed(string name)
        {
            foreach (var root in EditorSceneManager.GetActiveScene().GetRootGameObjects())
            {
                foreach (var t in root.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name == name)
                        return t.gameObject;
                }
            }

            return null;
        }

        private static void DestroyNamed(string name)
        {
            var go = FindNamed(name);
            if (go != null)
                Object.DestroyImmediate(go);
        }

        private static void ReparentKeepWorld(Transform child, Transform parent)
        {
            var rt = child as RectTransform;
            if (rt == null)
            {
                child.SetParent(parent, true);
                return;
            }

            Vector2 anchorMin = rt.anchorMin;
            Vector2 anchorMax = rt.anchorMax;
            Vector2 anchoredPos = rt.anchoredPosition;
            Vector2 sizeDelta = rt.sizeDelta;
            Vector2 pivot = rt.pivot;
            Vector3 scale = rt.localScale;
            Quaternion rot = rt.localRotation;

            rt.SetParent(parent, false);
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
            rt.localScale = scale;
            rt.localRotation = rot;
        }

        private static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.localScale = Vector3.one;
        }
    }
}
#endif
