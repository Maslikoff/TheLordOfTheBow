using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Game.Scripts.Levels
{
    public class SceneLoader : ISceneLoader
    {
        public async UniTask LoadAsync(SceneNames sceneName)
        {
            await SceneManager.LoadSceneAsync(sceneName.ToString());
        }
    }
}