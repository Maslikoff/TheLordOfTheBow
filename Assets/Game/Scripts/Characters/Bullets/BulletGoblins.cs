using System;
using Game.Scripts.PoolSystem;
using UnityEngine;

namespace Game.Scripts.Characters.Bullets
{
    public class BulletGoblins: Bullet
    {
        [SerializeField] private float _damage = 10f;
        
        protected override void MoveBullet()
        {
            _rigidbody.velocity = -transform.forward * _speed;
        }
        
        protected override bool CanCollide(Collision other)
        {
            if (other.gameObject.TryGetComponent(out Enemy.Enemy _))
                return false;
                
            return base.CanCollide(other);
        }
    }
}