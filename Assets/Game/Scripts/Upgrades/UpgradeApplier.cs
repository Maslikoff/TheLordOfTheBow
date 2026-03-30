using Game.Scripts.Characters;
using Game.Scripts.Characters.Bullets;
using UnityEngine;

namespace Game.Scripts.Upgrades
{
    public class UpgradeApplier : MonoBehaviour
    {
        [SerializeField] private Arrow _arrow;
        [SerializeField] private FireArrow _fireArrow;
        [SerializeField] private PoisonArrow _poisonArrow;
        
        private Health _playerHealth;
        
        private void Awake()
        {
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
            _arrow.UpgradeDamage(bonusDamage);
            Debug.Log($"Arrow damage increased by {bonusDamage}");
        }

        private void ApplyFireArrowDamageUpgrade(float bonusDamage)
        {
            _fireArrow.UpgradeDamage(bonusDamage);
            Debug.Log($"Fire Arrow damage increased by {bonusDamage}");
        }

        private void ApplyPoisonArrowDamageUpgrade(float bonusDamage)
        {
            _poisonArrow.UpgradeDamage(bonusDamage);
            Debug.Log($"Poison Arrow damage increased by {bonusDamage}");
        }

        private void ApplyArrowCountUpgrade(float bonusCount)
        {
            _arrow.UpgradeArrowCount();
            Debug.Log($"Arrow count increased by {bonusCount}");
        }

        private void ApplyFireArrowLifeTimeUpgrade(float bonusLifeTime)
        {
            _fireArrow.UpgradeLifeTime(bonusLifeTime);
            Debug.Log($"Fire Arrow LifeTime increased by {bonusLifeTime}");
        }

        private void ApplyPoisonArrowLifeTimeUpgrade(float bonusLifeTime)
        {
            _poisonArrow.UpgradeLifeTime(bonusLifeTime);
            Debug.Log($"Poison Arrow LifeTime increased by {bonusLifeTime}");
        }
    }
}