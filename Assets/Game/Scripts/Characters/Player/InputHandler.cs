using System;
using UnityEngine;

namespace Game.Scripts.Characters.Player
{
    public class InputHandler : MonoBehaviour
    {
        private const string AxisHorizontal = "Horizontal";
        private const string AxisVertical = "Vertical";
        private const float JoystickDeadZone = 0.01f;

        private DynamicJoystick _joystick;
        private Vector2 _moveInput;

        public event Action<Vector2> MoveInput;
        
        public void SetJoystick(DynamicJoystick joystick)
        {
            _joystick = joystick;
        }

        private void Update()
        {
            Vector2 keyboardInput = new Vector2(Input.GetAxis(AxisHorizontal), Input.GetAxis(AxisVertical));
            keyboardInput = keyboardInput.magnitude > 1f ? keyboardInput.normalized : keyboardInput;

            Vector2 joystickInput = _joystick != null ? _joystick.Direction : Vector2.zero;

            _moveInput = joystickInput.sqrMagnitude > JoystickDeadZone * JoystickDeadZone
                ? joystickInput
                : keyboardInput;

            MoveInput?.Invoke(_moveInput);
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