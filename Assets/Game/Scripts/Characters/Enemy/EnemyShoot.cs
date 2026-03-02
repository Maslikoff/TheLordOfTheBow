using System;
using System.Collections;
using Game.Scripts.Characters.Bullets;
using Game.Scripts.ObjectPool;
using Game.Scripts.Spawners;
using UnityEngine;

namespace Game.Scripts.Characters.Enemy
{
    public class EnemyShoot : ShootEntity
    {
        private const float MinimumDelay = 0.1f;
        
        [SerializeField] private Transform _firePoint;
        [SerializeField] private BulletPool _bulletPool;

        private Coroutine _autoFireCoroutine;
        private Enemy _enemy;
        
        protected void Start()
        {
            _enemy = GetComponent<Enemy>();
            
            _autoFireCoroutine = StartCoroutine(AutoFireRoutine());
        }

        private void OnDisable()
        {
            if (_autoFireCoroutine != null)
            {
                StopCoroutine(_autoFireCoroutine);
                _autoFireCoroutine = null;
            }
        }

        private IEnumerator AutoFireRoutine()
        {
            WaitForSeconds wait = new WaitForSeconds(MinimumDelay);
            
            while (enabled)
            {
                TryShoot();
                yield return wait; 
            }
            
            _autoFireCoroutine = null;
        }
        
        protected override void OnShotFired()
        {
            if (_bulletPool == null || _firePoint == null || _enemy?.PlayerTarget == null) 
                return;

            Bullet bullet = _bulletPool.GetFromPool();
            
            if (bullet != null)
            {
                Vector3 direction = (_enemy.PlayerTarget.position - _firePoint.position).normalized;
                
                bullet.transform.position = _firePoint.position;
                bullet.transform.rotation = Quaternion.LookRotation(direction);
                bullet.SetDirection(direction);
                
                bullet.gameObject.SetActive(true);
            }
        }
    }
}