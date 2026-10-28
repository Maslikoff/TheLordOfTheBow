using Game.Localization;
using Game.Scripts.Save;
using JTLStudio.SDK;
using UnityEngine;

namespace Game.Scripts.Sdk
{
    /// <summary>
    /// Creates the JTL SDK once before the first scene loads. Replaces the PluginYG2 prefab
    /// that used to boot itself from Resources. GameReady is NOT called here: the game raises it
    /// explicitly from the first interactive screen (see TapToStartZone).
    /// </summary>
    public static class SdkBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Create()
        {
            if (JTLSDK.IsCreated) return;

            JTLSDK.Create();
            JTLSDK.Data.Loaded += Saves.Reload;
            Loc.Init();
        }
    }
}
