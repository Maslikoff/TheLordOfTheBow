using System;
using Cysharp.Threading.Tasks;

namespace Game.Scripts.Levels
{
    public interface ISceneLoader
    {
        UniTask LoadAsync(SceneNames sceneName, SceneTransitionMode transitionMode = SceneTransitionMode.CloseAndOpen, Action onComplete = null);
    }
}