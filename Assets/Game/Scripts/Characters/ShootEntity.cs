using System;
using System.Collections;
using Game.Scripts.Characters.Bullets;
using Game.Scripts.Spawners;
using UnityEngine;

namespace Game.Scripts.Characters
{
    public abstract class ShootEntity : MonoBehaviour
    {
        [SerializeField] protected BulletSpawner _bulletSpawner;
        [SerializeField] protected BulletType _bulletType;
        [SerializeField] protected Transform _firePoint;
        [SerializeField] protected float _cooldownTime = 1f;
        [SerializeField] protected int _maxShotsPerBurst = 1;
    
        protected int _currentShotsInBurst;
        protected float _lastShotTime;
        protected bool _isReloading;
        protected bool _canShoot = true;
    
        public event Action<float> ReloadProgressUpdated;
        public event Action ShotFired;
        
        protected virtual void Start()
        {
            ResetShootState();
        }
        
        public virtual void Update()
        {
            TryShoot();
        }
        
         protected void TryShoot()
        {
            if (enabled == false || _canShoot == false || _isReloading || _bulletSpawner == null || _firePoint == null) 
                return;
            
            float timeSinceLastShot = Time.time - _lastShotTime;
            float requiredCooldown = _cooldownTime / _maxShotsPerBurst;
    
            if (timeSinceLastShot < requiredCooldown)
                return;
            
            _lastShotTime = Time.time;
            
            if (_currentShotsInBurst < _maxShotsPerBurst)
            {
                _currentShotsInBurst++;
                
                Vector3 direction = GetShootDirection();
                _bulletSpawner.SetFirePoint(_firePoint);
                _bulletSpawner.SpawnBullet(_bulletType, direction);
                
                ShotFired?.Invoke();
    
                if (_currentShotsInBurst >= _maxShotsPerBurst)
                    StartReload();
            }
        }
        
        protected abstract Vector3 GetShootDirection();
        
        public void ResetShootState()
        {
            _currentShotsInBurst = 0;
            _lastShotTime = 0;
            _isReloading = false;
            _canShoot = true;
            StopAllCoroutines();
        }

        private void StartReload()
        {
            _isReloading = true;
            _canShoot = false;
            _currentShotsInBurst = 0;
            StartCoroutine(ReloadCoroutine());
        }

        private IEnumerator ReloadCoroutine()
        {
            float elapsedTime = 0f;
            
            while (elapsedTime < _cooldownTime)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / _cooldownTime;
                ReloadProgressUpdated?.Invoke(progress);
                yield return null;
            }
            
            _isReloading = false;
            _canShoot = true;
            ReloadProgressUpdated?.Invoke(1f);
        }
    }
}
