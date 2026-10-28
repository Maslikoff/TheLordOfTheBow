using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Game.Scripts.Environment.Effect;
using UnityEngine;
using VContainer;

namespace Game.Scripts.Characters
{
    public class HitFeedback : MonoBehaviour
    {
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
        private static readonly Color EnemyFlashColor = new Color(1.5f, 1.5f, 1.5f, 1f);
        private static readonly Color PlayerFlashColor = new Color(1.5f, 0f, 0f, 1f);

        [Header("Type")]
        [SerializeField] private HitFeedbackType _feedbackType = HitFeedbackType.Enemy;
        
        [Header("Knockback Settings")]
        [SerializeField] private float _knockbackDistance = 0.5f;
        [SerializeField] private float _knockbackDuration = 0.2f;
        [SerializeField] private Ease _knockbackEase = Ease.OutQuad;
        
        [Header("Flash Settings")]
        [SerializeField] [Min(0.01f)] private float _flashDuration = 0.12f;
        [SerializeField] [Min(1)] private int _flashCount = 1;
        
        [Header("Camera Shake (Player Only)")]
        [SerializeField] private float _shakeDuration = 0.2f;
        [SerializeField] private float _shakeStrength = 0.3f;
        [SerializeField] private int _shakeVibrato = 10;
        
        private Health _health;
        private readonly List<FlashTarget> _flashTargets = new List<FlashTarget>();
        private Tweener _knockbackTween;
        private Coroutine _flashCoroutine;
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
            _flashTargets.Clear();

            foreach (Renderer renderer in GetComponentsInChildren<Renderer>(true))
            {
                if (renderer is not MeshRenderer && renderer is not SkinnedMeshRenderer)
                    continue;

                Material[] materials = renderer.sharedMaterials;
                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    Material material = materials[materialIndex];
                    if (material == null || !material.HasProperty(EmissionColorId))
                        continue;

                    _flashTargets.Add(new FlashTarget(renderer, materialIndex,
                        material.GetColor(EmissionColorId)));
                }
            }
        }

        public void RefreshRenderers()
        {
            StopFlash();
            CacheRenderers();
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
            StopFlash();
            SetFlash(GetFlashColor());
            _flashCoroutine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            int flashCount = Mathf.Max(1, _flashCount);
            float flashDuration = Mathf.Max(0.01f, _flashDuration);

            if (flashCount == 1)
            {
                yield return new WaitForSeconds(flashDuration);
            }
            else
            {
                float phaseDuration = flashDuration / (flashCount * 2f);
                var wait = new WaitForSeconds(phaseDuration);

                for (int i = 0; i < flashCount; i++)
                {
                    if (i > 0)
                        SetFlash(GetFlashColor());

                    yield return wait;
                    ResetFlash();
                    yield return wait;
                }
            }

            ResetFlash();
            _flashCoroutine = null;
        }
        
        private Color GetFlashColor() => _feedbackType == HitFeedbackType.Player
            ? PlayerFlashColor
            : EnemyFlashColor;
        
        private void PlayCameraShake()
        {
            if (_mainCamera != null)
                _mainCamera.transform.DOShakePosition(_shakeDuration, _shakeStrength, _shakeVibrato);
            
        }
        
        private void SetFlash(Color color)
        {
            foreach (FlashTarget target in _flashTargets)
            {
                if (target.Renderer == null)
                    continue;

                target.Renderer.GetPropertyBlock(target.PropertyBlock, target.MaterialIndex);
                target.PropertyBlock.SetColor(EmissionColorId, color);
                target.Renderer.SetPropertyBlock(target.PropertyBlock, target.MaterialIndex);
            }
        }
        
        private void ResetFlash()
        {
            foreach (FlashTarget target in _flashTargets)
            {
                if (target.Renderer == null)
                    continue;

                target.Renderer.GetPropertyBlock(target.PropertyBlock, target.MaterialIndex);
                target.PropertyBlock.SetColor(EmissionColorId, target.OriginalEmissionColor);
                target.Renderer.SetPropertyBlock(target.PropertyBlock, target.MaterialIndex);
            }
        }
        
        private void KillAllTweens()
        {
            _knockbackTween?.Kill();
            _knockbackTween = null;

            StopFlash();
        }

        private void StopFlash()
        {
            if (_flashCoroutine != null)
            {
                StopCoroutine(_flashCoroutine);
                _flashCoroutine = null;
            }

            ResetFlash();
        }
        
        private void OnDestroy()
        {
            KillAllTweens();
        }

        private sealed class FlashTarget
        {
            public FlashTarget(Renderer renderer, int materialIndex, Color originalEmissionColor)
            {
                Renderer = renderer;
                MaterialIndex = materialIndex;
                OriginalEmissionColor = originalEmissionColor;
                PropertyBlock = new MaterialPropertyBlock();
            }

            public Renderer Renderer { get; }
            public int MaterialIndex { get; }
            public Color OriginalEmissionColor { get; }
            public MaterialPropertyBlock PropertyBlock { get; }
        }
    }
}
