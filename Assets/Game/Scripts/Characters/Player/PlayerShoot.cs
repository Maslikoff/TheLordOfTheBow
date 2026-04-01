using Game.Scripts.Characters.Bullets;
using Game.Scripts.Upgrades;
using UnityEngine;

namespace Game.Scripts.Characters.Player
{
    public class PlayerShoot : ShootEntity
    {
        [SerializeField] private PlayerBulletUpgradeCollection _bulletUpgrades;
        [SerializeField] private float _arrowSpreadAngle = 45f;
        [SerializeField] private bool _autoFire = false;
        
        public override void Update()
        {
            if (_autoFire)
                base.Update();
        }
        
        protected override Vector3 GetShootDirection() => _firePoint.forward;
        
        protected override void PerformShot()
        {
            if (_bulletUpgrades == null)
                return;

            _bulletSpawner.SetFirePoint(_firePoint);

            Vector3 baseDirection = GetShootDirection();

            ShootArrow(baseDirection);
            ShootIfUnlocked(BulletType.FireArrow, baseDirection);
            ShootIfUnlocked(BulletType.PoisonArrow, baseDirection);
        }

        private void ShootArrow(Vector3 baseDirection)
        {
            PlayerBulletUpgradeEntry arrowEntry = _bulletUpgrades.Get(BulletType.Arrow);

            if (arrowEntry.IsUnlocked == false)
                return;

            BulletData shotData = new BulletData(arrowEntry.Damage, 0f);

            int arrowCount = arrowEntry.Count;

            if (arrowCount <= 1)
            {
                _bulletSpawner.SpawnBullet(BulletType.Arrow, baseDirection, shotData);
                return;
            }

            float[] angles = BuildAngles(arrowCount, _arrowSpreadAngle);

            for (int i = 0; i < angles.Length; i++)
            {
                Vector3 direction = Quaternion.AngleAxis(angles[i], _firePoint.up) * baseDirection;
                _bulletSpawner.SpawnBullet(BulletType.Arrow, direction, shotData);
            }
        }

        private void ShootIfUnlocked(BulletType bulletType, Vector3 direction)
        {
            PlayerBulletUpgradeEntry entry = _bulletUpgrades.Get(bulletType);

            if (entry.IsUnlocked == false)
                return;

            BulletData shotData = new BulletData(entry.Damage, entry.LifeTime);
            _bulletSpawner.SpawnBullet(bulletType, direction, shotData);
        }

        private float[] BuildAngles(int count, float spreadAngle)
        {
            if (count == 1)
                return new[] { 0f };

            if (count == 2)
                return new[] { -spreadAngle / 2f, spreadAngle / 2f };

            if (count == 3)
                return new[] { -spreadAngle, 0f, spreadAngle };

            float[] angles = new float[count];
            float step = spreadAngle / (count - 1);
            float start = -spreadAngle / 2f;

            for (int i = 0; i < count; i++)
                angles[i] = start + step * i;

            return angles;
        }
    }
}