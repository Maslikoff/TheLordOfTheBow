using UnityEngine;
using UnityEngine.EventSystems;

public class JoystickZone : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private Joystick _joystick;

    public void OnPointerDown(PointerEventData eventData)
    {
        _joystick.OnPointerDown(eventData);
    }
}