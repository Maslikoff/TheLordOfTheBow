using System.Collections;
using UnityEngine;
using TMPro;

namespace Game.Scripts.UI
{
    public class DamagePopup : MonoBehaviour
    {
        private const string FormatText = "F0";
        
        [SerializeField] private Canvas _canvas;
        [SerializeField] private TextMeshProUGUI _damageText;
        [SerializeField] private AnimationCurve _moveCurve;
        [SerializeField] private float _displayDuration = 0.5f;
        [SerializeField] private float _movePosition = 5f;
        
        private Coroutine _popupCoroutine;
        private Vector3 _startPosition;
        private Vector3 _endPosition;

        private void Awake()
        {
            _canvas.enabled = false;
            _startPosition = transform.localPosition;
            _endPosition = _startPosition + Vector3.up * _movePosition;
        }

        public void ShowDamage(float damage)
        {
            _damageText.text = damage.ToString(FormatText);
            
            if (_popupCoroutine != null)
                StopCoroutine(_popupCoroutine);
                
            _popupCoroutine = StartCoroutine(PopupRoutine());
        }
        
        public void ResetPopup()
        {
            if (_popupCoroutine != null)
            {
                StopCoroutine(_popupCoroutine);
                _popupCoroutine = null;
            }
            
            _canvas.enabled = false;
            transform.localPosition = _startPosition;
        }

        private IEnumerator PopupRoutine()
        {
            _canvas.enabled = true;
            transform.localPosition = _startPosition;
            
            float elapsedTime = 0f;
            
            while (elapsedTime < _displayDuration)
            {
                float t = elapsedTime / _displayDuration;
                float curveValue = _moveCurve.Evaluate(t);
                
                transform.localPosition = Vector3.Lerp(_startPosition, _endPosition, curveValue);
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            _canvas.enabled = false;
            _popupCoroutine = null;
        }
    }
}