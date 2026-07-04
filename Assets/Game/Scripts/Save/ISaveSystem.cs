namespace Game.Scripts.Save
{
    public interface ISaveSystem
    {
        bool IsMetaDataLoaded { get; }
        void SaveGameData();
        void SavePlayerProgress();
        void LoadGameData();
        void ManualSave();
    }
}