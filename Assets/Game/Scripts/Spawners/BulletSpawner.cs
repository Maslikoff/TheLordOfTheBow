using Game.Scripts.Characters.Bullets;
using Game.Scripts.ObjectPool;
using UnityEngine;

namespace Game.Scripts.Spawners
{
    public class BulletSpawner : MonoBehaviour
    {
        [SerializeField] private BulletPool _bulletPool;
        [SerializeField] private Transform _bulletSpawnPoint;

        public void SpawnBullet(BulletType bulletType, Vector3 direction, BulletData bulletData)
        {
            if (_bulletPool == null)
                return;

            Bullet bullet = _bulletPool.GetBullet(bulletType);

            if (bullet != null)
            {
                bullet.transform.position = _bulletSpawnPoint != null ? _bulletSpawnPoint.position : transform.position;
                bullet.transform.rotation = Quaternion.LookRotation(direction);
                bullet.SetDirection(direction);
                bullet.Initialize(bulletData);
                bullet.gameObject.SetActive(true);
            }
        }

        public void SetFirePoint(Transform firePoint)
        {
            _bulletSpawnPoint = firePoint;
        }

        public void SetBulletPool(BulletPool pool)
        {
            _bulletPool = pool;
        }
    }
}