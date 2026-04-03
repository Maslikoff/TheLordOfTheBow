using System;
using System.Collections.Generic;
using Game.Scripts.Characters.Bullets;
using Game.Scripts.Upgrades;
using UnityEngine;

namespace Game.Scripts.Characters.Player
{
    public class PlayerShoot : ShootEntity
    {
        private readonly Dictionary<BulletType, float> _lastShootTimeByType = new();
        private readonly Dictionary<BulletType, float> _cooldownByType = new();
        
        [SerializeField] private PlayerBulletUpgradeCollection _bulletUpgrades;
        [SerializeField] private List<BulletShootSettings> _bulletShootSettings;
        [SerializeField] private float _arrowSpreadAngle = 45f;
        [SerializeField] private bool _autoFire = false;

        public event Action ShotArrow;
        public event Action ShotFireArrow;
        public event Action ShotPoisonArrow;
        public event Action<float> ArrowReloadProgress;
        public event Action<float> FireArrowReloadProgress;
        public event Action<float> PoisonArrowReloadProgress;
        
        
        private void Awake()
        {
            InitializeDictionaries();
        }
        
        public override void Update()
        {
            UpdateAllReloadProgress();
            
            if (_autoFire)
                TryShoot();
        }

        protected override Vector3 GetShootDirection() => _firePoint.forward;

        private void InitializeDictionaries()
        {
            foreach (var setting in _bulletShootSettings)
            {
                if (_cooldownByType.ContainsKey(setting.BulletType))
                    continue;
                    
                _cooldownByType.Add(setting.BulletType, setting.ShootDelay);
                _lastShootTimeByType.Add(setting.BulletType, -setting.ShootDelay);
            }
        }

        protected override void TryShoot()
        {
            if (enabled == false || _canShoot == false || _isReloading || _bulletSpawner == null || _firePoint == null)
                return;
            
            PerformShot();
        }
        
        protected override void PerformShot()
        {
            if (_bulletUpgrades == null)
                return;
            
            if (_bulletSpawner == null || _firePoint == null)
                return;

            _bulletSpawner.SetFirePoint(_firePoint);

            Vector3 baseDirection = GetShootDirection();

            TryShootArrow(baseDirection);
            TryShootSpecial(BulletType.FireArrow, baseDirection);
            TryShootSpecial(BulletType.PoisonArrow, baseDirection);
        }
        
        private void UpdateAllReloadProgress()
        {
            UpdateReloadProgressForType(BulletType.Arrow, ArrowReloadProgress);
            UpdateReloadProgressForType(BulletType.FireArrow, FireArrowReloadProgress);
            UpdateReloadProgressForType(BulletType.PoisonArrow, PoisonArrowReloadProgress);
        }
        
        private void UpdateReloadProgressForType(BulletType type, Action<float> reloadEvent)
        {
            if (reloadEvent == null) 
                return;
        
            if (_cooldownByType.TryGetValue(type, out float cooldown) &&
                _lastShootTimeByType.TryGetValue(type, out float lastTime))
            {
                float timeSinceLast = Time.time - lastTime;
                
                if (timeSinceLast < cooldown)
                {
                    float progress = timeSinceLast / cooldown;
                    reloadEvent?.Invoke(progress);
                }
                else
                {
                    reloadEvent?.Invoke(1f);
                }
            }
        }

        private bool CanShoot(BulletType type)
        {
            if (_cooldownByType.TryGetValue(type, out float cooldown) == false)
                return false;

            if (_lastShootTimeByType.TryGetValue(type, out float last) == false)
                return false;

            return Time.time - last >= cooldown;
        }
        
        private void TryShootArrow(Vector3 baseDirection)
        {
            PlayerBulletUpgradeEntry arrowEntry = _bulletUpgrades.Get(BulletType.Arrow);

            if (arrowEntry == null || arrowEntry.IsUnlocked == false)
                return;

            if (CanShoot(BulletType.Arrow) == false)
                return;

            BulletData shotData = new BulletData(arrowEntry.Damage, 0f);

            int arrowCount = arrowEntry.Count;

            if (arrowCount <= 1)
            {
                _bulletSpawner.SpawnBullet(BulletType.Arrow, baseDirection, shotData);
            }
            else
            {
                float[] angles = BuildAngles(arrowCount, _arrowSpreadAngle);

                for (int i = 0; i < angles.Length; i++)
                {
                    Vector3 direction = Quaternion.AngleAxis(angles[i], _firePoint.up) * baseDirection;
                    _bulletSpawner.SpawnBullet(BulletType.Arrow, direction, shotData);
                }
            }

            MarkShot(BulletType.Arrow);
            ActionShootForType(BulletType.Arrow);
        }

        private void TryShootSpecial(BulletType bulletType, Vector3 direction)
        {
            PlayerBulletUpgradeEntry entry = _bulletUpgrades.Get(bulletType);

            if (entry == null || entry.IsUnlocked == false)
                return;

            if (CanShoot(bulletType) == false)
                return;

            BulletData shotData = new BulletData(entry.Damage, entry.LifeTime);
            _bulletSpawner.SpawnBullet(bulletType, direction, shotData);

            MarkShot(bulletType);
            ActionShootForType(bulletType);
        }
        
        private void MarkShot(BulletType type)
        {
            if (_lastShootTimeByType.ContainsKey(type))
                _lastShootTimeByType[type] = Time.time;
        }

        private void ActionShootForType(BulletType type)
        {
            switch (type)
            {
                case BulletType.Arrow:
                    ShotArrow?.Invoke();
                    break;
                
                case BulletType.FireArrow:
                    ShotFireArrow?.Invoke();
                    break;
                
                case BulletType.PoisonArrow:
                    ShotPoisonArrow?.Invoke();
                    break;
            }
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