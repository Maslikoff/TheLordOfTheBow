using System;
using UnityEngine;

namespace Game.Scripts.Experience
{
    public class Experience : MonoBehaviour
    {
        [Header("Experience Settings")]
        [SerializeField] private PlayerLevelXpConfig _levelXpConfig;
        [SerializeField] private float _startExperience = 0f;
        [SerializeField] private int _startLevel = 1;
        [SerializeField] private AnimationCurve _experienceCurve;
        
        private float _currentExperience;
        private int _currentLevel;
        private float _experienceForNextLevel;
        
        public float CurrentExperience => _currentExperience;
        public int CurrentLevel => _currentLevel;
        public float ExperienceForNextLevel => _experienceForNextLevel;
        public float ExperienceProgress => _currentExperience / _experienceForNextLevel;
        
        public event Action<int> LevelUp;
        public event Action<float> ExperienceChanged;
        public event Action<float, float> ExperienceProgressChanged;
        
        private void Awake()
        {
            _currentLevel = _startLevel;
            _currentExperience = _startExperience;
            
            UpdateExperienceForNextLevel();
        }
        
        private void Start()
        {
            ExperienceChanged?.Invoke(_currentExperience);
            ExperienceProgressChanged?.Invoke(_currentExperience, _experienceForNextLevel);
        }
        
        public void AddExperience(float amount)
        {
            if (amount <= 0) return;
            
            _currentExperience += amount;
            
            ExperienceChanged?.Invoke(_currentExperience);
            ExperienceProgressChanged?.Invoke(_currentExperience, _experienceForNextLevel);
            
            CheckLevelUp();
        }
        
        public void ResetExperience()
        {
            _currentLevel = _startLevel;
            _currentExperience = _startExperience;
            UpdateExperienceForNextLevel();
            
            ExperienceChanged?.Invoke(_currentExperience);
            ExperienceProgressChanged?.Invoke(_currentExperience, _experienceForNextLevel);
        }
        
        public (int level, float experience) GetSaveData() => (_currentLevel, _currentExperience);
        
        public void LoadSaveData(int level, float experience)
        {
            _currentLevel = level;
            _currentExperience = experience;
            
            UpdateExperienceForNextLevel();
            
            ExperienceChanged?.Invoke(_currentExperience);
            ExperienceProgressChanged?.Invoke(_currentExperience, _experienceForNextLevel);
        }
        
        public void RestoreTo(int level, float experience)
        {
            LoadSaveData(level, experience);
        }

        private void CheckLevelUp()
        {
            while (_currentExperience >= _experienceForNextLevel)
            {
                _currentExperience -= _experienceForNextLevel;
                _currentLevel++;
                
                UpdateExperienceForNextLevel();
                
                LevelUp?.Invoke(_currentLevel);
                
                ExperienceProgressChanged?.Invoke(_currentExperience, _experienceForNextLevel);
            }
            
            ExperienceChanged?.Invoke(_currentExperience);
        }

        private void UpdateExperienceForNextLevel()
        {
            if (_levelXpConfig != null)
            {
                _experienceForNextLevel = _levelXpConfig.GetXpRequiredForLevel(_currentLevel);
                return;
            }
            
            if (_experienceCurve != null && _experienceCurve.keys.Length > 0)
            {
                _experienceForNextLevel = _experienceCurve.Evaluate(_currentLevel);
                return;
            }
            
            _experienceForNextLevel = 100f * _currentLevel;
        }
    }
}