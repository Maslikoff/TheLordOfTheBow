using Game.Scripts.StateServices;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Scripts.Characters.Player
{
    public class JoystickZone : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private Joystick _joystick;
        
        private int _activePointerId = -1;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (GameplayControlAccess.Instance != null && GameplayControlAccess.Instance.IsBlocked)
                return;

            if (_joystick == null)
                return;
            
            if (_activePointerId != -1)
                return;
            
            _activePointerId = eventData.pointerId;
            _joystick.OnPointerDown(eventData);
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (GameplayControlAccess.Instance != null && GameplayControlAccess.Instance.IsBlocked)
                return;

            if (_joystick == null)
                return;
            
            if (eventData.pointerId != _activePointerId)
                return;
            
            _joystick.OnDrag(eventData);
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {
            if (GameplayControlAccess.Instance != null && GameplayControlAccess.Instance.IsBlocked)
                return;

            if (_joystick == null)
                return;
            
            if (eventData.pointerId != _activePointerId)
                return;
            
            _joystick.OnPointerUp(eventData);
            _activePointerId = -1;
        }
        
        private void OnDisable()
        {
            _activePointerId = -1;
        }
    }
}
