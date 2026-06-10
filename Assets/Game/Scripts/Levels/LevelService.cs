using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Scripts.Levels
{
    public class LevelService : ILevelService
    {
        private readonly LevelCatalog _levelCatalog;
        private readonly ISceneLoader _sceneLoader;

        private int _currentLevelIndex;

        public LevelService(LevelCatalog levelCatalog, ISceneLoader sceneLoader)
        {
            _levelCatalog = levelCatalog;
            _sceneLoader = sceneLoader;
            
            _currentLevelIndex = 0;
        }
        
        public LevelConfig CurrentConfig { get; private set; }
        public int CurrentLevel => _currentLevelIndex + 1;

        public async UniTask LoadCurrentLevelAsync()
        {
            if (TryGetLevelConfig(_currentLevelIndex, out LevelConfig config) == false)
                return;
 
            CurrentConfig = config;
            
            Debug.Log($"Start current level index: {_currentLevelIndex}");
            
            await _sceneLoader.LoadAsync(config.SceneNames);
        }

        public async UniTask LoadLevelAsync(int index)
        {
            if (TryGetLevelConfig(index, out LevelConfig config) == false)
                return;
            
            _currentLevelIndex = index;
            CurrentConfig = config;
            
            Debug.Log($"Switch level: {_currentLevelIndex - 1} -> {_currentLevelIndex}");
            
            await _sceneLoader.LoadAsync(config.SceneNames);
        }
        
        public async UniTask LoadNextLevelAsync()
        {
            int count = _levelCatalog.Count;

            if (count <= 0)
                return;

            int nextIndex = (_currentLevelIndex + 1) % count;
            
            await LoadLevelAsync(nextIndex);
        }

        public UniTask OnRestartCurrentLevel()
        {
            throw new System.NotImplementedException();
        }

        public async UniTask RestartCurrentLevelAsync()
        {
            if (TryGetLevelConfig(_currentLevelIndex, out LevelConfig config) == false)
                return;
        
            Debug.Log($"Restart level: {_currentLevelIndex}");
        
            await _sceneLoader.LoadAsync(config.SceneNames);
            
            Time.timeScale = 1;
        }

        private bool TryGetLevelConfig(int index, out LevelConfig config)
        {
            if (index < 0)
            {
                config = null;
                return false;
            }

            return _levelCatalog.TryGetLevel(index, out config) && config != null;
        }
    }
}