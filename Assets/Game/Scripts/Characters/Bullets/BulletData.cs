namespace Game.Scripts.Characters.Bullets
{
    public struct BulletData
    {
        public BulletData(float damage, float lifeTime)
        {
            Damage = damage;
            LifeTime = lifeTime;
        }
        
        public float Damage { get; private  set; }
        public float LifeTime { get; private set; }
    }
}