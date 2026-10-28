namespace Game.Scripts.Characters.Bullets
{
    public struct BulletData
    {
        public BulletData(float damage, float lifeTime, float poisonDamage = 0f, int pierce = 0, bool critical = false)
        {
            Damage = damage;
            LifeTime = lifeTime;
            PoisonDamage = poisonDamage;
            Pierce = pierce;
            Critical = critical;
        }

        public float Damage { get; private set; }
        public float LifeTime { get; private set; }
        public float PoisonDamage { get; private set; }
        public int Pierce { get; private set; }
        public bool Critical { get; private set; }
    }
}
