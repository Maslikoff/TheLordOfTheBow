using UnityEngine;

namespace Game.Scripts.Characters.Enemy
{
    public class EnemyShoot : ShootEntity
    {
        private Enemy _enemy;

        protected override void Start()
        {
            base.Start();

            _enemy = GetComponent<Enemy>();
            HideFirePointDecorativeVisuals();
        }
        
        private void HideFirePointDecorativeVisuals()
        {
            if (_firePoint == null)
                return;
            
            Renderer[] renderers = _firePoint.GetComponentsInChildren<Renderer>(true);
            
            foreach (Renderer renderer in renderers)
            {
                if (renderer != null)
                    renderer.enabled = false;
            }
        }

        protected override Vector3 GetShootDirection()
        {
            if (_enemy?.PlayerTarget != null)
                return (_enemy.PlayerTarget.position - _firePoint.position).normalized;
            
            return transform.forward;
        }
        
        protected override Transform GetShotOwner() => transform;
    }
}
