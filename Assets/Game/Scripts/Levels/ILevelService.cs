using Cysharp.Threading.Tasks;

namespace Game.Scripts.Levels
{
    public interface ILevelService
    {
        LevelConfig CurrentConfig { get; }
        int CurrentLevel { get; }
        int CurrentLevelIndex { get; }
        int Cycle { get; }
        int UnlockedStage { get; }
        void SetCampaignPosition(int index, int cycle, int unlockedStage);
        void SetCurrentLevelIndex(int index);
        UniTask LoadCurrentLevelAsync();
        UniTask LoadLevelAsync(int index);
        UniTask LoadNextLevelAsync();
        UniTask RestartCurrentLevelAsync();
    }
}
