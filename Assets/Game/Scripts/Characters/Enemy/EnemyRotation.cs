using UnityEngine;

namespace Game.Scripts.Characters.Enemy
{
    public class EnemyRotation : MonoBehaviour
    {
        [SerializeField] private Transform _visualRoot;
        [SerializeField] private float _rotationSpeed = 5f;
        
        private Transform _target;
        private Enemy _enemy;

        private void Awake()
        {
            _enemy = GetComponent<Enemy>();
            _target = _enemy.PlayerTarget;
        }
        
        private void Update()
        {
            RotateTowardsTarget();
        }
        
        public void SetTarget(Transform target)
        {
            _target = target;
        }
        
        private void RotateTowardsTarget()
        {
            Vector3 direction = (_target.position - transform.position).normalized;
            
            if (direction == Vector3.zero) 
                return;
                
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            _visualRoot.rotation = Quaternion.Slerp(_visualRoot.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
        }
    }
}