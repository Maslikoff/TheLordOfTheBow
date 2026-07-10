namespace Game.Scripts.Save
{
    public interface ISaveSystem
    {
        bool IsMetaDataLoaded { get; }
        void SaveGameData();
        void SavePlayerProgress();
        void LoadGameData();
        void ManualSave();
        void SaveWaveCheckpoint(int levelIndex, int waveIndex);
        int GetWaveCheckpointOrDefault(int levelIndex);
        void ClearWaveCheckpoint(int levelIndex);
        void CommitSessionProgress();
    }
}