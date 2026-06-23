using System;

namespace Game.Scripts.Upgrades
{
    [Serializable]
    public class BulletUpgradeState
    {
        public bool IsUnlocked;
        public float DamageBonus;
        public float LifeTimeBonus;
        public int CountBonus;

        public BulletUpgradeState(bool isUnlocked, float damageBonus = 0, float lifeTimeBonus = 0, int countBonus = 0)
        {
            IsUnlocked = isUnlocked;
            DamageBonus = damageBonus;
            LifeTimeBonus = lifeTimeBonus;
            CountBonus = countBonus;
        }
    }
}