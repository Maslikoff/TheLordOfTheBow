using UnityEngine;

namespace Game.Scripts.Characters.Player
{
    [RequireComponent(typeof(PlayerMovment))]
    [RequireComponent(typeof(InputHandler))]
    public class Player : MonoBehaviour
    {
        [SerializeField] private PlayerMovment _movment;
        [SerializeField] private InputHandler _inputHandler;

        private void OnEnable()
        {
            _inputHandler.MoveInput += _movment.Move;
        }

        private void OnValidate()
        {
            _movment ??= GetComponent<PlayerMovment>();
            _inputHandler ??= GetComponent<InputHandler>();
        }

        private void OnDisable()
        {
            _inputHandler.MoveInput -= _movment.Move;
        }
    }
}