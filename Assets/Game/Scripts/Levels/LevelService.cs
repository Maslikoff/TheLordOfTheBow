using Cysharp.Threading.Tasks;
using Game.Scripts.Levels;
using UnityEngine;

namespace Game.Scripts.Levels
{
    public class LevelService : ILevelService
    {
        private readonly LevelCatalog _levelCatalog;
        private readonly ISceneLoader _sceneLoader;
        private readonly AddressablesContent.IAddressablesContentService _content;

        private int _currentLevelIndex;

        public LevelService(
            LevelCatalog levelCatalog,
            ISceneLoader sceneLoader,
            AddressablesContent.IAddressablesContentService content)
        {
            _levelCatalog = levelCatalog;
            _sceneLoader = sceneLoader;
            _content = content;
            _currentLevelIndex = 0;
        }

        public LevelConfig CurrentConfig { get; private set; }
        public int CurrentLevel => _currentLevelIndex + 1;
        public int CurrentLevelIndex => _currentLevelIndex;
        public int Cycle { get; private set; }
        public int UnlockedStage { get; private set; } = 1;

        public void SetCampaignPosition(int index, int cycle, int unlockedStage)
        {
            Cycle = Mathf.Max(0, cycle);
            UnlockedStage = Mathf.Clamp(Mathf.Max(unlockedStage, index + 1), 1, CampaignRules.StageCount);
            SetCurrentLevelIndex(index);
        }

        public async UniTask LoadCurrentLevelAsync()
        {
            LevelConfig config = await _levelCatalog.LoadLevelAsync(_currentLevelIndex, _content);
            if (config == null)
                return;

            CurrentConfig = config;
            Debug.Log($"Start current level index: {_currentLevelIndex}");
            await _sceneLoader.LoadAsync(config.SceneNames);
        }

        public async UniTask LoadLevelAsync(int index)
        {
            LevelConfig config = await _levelCatalog.LoadLevelAsync(index, _content);
            if (config == null)
                return;

            _currentLevelIndex = index;
            UnlockedStage = Mathf.Max(UnlockedStage, index + 1);
            CurrentConfig = config;

            Debug.Log($"Switch level: {_currentLevelIndex - 1} -> {_currentLevelIndex}");
            Time.timeScale = 1;

            await _sceneLoader.LoadAsync(config.SceneNames);
        }

        public async UniTask LoadNextLevelAsync()
        {
            int count = _levelCatalog.Count;
            if (count <= 0)
                return;

            var next = CampaignRules.Next(_currentLevelIndex, Cycle);
            Cycle = next.cycle;
            await LoadLevelAsync(next.index);
        }

        public async UniTask RestartCurrentLevelAsync()
        {
            LevelConfig config = await _levelCatalog.LoadLevelAsync(_currentLevelIndex, _content);
            if (config == null)
                return;

            CurrentConfig = config;
            Debug.Log($"Restart level: {_currentLevelIndex}");
            Time.timeScale = 1;
            await _sceneLoader.LoadAsync(config.SceneNames);
        }

        public void SetCurrentLevelIndex(int index)
        {
            if (index < 0 || index >= _levelCatalog.Count)
                return;

            _currentLevelIndex = index;

            if (_levelCatalog.TryGetLevel(index, out LevelConfig config))
                CurrentConfig = config;
        }

        public void AdvanceToNextLevelIndex()
        {
            int count = _levelCatalog.Count;
            if (count <= 0)
                return;

            var next = CampaignRules.Next(_currentLevelIndex, Cycle);
            SetCampaignPosition(next.index, next.cycle, Mathf.Max(UnlockedStage, next.index + 1));
        }
    }
}
