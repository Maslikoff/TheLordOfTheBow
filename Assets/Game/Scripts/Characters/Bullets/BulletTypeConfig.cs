using System;
using UnityEngine;

namespace Game.Scripts.Characters.Bullets
{
    [Serializable]
    public struct BulletTypeConfig
    {
        [SerializeField] private BulletType _type;
        [SerializeField] private Bullet _prefab;
        [SerializeField] private int _poolSize;

        public BulletType Type => _type;
        public Bullet Prefab => _prefab;
        public int PoolSize => _poolSize;
    }
}