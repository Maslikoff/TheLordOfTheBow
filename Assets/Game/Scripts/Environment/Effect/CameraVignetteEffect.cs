using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using VContainer;

namespace Game.Scripts.Environment.Effect
{
    public class CameraVignetteEffect : MonoBehaviour
    {
        [Header("Vignette Settings")]
        [SerializeField] private Volume _globalVolume;
        [SerializeField] private float _vignetteIntensity = 0.5f;
        [SerializeField] private float _vignetteDuration = 0.3f;
        [SerializeField] private float _vignetteFadeDuration = 0.5f;
        
        [Header("Colors")]
        [SerializeField] private Color _playerHitColor = Color.red;
        [SerializeField] private Color _enemyHitColor = Color.black;
        
        private Coroutine _vignetteCoroutine;
        private Vignette _vignette;
        private bool _isInitialized;
        
        [Inject]
        private void Construct(Volume globalVolume)
        {
            _globalVolume = globalVolume;
        }
        
        private void Awake()
        {
            Initialize();
        }
        
        private void Initialize()
        {
            _globalVolume.profile.TryGet(out _vignette);
            _vignette.active = true;
            _vignette.intensity.overrideState = true;
            _vignette.intensity.value = 0f;
            _vignette.color.overrideState = true;
            _vignette.color.value = Color.black;
            
            _isInitialized = true;
        }
        
        public void PlayPlayerHitVignette()
        {
            PlayVignette(_playerHitColor);
        }
        
        public void PlayEnemyHitVignette()
        {
            PlayVignette(_enemyHitColor);
        }
        
        private void PlayVignette(Color color)
        {
            if (_isInitialized == false)
                Initialize();
            
            if (_isInitialized == false || _vignette == null)
                return;
            
            if (_vignetteCoroutine != null)
                StopCoroutine(_vignetteCoroutine);
            
            _vignetteCoroutine = StartCoroutine(VignetteRoutine(color));
        }
        
        private IEnumerator VignetteRoutine(Color color)
        {
            _vignette.active = true;
            _vignette.color.value = color;
            _vignette.intensity.value = 0f;
            
            float elapsed = 0f;
            
            while (elapsed < _vignetteDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _vignetteDuration;
                _vignette.intensity.value = Mathf.Lerp(0f, _vignetteIntensity, t);
                
                yield return null;
            }
            
            _vignette.intensity.value = _vignetteIntensity;
            
            elapsed = 0f;
            
            while (elapsed < _vignetteFadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _vignetteFadeDuration;
                _vignette.intensity.value = Mathf.Lerp(_vignetteIntensity, 0f, t);
                
                yield return null;
            }
            
            _vignette.intensity.value = 0f;
        }
        
        private void OnDestroy()
        {
            DOTween.Kill(_vignette);
            
            if (_vignette != null)
                _vignette.intensity.value = 0f;
        }
    }
}