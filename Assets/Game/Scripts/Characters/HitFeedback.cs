using DG.Tweening;
using Game.Scripts.Environment.Effect;
using UnityEngine;
using VContainer;

namespace Game.Scripts.Characters
{
    public class HitFeedback : MonoBehaviour
    {
        [Header("Type")]
        [SerializeField] private HitFeedbackType _feedbackType = HitFeedbackType.Enemy;
        
        [Header("Knockback Settings")]
        [SerializeField] private float _knockbackDistance = 0.5f;
        [SerializeField] private float _knockbackDuration = 0.2f;
        [SerializeField] private Ease _knockbackEase = Ease.OutQuad;
        
        [Header("Flash Settings")]
        [SerializeField] private float _flashDuration = 0.5f;
        [SerializeField] private int _flashCount = 2;
        
        [Header("Camera Shake (Player Only)")]
        [SerializeField] private float _shakeDuration = 0.2f;
        [SerializeField] private float _shakeStrength = 0.3f;
        [SerializeField] private int _shakeVibrato = 10;
        
        private Health _health;
        private Renderer[] _renderers;
        private Color[] _originalColors;
        private Tweener _knockbackTween;
        private Sequence _flashSequence;
        private Camera _mainCamera;
        
        private ICameraVignetteService _vignetteService;
        
        [Inject]
        public void Construct(ICameraVignetteService vignetteService)
        {
            _vignetteService = vignetteService;
        }
        
        private void Awake()
        {
            _health = GetComponent<Health>();
            _mainCamera = Camera.main;
            
            CacheRenderers();
        }
        
        private void OnEnable()
        {
            if (_health != null)
                _health.DamageTaken += OnDamageTaken;
        }
        
        private void OnDisable()
        {
            if (_health != null)
                _health.DamageTaken -= OnDamageTaken;
            
            KillAllTweens();
        }
        
        private void CacheRenderers()
        {
            _renderers = GetComponentsInChildren<Renderer>(true);
            _originalColors = new Color[_renderers.Length];
            
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null)
                    _originalColors[i] = _renderers[i].material.color;
            }
        }
        
        private void OnDamageTaken(float damage)
        {
            PlayKnockback();
            PlayFlashEffect();
            PlayVignette();
            
            if (_feedbackType == HitFeedbackType.Player)
                PlayCameraShake();
        }
        
        private void PlayVignette()
        {
            if (_vignetteService == null) 
                return;
            
            if (_feedbackType == HitFeedbackType.Player)
                _vignetteService.PlayPlayerHitVignette();
            else
                _vignetteService.PlayEnemyHitVignette();
        }
        
        private void PlayKnockback()
        {
            Vector3 knockbackDirection = GetKnockbackDirection();
            Vector3 targetPosition = transform.position + knockbackDirection * _knockbackDistance;
            Vector3 originalPosition = transform.position;
            
            _knockbackTween?.Kill();
            
            _knockbackTween = transform
                .DOMove(targetPosition, _knockbackDuration * 0.5f)
                .SetEase(_knockbackEase)
                .OnComplete(() =>
                {
                    _knockbackTween = transform
                        .DOMove(originalPosition, _knockbackDuration * 0.5f)
                        .SetEase(Ease.OutQuad);
                });
        }
        
        private Vector3 GetKnockbackDirection()
        {
            if (_feedbackType == HitFeedbackType.Player)
                return GetDirectionFromNearestEnemy();
            else
                return GetDirectionFromPlayer();
        }
        
        private Vector3 GetDirectionFromPlayer()
        {
            Player.Player player = FindObjectOfType<Player.Player>();
            
            if (player != null)
            {
                Vector3 direction = transform.position - player.transform.position;
                direction.y = 0;
                
                return direction.normalized;
            }
            
            return GetRandomDirection();
        }
        
        private Vector3 GetDirectionFromNearestEnemy()
        {
            Enemy.Enemy[] enemies = FindObjectsOfType<Enemy.Enemy>();
            Enemy.Enemy closestEnemy = null;
            float closestDistance = float.MaxValue;
            
            foreach (var enemy in enemies)
            {
                if (enemy == null || enemy.gameObject.activeSelf == false) 
                    continue;
                
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy;
                }
            }
            
            if (closestEnemy != null)
            {
                Vector3 direction = transform.position - closestEnemy.transform.position;
                direction.y = 0;
                    
                return direction.normalized;
            }
            
            return GetRandomDirection();
        }
        
        private Vector3 GetRandomDirection() => new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
        
        private void PlayFlashEffect()
        {
            _flashSequence?.Kill();
            _flashSequence = DOTween.Sequence();
            
            float singleFlashDuration = _flashDuration / (_flashCount * 2);
            Color flashColor = GetFlashColor();
            
            for (int i = 0; i < _flashCount; i++)
            {
                _flashSequence.AppendCallback(() => SetColor(flashColor));
                _flashSequence.AppendInterval(singleFlashDuration);
                
                _flashSequence.AppendCallback(() => ResetColors());
                _flashSequence.AppendInterval(singleFlashDuration);
            }
            
            _flashSequence.OnComplete(ResetColors);
        }
        
        private Color GetFlashColor() => _feedbackType == HitFeedbackType.Player ? Color.red : Color.white;
        
        private void PlayCameraShake()
        {
            if (_mainCamera != null)
                _mainCamera.transform.DOShakePosition(_shakeDuration, _shakeStrength, _shakeVibrato);
            
        }
        
        private void SetColor(Color color)
        {
            foreach (var renderer in _renderers)
            {
                if (renderer != null)
                    renderer.material.color = color;
            }
        }
        
        private void ResetColors()
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null)
                    _renderers[i].material.color = _originalColors[i];
            }
        }
        
        private void KillAllTweens()
        {
            _knockbackTween?.Kill();
            _knockbackTween = null;
            
            _flashSequence?.Kill();
            _flashSequence = null;
            
            ResetColors();
        }
        
        private void OnDestroy()
        {
            KillAllTweens();
        }
    }
}