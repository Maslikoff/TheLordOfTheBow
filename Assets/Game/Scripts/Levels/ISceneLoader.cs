using Cysharp.Threading.Tasks;

namespace Game.Scripts.Levels
{
    public interface ISceneLoader
    {
        UniTask LoadAsync(SceneNames sceneName);
    }
}