using Game.Scripts.Characters.Bullets;
using Game.Scripts.Upgrades;
using UnityEngine;

namespace Game.Scripts.UpgradeCard
{
    [CreateAssetMenu(fileName = "BulletUpgradeCard", menuName = "Game/Upgrades/Bullet")]
    public class BulletUpgradeCard : Upgrades.UpgradeCard
    {
        [SerializeField] private BulletType _bulletType;
        [SerializeField] private BulletUpgradeType _bulletUpgradeType;
        [SerializeField] private float _value;
        
        public override void Apply(PlayerUpgradeHolder playerUpgradeHolder)
        {
            if (playerUpgradeHolder.Collection == null)
                return;

            switch (_bulletUpgradeType)
            {
                case BulletUpgradeType.Damage:
                    playerUpgradeHolder.Collection.AddDamage(_bulletType, _value);
                    break;

                case BulletUpgradeType.LifeTime:
                    playerUpgradeHolder.Collection.AddLifeTime(_bulletType, _value);
                    break;

                case BulletUpgradeType.Count:
                    playerUpgradeHolder.Collection.AddCount(_bulletType, Mathf.RoundToInt(_value));
                    break;
            }
        }
    }
}