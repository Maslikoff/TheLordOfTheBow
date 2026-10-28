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
        int BeginStage(Game.Scripts.Characters.Player.Player player);
        void RestartStage();
        void RecordDefeat();
        void RestoreAfterRevive();
    }
}
