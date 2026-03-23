using System.Collections;
using Game.Scripts.Characters.Bullets;
using Game.Scripts.Spawners;
using UnityEngine;

namespace Game.Scripts.Characters.Player
{
    public class PlayerShoot : ShootEntity
    {
        [SerializeField] private BulletSpawner _bulletSpawner;
        [SerializeField] private BulletType _bulletType;
        [SerializeField] private Transform _firePoint;
        [SerializeField] private bool _autoFire = false;
        
        protected void Start()
        {
            ResetShootState();
            
            if (_autoFire)
                StartCoroutine(AutoFireRoutine());
        }
        
        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private IEnumerator AutoFireRoutine()
        {
            WaitForSeconds wait = new WaitForSeconds(_cooldownTime);
            
            while (_autoFire && enabled && gameObject.activeInHierarchy)
            {
                TryShoot();
                
                yield return wait; 
            }
        }

        protected override void OnShotFired()
        {
            if (_bulletSpawner != null && _firePoint != null)
            {
                _bulletSpawner.SetFirePoint(_firePoint);
                _bulletSpawner.SetDirection(_firePoint.forward);
                _bulletSpawner.SpawnBullet();
            }
        }
    }
}