using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.UI
{
    public class ExperienceUI : MonoBehaviour
    {
        [SerializeField] private Experience.Experience _playerExperience;

        [Header("UI References")] [SerializeField]
        private Slider _experienceSlider;

        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _experienceText;
        [SerializeField] private TextMeshProUGUI _nextLevelText;

        [Header("Animation")] [SerializeField] private float _sliderAnimationSpeed = 5f;

        private float _targetSliderValue;

        private void Start()
        {
            if (_playerExperience == null)
            {
                enabled = false;

                return;
            }

            _playerExperience.LevelUp += OnLevelUp;
            _playerExperience.ExperienceProgressChanged += OnExperienceProgressChanged;
            _playerExperience.ExperienceChanged += OnExperienceChanged;

            UpdateUI();
            UpdateProgressUI();
        }

        private void Update()
        {
            if (_experienceSlider != null && Mathf.Abs(_experienceSlider.value - _targetSliderValue) > 0.01f)
            {
                _experienceSlider.value = Mathf.Lerp(_experienceSlider.value, _targetSliderValue,
                    _sliderAnimationSpeed * Time.deltaTime);
            }
        }

        private void OnDestroy()
        {
            if (_playerExperience != null)
            {
                _playerExperience.LevelUp -= OnLevelUp;
                _playerExperience.ExperienceProgressChanged -= OnExperienceProgressChanged;
                _playerExperience.ExperienceChanged -= OnExperienceChanged;
            }
        }

        private void OnLevelUp(int newLevel)
        {
            UpdateUI();
            UpdateProgressUI();

            if (_levelText != null)
            {
                _levelText.transform.localScale = Vector3.one * 1.2f;

                StartCoroutine(AnimateLevelText());
            }
        }

        private void OnExperienceProgressChanged(float currentExp, float requiredExp)
        {
            if (requiredExp > 0 && _experienceSlider != null)
                _targetSliderValue = currentExp / requiredExp;
            
            UpdateProgressUI();
        }

        private void OnExperienceChanged(float newExperience)
        {
        }

        private void UpdateUI()
        {
            _levelText.text = $"LVL {_playerExperience.CurrentLevel}";
        }

        private void UpdateProgressUI()
        {
            float currentExp = _playerExperience.CurrentExperience;
            float requiredExp = _playerExperience.ExperienceForNextLevel;

            _experienceText.text = $"{Mathf.FloorToInt(currentExp)} / {Mathf.FloorToInt(requiredExp)} XP";
            _nextLevelText.text = $"Next: {Mathf.FloorToInt(requiredExp)} XP";
        }
        
        private System.Collections.IEnumerator AnimateLevelText()
        {
            float elapsed = 0;
            float duration = 0.3f;
            Vector3 startScale = Vector3.one * 1.2f;
            Vector3 endScale = Vector3.one;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                _levelText.transform.localScale = Vector3.Lerp(startScale, endScale, t);
                
                yield return null;
            }
            
            _levelText.transform.localScale = Vector3.one;
        }
    }
}