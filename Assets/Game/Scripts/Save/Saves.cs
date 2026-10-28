using System;
using JTLStudio.SDK;

namespace Game.Scripts.Save
{
    /// <summary>
    /// Static access to the game save, backed by <see cref="JTLSDK.Data"/>.
    /// Replaces the former PluginYG2 <c>YG2.saves</c> / <c>YG2.SaveProgress()</c> surface.
    /// The save is stored as two logical documents instead of one blob:
    /// <c>campaign</c> (run state) and <c>profile</c> (wallet, stage pointer, cosmetics).
    /// </summary>
    public static class Saves
    {
        private const string CampaignKey = "campaign";
        private const string ProfileKey = "profile";

        private static SaveData _data;

        /// <summary>Raised after the cloud document is (re)loaded: start, player sign-in, retried load.</summary>
        public static event Action Loaded;

        /// <summary>Former <c>YG2.isSDKEnabled</c>: true once the Data module has loaded.</summary>
        public static bool IsReady => JTLSDK.IsCreated && JTLSDK.Data.IsReady;

        public static SaveData Data
        {
            get
            {
                if (_data == null) LoadFromSdk();
                return _data;
            }
        }

        /// <summary>Wired to <see cref="JTLSDK.Data"/>.Loaded by the SDK bootstrap.</summary>
        internal static void Reload()
        {
            LoadFromSdk();
            Loaded?.Invoke();
        }

        /// <summary>
        /// Former <c>YG2.SaveProgress()</c>. Pass <paramref name="important"/> = false for
        /// frequent in-run writes so they ride the next important save instead of waking autosave.
        /// </summary>
        public static void Save(bool important = true)
        {
            if (JTLSDK.IsCreated == false) return;

            SaveData data = Data;
            if (data.Campaign != null)
                JTLSDK.Data.SetObject(CampaignKey, data.Campaign, important);
            JTLSDK.Data.SetObject(ProfileKey, data.ExportProfile(), important);
            JTLSDK.Data.Save();
        }

#if UNITY_EDITOR
        /// <summary>Editor/test only: swaps the cached document without touching the cloud.</summary>
        public static void OverrideForEditorTests(SaveData data) => _data = data;
#endif

        private static void LoadFromSdk()
        {
            var data = new SaveData();

            if (JTLSDK.IsCreated && JTLSDK.Data.IsReady)
            {
                data.Campaign = JTLSDK.Data.GetObject<CampaignSaveData>(CampaignKey, null);
                data.ImportProfile(JTLSDK.Data.GetObject(ProfileKey, new SaveData.ProfileData()));
            }

            _data = data;
        }
    }
}
