#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Game.Scripts.Editor
{
    /// <summary>
    /// Bakes Tutorial UI so Play Mode cannot revert prefab instances.
    /// </summary>
    [InitializeOnLoad]
    public static class LockTutorialUi
    {
        const string TutorialPath = "Assets/Game/Scenes/Level_Tutorial.unity";
        const string PrefKey = "Game.LockTutorialUi.Pending";

        static LockTutorialUi()
        {
            if (SessionState.GetBool(PrefKey, false))
            {
                SessionState.SetBool(PrefKey, false);
                EditorApplication.delayCall += Lock;
            }
        }

        [MenuItem("Tools/UI/Lock Tutorial UI (Prevent Play Reset)")]
        public static void Lock()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                SessionState.SetBool(PrefKey, true);
                EditorApplication.isPlaying = false;
                Debug.Log("[LockTutorialUi] Exit Play Mode first — lock will run after stop.");
                return;
            }

            var active = EditorSceneManager.GetActiveScene().path;
            var scene = EditorSceneManager.OpenScene(TutorialPath, OpenSceneMode.Single);

            int unpacked = UnpackUiPrefabs(scene);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            if (string.IsNullOrEmpty(active) == false && active != TutorialPath
                && AssetDatabase.LoadAssetAtPath<Object>(active) != null)
            {
                EditorSceneManager.OpenScene(active, OpenSceneMode.Single);
            }

            Debug.Log($"[LockTutorialUi] Unpacked {unpacked} UI prefab(s). Scene saved.");
        }

        static int UnpackUiPrefabs(UnityEngine.SceneManagement.Scene scene)
        {
            var toUnpack = new List<GameObject>();
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var t in root.GetComponentsInChildren<Transform>(true))
                {
                    var go = t.gameObject;
                    if (PrefabUtility.IsOutermostPrefabInstanceRoot(go) == false)
                        continue;

                    string assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go);
                    bool isUiPrefab = assetPath.Replace('\\', '/')
                        .IndexOf("/Prefabs/UI/", System.StringComparison.OrdinalIgnoreCase) >= 0;
                    if (isUiPrefab == false
                        && go.name != "ShopUI"
                        && go.name != "PauseUI"
                        && go.name != "LanguageButton"
                        && go.name != "UpgradeCard"
                        && go.name != "LoadCanvas")
                        continue;

                    toUnpack.Add(go);
                }
            }

            foreach (var go in toUnpack)
            {
                PrefabUtility.UnpackPrefabInstance(
                    go,
                    PrefabUnpackMode.Completely,
                    InteractionMode.AutomatedAction);
            }

            return toUnpack.Count;
        }
    }
}
#endif
