using Cysharp.Threading.Tasks;

namespace Game.Scripts.Levels
{
    public interface ILevelService
    {
        LevelConfig CurrentConfig { get; }
        int CurrentLevel { get; }
        UniTask LoadCurrentLevelAsync();
        UniTask LoadLevelAsync(int index);
        UniTask LoadNextLevelAsync();
        UniTask RestartCurrentLevelAsync();
    }
}