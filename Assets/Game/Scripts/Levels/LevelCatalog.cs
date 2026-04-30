using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts.Levels
{
    [CreateAssetMenu(fileName = "LevelCatalog", menuName = "Game/Levels/Level Catalog", order = 0)]
    public class LevelCatalog : ScriptableObject
    {
        [SerializeField] private List<LevelConfig> _levels;
        
        public int Count => _levels.Count;

        public bool TryGetLevel(int index, out LevelConfig level)
        {
            if (index >= 0 && index < Count)
            {
                level = _levels[index];
                return true;
            }
            
            level = null;
            return false;
        }
    }
}