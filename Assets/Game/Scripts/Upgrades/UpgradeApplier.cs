using UnityEngine;

namespace Game.Scripts.Upgrades
{
    [RequireComponent(typeof(PlayerUpgradeHolder))]
    public class UpgradeApplier : MonoBehaviour
    {
        private PlayerUpgradeHolder _playerUpgradeHolder;
        
        private void Awake()
        {
            _playerUpgradeHolder = GetComponent<PlayerUpgradeHolder>();
        }

        public void ApplyUpgrade(UpgradeCard upgrade)
        {
            if(upgrade == null)
                return;
            
            upgrade.Apply(_playerUpgradeHolder);
        }
    }
}