using System;
using Game.Scripts.Audio;
using Game.Scripts.Characters.Bullets;
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
    [RequireComponent(typeof(PoisonStatus))]
    [RequireComponent(typeof(HitFeedback))]
    public abstract class Enemy : MonoBehaviour, IPoolable
    {
        [SerializeField] protected Race _race;
        [SerializeField] protected EnemyShoot _enemyShoot;
        [SerializeField] protected EnemyRotation _enemyRotation;
        [SerializeField] protected Health _health;
        [SerializeField] protected DamagePopup _damagePopup;
        [SerializeField] protected PoisonStatus _poisonStatus;
        
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

        public event Action<IPoolable> Released;
        public bool WasKilled { get; private set; }
        private bool _released;
        private float _baseHealth;
        private float _baseXp;

        public void PrepareForSpawn(int cycle)
        {
            _released = false;
            WasKilled = false;
            var reward = GetComponent<Game.Scripts.Experience.ExperienceReward>();
            if (_baseHealth <= 0)
            {
                _baseHealth = _health.MaxCount;
                _baseXp = reward != null ? reward.Reward : 0;
            }
            float hp = _baseHealth * Game.Scripts.Levels.CampaignRules.EnemyHealth(cycle);
            _health.Restore(hp, hp);
            _enemyShoot.DamageMultiplier = Game.Scripts.Levels.CampaignRules.EnemyDamage(cycle);
            reward?.SetExperienceReward(Mathf.Round(_baseXp * Game.Scripts.Levels.CampaignRules.Experience(cycle)));
        }

        protected virtual void OnEnable()
        {
            _enemyShoot?.ResetShootState();
            _damagePopup?.ResetPopup();
            
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
            _poisonStatus ??= GetComponent<PoisonStatus>();
        }

        protected virtual void OnDisable()
        {
            if (_health != null)
            {
                _health.DamageTaken -= OnDamageTaken;
                _health.Death -= OnDeath;
            }

            _poisonStatus?.Clear();
            _damagePopup?.ResetPopup();
        }

        public void Initialize(Transform playerTarget, BulletSpawner bulletSpawner)
        {
            _audioService?.PlayOneShot(_spawnSound);
            PlayerTarget = playerTarget;
            _enemyRotation.SetTarget(playerTarget);
            
            if (_enemyShoot != null && bulletSpawner != null)
                _enemyShoot.Initialize(bulletSpawner);
        }
        
        public void Release()
        {
            if (_released) return;
            _released = true;
            EnemyBulletTracker.ReleaseAllForOwner(transform);
            Released?.Invoke(this);
        }
        
        private void OnDamageTaken(float damage)
        {
            _damagePopup?.ShowDamage(damage);
        }

        private void OnDeath()
        {
            WasKilled = true;
            _effectService?.PlayEffect(EffectType.EnemyDeath, transform.position);
            _audioService?.PlayOneShot(_deathSound);
            
            Release();
        }
    }
}
