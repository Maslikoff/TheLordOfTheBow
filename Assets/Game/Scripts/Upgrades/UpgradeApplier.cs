using Game.Scripts.Characters;
using Game.Scripts.Characters.Bullets;
using UnityEngine;

namespace Game.Scripts.Upgrades
{
    public class UpgradeApplier : MonoBehaviour
    {
        private ShootEntity _playerShoot;
        private Health _playerHealth;
        
        private void Awake()
        {
            _playerShoot = GetComponent<ShootEntity>();
            _playerHealth = GetComponent<Health>();
        }

        public void ApplyUpgrade(UpgradeCard upgrade)
        {
            switch (upgrade.UpgradeType)
            {
                case UpgradeType.MaxHealth:
                    ApplyMaxHealthUpgrade(upgrade.Value);
                    break; 
                
                case UpgradeType.ArrowDamage:
                    ApplyArrowDamageUpgrade(upgrade.Value);
                    break; 
                
                case UpgradeType.FireArrowDamage:
                    ApplyFireArrowDamageUpgrade(upgrade.Value);
                    break;
                
                case UpgradeType.PoisonArrowDamage:
                    ApplyPoisonArrowDamageUpgrade(upgrade.Value);
                    break;
                
                case UpgradeType.ArrowCount:
                    ApplyArrowCountUpgrade(upgrade.Value);
                    break;
                
                case UpgradeType.FireArrowLifeTime:
                    ApplyFireArrowLifeTimeUpgrade(upgrade.Value);
                    break;
                
                case UpgradeType.PoisonArrowLifeTime:
                    ApplyPoisonArrowLifeTimeUpgrade(upgrade.Value);
                    break;
            }
        }
        
        private void ApplyMaxHealthUpgrade(float bonusHealth)
        {
            _playerHealth.IncreaseMaxHealth(bonusHealth);
            Debug.Log($"Max Health increased by {bonusHealth}");
        }

        private void ApplyArrowDamageUpgrade(float bonusDamage)
        {
            //_playerShoot.UpgradeBulletDamage(BulletType.Arrow, bonusDamage);
            Debug.Log($"Arrow damage increased by {bonusDamage}");
        }

        private void ApplyFireArrowDamageUpgrade(float bonusDamage)
        {
            //_playerShoot.UpgradeBulletDamage(BulletType.FireArrow, bonusDamage);
            Debug.Log($"Fire Arrow damage increased by {bonusDamage}");
        }

        private void ApplyPoisonArrowDamageUpgrade(float bonusDamage)
        {
            //_playerShoot.UpgradeBulletDamage(BulletType.PoisonArrow, bonusDamage);
            Debug.Log($"Poison Arrow damage increased by {bonusDamage}");
        }

        private void ApplyArrowCountUpgrade(float bonusCount)
        {
            //_playerShoot.UpgradeBulletCount(BulletType.Arrow, (int)bonusCount);
            Debug.Log($"Arrow count increased by {bonusCount}");
        }

        private void ApplyFireArrowLifeTimeUpgrade(float bonusLifeTime)
        {
            //_playerShoot.UpgradeBulletLifeTime(BulletType.FireArrow, bonusLifeTime);
            Debug.Log($"Fire Arrow LifeTime increased by {bonusLifeTime}");
        }

        private void ApplyPoisonArrowLifeTimeUpgrade(float bonusLifeTime)
        {
            //_playerShoot.UpgradeBulletLifeTime(BulletType.PoisonArrow, bonusLifeTime);
            Debug.Log($"Poison Arrow LifeTime increased by {bonusLifeTime}");
        }
    }
}