using System;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    private const string AxisHorizontal = "Horizontal";
    private const string AxisVertical = "Vertical";
    private const int LeftMouseButton = 0;
    private const int RightMouseButton = 1;
    private const KeyCode SpaceKey = KeyCode.Space;

    [SerializeField] private DynamicJoystick _joystick;

    private Vector2 _moveInput;

    public event Action<Vector2> MoveInput;
    public event Action LeftClick;
    public event Action RightClick;
    public event Action Space;

    private void Update()
    {
        HandleKeyboardInput();
        HandleJoystickInput();

        if (Input.GetMouseButtonDown(LeftMouseButton))
            LeftClick?.Invoke();

        if (Input.GetMouseButtonDown(RightMouseButton))
            RightClick?.Invoke();

        if (Input.GetKeyDown(SpaceKey))
            Space?.Invoke();
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