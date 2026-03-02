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
        protected bool _isReloading;
        protected bool _canShoot = true;
    
        public event Action<float> ReloadProgressUpdated;
        public event Action ShotFired;
        
        protected abstract void OnShotFired();

        public virtual void TryShoot()
        {
            if (_canShoot == false || _isReloading) 
                return;
    
            if (_currentShotsInBurst < _maxShotsPerBurst)
            {
                _currentShotsInBurst++;
                OnShotFired();
                
                ShotFired?.Invoke();
    
                if (_currentShotsInBurst >= _maxShotsPerBurst)
                    StartReload();
            }
        }
        
        public virtual void ResetShootState()
        {
            _currentShotsInBurst = 0;
            _isReloading = false;
            _canShoot = true;
            StopAllCoroutines();
        }
    
        protected virtual void StartReload()
        {
            _isReloading = true;
            _canShoot = false;
            _currentShotsInBurst = 0;
    
            StartCoroutine(ReloadCoroutine());
        }
    
        protected virtual IEnumerator ReloadCoroutine()
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
