using System;
using Game.Scripts.Audio;
using UnityEngine;
using Game.Scripts.ObjectPool;
using Game.Scripts.Spawners;
using Game.Scripts.UI;
using Game.Scripts.Environment.Effect;
using VContainer;

namespace Game.Scripts.Characters.Enemy
{
    [RequireComponent(typeof(EnemyShoot))]
    [RequireComponent(typeof(EnemyRotation))]
    [RequireComponent(typeof(Health))]
    public abstract class Enemy : MonoBehaviour, IPoolable
    {
        [SerializeField] protected Race _race;
        [SerializeField] protected EnemyShoot _enemyShoot;
        [SerializeField] protected EnemyRotation _enemyRotation;
        [SerializeField] protected Health _health;
        [SerializeField] protected DamagePopup _damagePopup;
        
        [Header("Sounds")]
        [SerializeField] private AudioAsset _spawnSound;
        [SerializeField] private AudioAsset _deathSound;
        
        private IEffectService _effectService;
        private IAudioService _audioService;

        [Inject]
        public void Construct(IEffectService effectService, IAudioService audioService)
        {
            _effectService = effectService?? throw new ArgumentNullException(nameof(effectService));
            _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
        }
        
        public Race RaceEnemy => _race;
        public Transform PlayerTarget { get; protected set; }
        public BulletSpawner Bullets { get; protected set; }

        public event Action<IPoolable> Released;

        protected virtual void OnEnable()
        {
            if (_enemyShoot != null)
                _enemyShoot.ResetShootState();
            
            if (_damagePopup != null)
                _damagePopup.ResetPopup();
            
            if (_health != null)
            {
                _health.DamageTaken += OnDamageTaken;
                _health.Death += OnDeath;
            }
        }

        private void OnValidate()
        {
            _enemyShoot ??= GetComponent<EnemyShoot>();
            _enemyRotation ??= GetComponent<EnemyRotation>();
            _health ??= GetComponent<Health>();
        }

        protected virtual void OnDisable()
        {
            if (_health != null)
            {
                _health.DamageTaken -= OnDamageTaken;
                _health.Death -= OnDeath;
            }
            
            if (_damagePopup != null)
                _damagePopup.ResetPopup();
        }

        public void Initialize(Transform playerTarget)
        {
            _audioService?.PlayOneShot(_spawnSound);
            PlayerTarget = playerTarget;
            _enemyRotation.SetTarget(playerTarget);
        }
        
        public void Release()
        {
            Released?.Invoke(this);
        }
        
        private void OnDamageTaken(float damage)
        {
            if (_damagePopup != null)
                _damagePopup.ShowDamage(damage);
        }

        private void OnDeath()
        {
            _effectService?.PlayEffect(EffectType.EnemyDeath, transform.position);
            _audioService?.PlayOneShot(_deathSound);
            
            Release();
        }
    }
}