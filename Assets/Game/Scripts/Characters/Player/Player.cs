using UnityEngine;

namespace Game.Scripts.Characters.Player
{
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(InputHandler))]
    public class Player : MonoBehaviour
    {
        [SerializeField] private PlayerMovement movement;
        [SerializeField] private InputHandler _inputHandler;

        private void OnEnable()
        {
            _inputHandler.MoveInput += movement.Move;
        }

        private void OnValidate()
        {
            movement ??= GetComponent<PlayerMovement>();
            _inputHandler ??= GetComponent<InputHandler>();
        }

        private void OnDisable()
        {
            _inputHandler.MoveInput -= movement.Move;
        }
    }
}