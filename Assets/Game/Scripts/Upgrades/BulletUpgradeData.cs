using System;
using Game.Scripts.Characters.Bullets;

namespace Game.Scripts.Upgrades
{
    [Serializable]
    public class BulletUpgradeData
    {
        public BulletType BulletType;
        public bool IsUnlocked;
        public float DamageBonus;
        public float LifeTimeBonus;
        public int CountBonus;
    }
}