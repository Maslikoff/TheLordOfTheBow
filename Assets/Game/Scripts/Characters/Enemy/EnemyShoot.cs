using UnityEngine;

namespace Game.Scripts.Characters.Enemy
{
    public class EnemyShoot : ShootEntity
    {
        private Enemy _enemy;

        protected override  void Start()
        {
            base.Start();

            _enemy = GetComponent<Enemy>();
        }

        protected override Vector3 GetShootDirection()
        {
            if (_enemy?.PlayerTarget != null)
                return (_enemy.PlayerTarget.Transform.position - _firePoint.position).normalized;
            
            return transform.forward;
        }
    }
}