using Game.Scripts.Upgrades;
using UnityEngine;

namespace Game.Scripts.UpgradeCard
{
    [CreateAssetMenu(fileName = "MaxHealthUpgradeCard", menuName = "Game/Upgrades/Max Health")]
    public class MaxHealthUpgradeCard : Upgrades.UpgradeCard
    {
        [SerializeField] private float _bonusHealth;

        public override void Apply(PlayerUpgradeHolder playerUpgrateHolder)
        {
            if (playerUpgrateHolder.PlayerHealth == null)
                return;

            playerUpgrateHolder.PlayerHealth.IncreaseMaxHealth(_bonusHealth);
            Debug.Log($"Max Health increased by {_bonusHealth}");
        }
    }
}