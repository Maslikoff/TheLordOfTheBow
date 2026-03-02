using Game.Scripts.Characters.Bullets;
using Game.Scripts.Characters.Enemy;
using Game.Scripts.ObjectPool;
using UnityEngine;

namespace Game.Scripts.Spawners
{
    public class BulletSpawner : Spawner<Bullet>
    {
        [SerializeField] private Transform _bulletSpawnPoint;
        [SerializeField] private Vector3 _direction = Vector3.forward;

        protected override void SpawnObject()
        {
            SpawnBullet();
        }

        public void SpawnBullet()
        {
            Bullet bullet = _objectPool.GetFromPool();

            if (bullet != null)
            {
                bullet.transform.position = _bulletSpawnPoint != null ? _bulletSpawnPoint.position : transform.position;
                bullet.transform.rotation = Quaternion.LookRotation(_direction);
                bullet.gameObject.SetActive(true);

                bullet.Released += OnBulletReleased;

                IncreaseObjectCount();
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