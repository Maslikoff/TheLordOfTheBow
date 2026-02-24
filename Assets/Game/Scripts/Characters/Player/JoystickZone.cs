using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Scripts.Characters.Player
{
    public class JoystickZone : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private Joystick _joystick;

        public void OnPointerDown(PointerEventData eventData)
        {
            _joystick.OnPointerDown(eventData);
        }
    }
}
