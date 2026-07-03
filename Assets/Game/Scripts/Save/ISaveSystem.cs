namespace Game.Scripts.Save
{
    public interface ISaveSystem
    {
        bool IsMetaDataLoaded { get; }
        void SaveGameData();
        void LoadGameData();
        void ManualSave();
    }
}