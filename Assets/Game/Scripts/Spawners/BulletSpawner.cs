using Game.Scripts.Characters.Bullets;
using Game.Scripts.ObjectPool;
using UnityEngine;

namespace Game.Scripts.Spawners
{
    public class BulletSpawner : Spawner<Bullet>
    {
        [SerializeField] private Transform _bulletSpawnPoint;
        [SerializeField] private Vector3 _direction = Vector3.forward;
        
        [Header("Bullet Type Settings")]
        [SerializeField] private bool _useMultipleBulletTypes = false;
        [SerializeField] private BulletType _singleBulletType = BulletType.Arrow;
        [SerializeField] private BulletType[] _multipleBulletTypes;

        private BulletPool _bulletPool;
        private int _currentBulletIndex = 0;
        
        protected override void Initialize()
        {
            base.Initialize();
            
            _bulletPool = _objectPool as BulletPool;
        }
        
        protected override void SpawnObject()
        {
            SpawnBullet();
        }

        public void SpawnBullet()
        {
            if (_objectPool == null)
            {
                Debug.LogError("ObjectPool is not initialized!");
                return;
            }

            Bullet bullet = GetBulletFromPool();

            if (bullet != null)
            {
                bullet.transform.position = _bulletSpawnPoint != null ? _bulletSpawnPoint.position : transform.position;
                bullet.transform.rotation = Quaternion.LookRotation(_direction);
                bullet.gameObject.SetActive(true);
                bullet.SetDirection(_direction);

                bullet.Released += OnBulletReleased;
                
                IncreaseObjectCount();

                if (_useMultipleBulletTypes && _multipleBulletTypes.Length > 0)
                    _currentBulletIndex = (_currentBulletIndex + 1) % _multipleBulletTypes.Length;
            }
        }

        public void SetDirection(Vector3 direction)
        {
            _direction = direction.normalized;
        }

        public void SetFirePoint(Transform bulletSpawnPoint)
        {
            _bulletSpawnPoint = bulletSpawnPoint;
        }
        
        private Bullet GetBulletFromPool()
        {
            if (_bulletPool == null)
                return _objectPool.GetFromPool();

            if (_useMultipleBulletTypes && _multipleBulletTypes.Length > 0)
                return _bulletPool.GetBullet(_multipleBulletTypes[_currentBulletIndex]);
            else
                return _bulletPool.GetBullet(_singleBulletType);
        }

        private void OnBulletReleased(IPoolable poolable)
        {
            if (poolable is Bullet bullet)
            {
                bullet.Released -= OnBulletReleased;
                DecreaseObjectCount();
            }
        }
    }
}