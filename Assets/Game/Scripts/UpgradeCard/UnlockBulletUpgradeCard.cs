using Game.Scripts.Characters.Bullets;
using Game.Scripts.Upgrades;
using UnityEngine;

namespace Game.Scripts.UpgradeCard
{
    [CreateAssetMenu(fileName = "UnlockBulletUpgradeCard", menuName = "Game/Upgrades/Unlock Bullet")]
    public class UnlockBulletUpgradeCard : Upgrades.UpgradeCard
    {
        [SerializeField] private BulletType _bulletTypeToUnlock;

        public override void Apply(PlayerUpgradeHolder playerUpgrateHolder)
        {
            if (playerUpgrateHolder.Collection == null)
                return;

            playerUpgrateHolder.Collection.Unlock(_bulletTypeToUnlock);
        }
    }
}