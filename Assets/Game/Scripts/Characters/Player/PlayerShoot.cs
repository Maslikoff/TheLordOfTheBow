using UnityEngine;

namespace Game.Scripts.Characters.Player
{
    public class PlayerShoot : ShootEntity
    {
        [SerializeField] private bool _autoFire = false;
        
        public override void Update()
        {
            if (_autoFire)
                base.Update();
        }
        
        protected override Vector3 GetShootDirection() => _firePoint.forward;
    }
}