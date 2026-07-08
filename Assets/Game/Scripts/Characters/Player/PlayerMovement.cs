using UnityEngine;

namespace Game.Scripts.Characters.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        private const float MinMoveDistance = 0.1f;

        [SerializeField] private CharacterController _characterController;
        [SerializeField] private Transform _visualRoot;
        [SerializeField] private float _speed = 5f;
        [SerializeField] private float _rotationSpeed = 10f;
        
        [Header("Idle Facing")]
        [SerializeField] private float _idleTurnSpeedMultiplier = 1f;
        [SerializeField] private float _idleInputThreshold = 0.02f;
        
        private Vector3 _idleForwardWorld;

        private void Awake()
        {
            if (_visualRoot != null)
                _idleForwardWorld = Vector3.ProjectOnPlane(_visualRoot.forward, Vector3.up).normalized;
        }
        
        private void OnValidate()
        {
            _characterController ??= GetComponent<CharacterController>();
        }

        public void Move(Vector2 direction)
        {
            Vector3 move = new Vector3(direction.x, 0f, direction.y);
            _characterController.Move(move * (_speed * Time.deltaTime));
            
            if (_visualRoot == null)
                return;
            
            if (move.sqrMagnitude > MinMoveDistance * MinMoveDistance)
            {
                RotateTowardsDirection(move, _rotationSpeed);
                return;
            }
            
            if (direction.sqrMagnitude <= _idleInputThreshold * _idleInputThreshold && _idleForwardWorld != Vector3.zero)
                RotateTowardsDirection(_idleForwardWorld, _rotationSpeed * _idleTurnSpeedMultiplier);
        }

        private void RotateTowardsDirection(Vector3 direction, float speed)
        {
            Vector3 flat = Vector3.ProjectOnPlane(direction, Vector3.up);
            
            if (flat.sqrMagnitude < 0.0001f)
                return;
            
            Quaternion targetRotation = Quaternion.LookRotation(flat.normalized);
            _visualRoot.rotation = Quaternion.Slerp(_visualRoot.rotation, targetRotation, speed * Time.deltaTime);
        }
    }
}
