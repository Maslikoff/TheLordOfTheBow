using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Scripts.AddressablesContent;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game.Scripts.Levels
{
    [CreateAssetMenu(fileName = "LevelCatalog", menuName = "Game/Levels/Level Catalog", order = 0)]
    public class LevelCatalog : ScriptableObject
    {
        [SerializeField] private List<AssetReferenceT<LevelConfig>> _levels = new();

        private readonly Dictionary<int, LevelConfig> _loadedLevels = new();

        public int Count => _levels.Count;

        public bool TryGetLevel(int index, out LevelConfig level)
        {
            if (_loadedLevels.TryGetValue(index, out level) && level != null)
                return true;

            level = null;
            return false;
        }

        public async UniTask<LevelConfig> LoadLevelAsync(int index, IAddressablesContentService content)
        {
            if (index < 0 || index >= _levels.Count)
                return null;

            if (_loadedLevels.TryGetValue(index, out LevelConfig cached) && cached != null)
                return cached;

            AssetReferenceT<LevelConfig> reference = _levels[index];
            LevelConfig level = await content.LoadAssetAsync<LevelConfig>(reference);
            await level.ResolveDependenciesAsync(content);
            _loadedLevels[index] = level;
            return level;
        }

        public async UniTask PreloadAllAsync(IAddressablesContentService content)
        {
            for (int i = 0; i < _levels.Count; i++)
                await LoadLevelAsync(i, content);
        }
    }
}
