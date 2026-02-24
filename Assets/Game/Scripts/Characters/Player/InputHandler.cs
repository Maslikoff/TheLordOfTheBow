using System;
using UnityEngine;

namespace Game.Scripts.Characters.Player
{
    public class InputHandler : MonoBehaviour
    {
        private const string AxisHorizontal = "Horizontal";
        private const string AxisVertical = "Vertical";

        [SerializeField] private DynamicJoystick _joystick;

        private Vector2 _moveInput;

        public event Action<Vector2> MoveInput;

        private void Update()
        {
            HandleKeyboardInput();
            HandleJoystickInput();
        }

        private void HandleKeyboardInput()
        {
            Vector2 keyboardInput = new Vector2(Input.GetAxis(AxisHorizontal), Input.GetAxis(AxisVertical));

            keyboardInput = keyboardInput.magnitude > 1f ? keyboardInput.normalized : keyboardInput;
            _moveInput = keyboardInput;

            MoveInput?.Invoke(_moveInput);
        }

        private void HandleJoystickInput()
        {
            MoveInput?.Invoke(_joystick.Direction);
        }
    }
}