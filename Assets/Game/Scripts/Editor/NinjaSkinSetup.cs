#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Game.Scripts.Editor
{
    // Compatibility entry points for older setup commands and editor scripts.
    public static class NinjaSkinSetup
    {
        public const string Art = "Assets/Game/Art/Characters/Ninja";
        public const string Model = Art + "/Ninja@Run.fbx";
        public const string Prefab = "Assets/Game/Prefabs/Skins/Ninja.prefab";
        public const string Icon = "Assets/Game/Art/Shop/Icon_Ninja.png";

        [MenuItem("Tools/Shop/Setup Ninja Skin")]
        public static void Setup() => CharacterContentSetup.SetupAll();

        public static void ConfigureCatalog() => CharacterContentSetup.ConfigureCatalog();

        public static void RegisterAddressables() => CharacterContentSetup.RegisterAddressables();

        public static Bounds GeometryBounds(GameObject root) => CharacterContentSetup.GeometryBounds(root);
    }
}
#endif
