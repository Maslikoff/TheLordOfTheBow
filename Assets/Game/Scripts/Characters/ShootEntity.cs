using System;
using System.Collections;
using UnityEngine;

namespace Game.Scripts.Characters
{
    public abstract class ShootEntity : MonoBehaviour
    {
        [SerializeField] protected float _cooldownTime = 1f;
        [SerializeField] protected int _maxShotsPerBurst = 1;
    
        protected int _currentShotsInBurst;
        protected float _lastShotTime;
        
        protected bool _isReloading;
        protected bool _canShoot = true;
    
        public event Action<float> ReloadProgressUpdated;
        public event Action ShotFired;
        
        protected abstract void OnShotFired();

        protected void TryShoot()
        {
            if (enabled == false || _canShoot == false || _isReloading) 
                return;
            
            float timeSinceLastShot = Time.time - _lastShotTime;
            float requiredCooldown = _cooldownTime / _maxShotsPerBurst;
    
            if (timeSinceLastShot < requiredCooldown)
                return;
            
            _lastShotTime = Time.time;
            
            if (_currentShotsInBurst < _maxShotsPerBurst)
            {
                _currentShotsInBurst++;
                OnShotFired();
                
                ShotFired?.Invoke();
    
                if (_currentShotsInBurst >= _maxShotsPerBurst)
                    StartReload();
            }
        }
        
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
