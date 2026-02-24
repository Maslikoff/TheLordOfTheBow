using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Characters.Health
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Health _health;
        
        [Header("Slider")]
        [SerializeField] private Slider _healthSlider;
        [SerializeField] private Image _fillImage;
        [SerializeField] private Gradient _healthGradient;
        [SerializeField] private float _smoothSpeed = 2f;
        [SerializeField] private bool _smoothChanges = true;
        
        [Header("Text")]
        [SerializeField] private TextMeshProUGUI _healthText;
        [SerializeField] private string _textFormat = "{0}/{1}";
        [SerializeField] private bool _showAsPercentage = false;

        private float _targetValue;

        private void Start()
        {
            _health.Changed += OnHealthChanged;
            _health.Death += OnDeath;

            _healthSlider.maxValue = _health.MaxCount;
            _healthSlider.value = _health.CurrentCount;
            _targetValue = _health.CurrentCount;

            UpdateHealthColor();
            UpdateText(_health.CurrentCount);
        }

        private void Update()
        {
            if(_smoothChanges && _healthSlider.value != _targetValue)
            {
                _healthSlider.value = Mathf.Lerp(_healthSlider.value, _targetValue, Time.deltaTime * _smoothSpeed);
                UpdateHealthColor();
                UpdateText(_health.CurrentCount);
            }
        }
        
        private void OnDestroy()
        {
            if (_health != null)
            {
                _health.Changed -= OnHealthChanged;
                _health.Death -= OnDeath;
            }
        }

        private void OnHealthChanged(int currentHealth)
        {
            _targetValue = currentHealth;

            if (_smoothChanges == false)
            {
                _healthSlider.value = currentHealth;
                UpdateHealthColor();
                UpdateText(currentHealth);
            }
        }

        private void UpdateHealthColor()
        {
            if (_fillImage != null)
            {
                float healthPercentage = _healthSlider.value / _healthSlider.maxValue;
                _fillImage.color = _healthGradient.Evaluate(healthPercentage);
            }
        }
        
        private void UpdateText(float currentHealth)
        {
            if (_healthText == null || _health == null) 
                return;
            
            if (_showAsPercentage)
            {
                float percentage = (currentHealth / _health.MaxCount) * 100f;
                _healthText.text = $"{percentage:F0}%";
            }
            else
            {
                _healthText.text = string.Format(_textFormat, 
                    Mathf.CeilToInt(currentHealth), 
                    Mathf.CeilToInt(_health.MaxCount));
            }
        }

        private void OnDeath()
        {
            _healthSlider.value = 0;
            UpdateHealthColor();
            UpdateText(0);
        }
    }
}