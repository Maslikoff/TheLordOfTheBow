using UnityEngine;

namespace Game.Scripts.Experience
{
    [CreateAssetMenu(fileName = "PlayerLevelXpConfig", menuName = "Game/Experience/Player Level XP Config")]
    public class PlayerLevelXpConfig : ScriptableObject
    {
        [Header("Формула: round(BaseOffset + Scale × Level^Exponent)")]
        [SerializeField] private float _baseOffset = 167f;
        [SerializeField] private float _scale = 33f;
        [SerializeField] private float _exponent = 1.25f;
        
        [Header("Ограничения")]
        [SerializeField] private int _minXp = 150;
        [SerializeField] private int _maxXp = 9999;
        
        public float GetXpRequiredForLevel(int currentLevel)
        {
            if (currentLevel < 1)
                currentLevel = 1;
            float raw = _baseOffset + _scale * Mathf.Pow(currentLevel, _exponent);
            return Mathf.Clamp(Mathf.Round(raw), _minXp, _maxXp);
        }
        
#if UNITY_EDITOR
        [ContextMenu("Preview XP Table (1-15)")]
        private void PreviewTable()
        {
            for (int level = 1; level <= 15; level++)
                Debug.Log($"LVL {level} → {level + 1}: {GetXpRequiredForLevel(level)} XP");
        }
#endif
    }
}