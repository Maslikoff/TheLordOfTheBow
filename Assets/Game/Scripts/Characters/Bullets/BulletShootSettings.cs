using System;
using UnityEngine;

namespace Game.Scripts.Characters.Bullets
{
    [Serializable]
    public class BulletShootSettings
    {
        [SerializeField] private BulletType _bulletType;
        [SerializeField] private float _shootDelay = 3f;
        
        public BulletType BulletType => _bulletType;
        public float ShootDelay => _shootDelay;
    }
}