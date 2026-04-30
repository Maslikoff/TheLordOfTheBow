using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace Game.Scripts.Levels
{
    public class LevelDebug : MonoBehaviour
    {
        private ILevelService _levelService;

        [Inject]
        private void Construct(ILevelService levelService)
        {
            _levelService = levelService;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q))
                LoadPreviousLevel().Forget();

            if (Input.GetKeyDown(KeyCode.E))
                _levelService.LoadNextLevelAsync().Forget();
        }

        private async UniTaskVoid LoadPreviousLevel()
        {
            int previousIndex = _levelService.CurrentLevel - 1;
            if (previousIndex < 0)
                return;

            await _levelService.LoadLevelAsync(previousIndex);
        }
    }
}