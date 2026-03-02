using System.Collections;
using Game.Scripts.Spawners;
using UnityEngine;

namespace Game.Scripts.Characters.Player
{
    public class PlayerShoot : ShootEntity
    {
        private const float MinimumDelay = 0.1f;
        
        [SerializeField] private BulletSpawner _bulletSpawner;
        [SerializeField] private Transform _firePoint;
        [SerializeField] private bool _autoFire = false;

        private Coroutine _autoFireCoroutine;
        
        protected void Start()
        {
            _autoFireCoroutine = StartCoroutine(AutoFireRoutine());
        }

        private IEnumerator AutoFireRoutine()
        {
            while (_autoFire)
            {
                TryShoot();
                yield return new WaitForSeconds(MinimumDelay); 
            }
            _autoFireCoroutine = null;
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