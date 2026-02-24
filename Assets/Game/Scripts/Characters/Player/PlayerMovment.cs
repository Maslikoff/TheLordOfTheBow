using UnityEngine;

namespace Game.Scripts.Characters.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovment : MonoBehaviour
    {
        private const float MinDistance = 0.1f;

        [SerializeField] private CharacterController _characterController;
        [SerializeField] private Transform _visualRoot;
        [SerializeField] private float _speed = 5f;
        [SerializeField] private float _rotationSpeed = 10f;

        private void OnValidate()
        {
            _characterController ??= GetComponent<CharacterController>();
        }

        public void Move(Vector2 direction)
        {
            Vector3 move = new Vector3(direction.x, 0, direction.y);

            _characterController.Move(move * _speed * Time.deltaTime);

            if (move.magnitude > 0.1f && _visualRoot != null)
                RotateTowardsDirection(move);
        }

        private void RotateTowardsDirection(Vector3 direction)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            _visualRoot.rotation =
                Quaternion.Slerp(_visualRoot.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
        }
    }
}
