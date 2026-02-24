using Game.Scripts.Characters;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.UI
{
    public class ShootView : MonoBehaviour
    {
        [SerializeField] private ShootEntity _shootController;
        [SerializeField] private Image _cooldownImage;
    
        [SerializeField] private bool _clockwise = true;
    
        private void Start()
        {
            _shootController.ShotFired += OnShotFired;
            _shootController.ReloadProgressUpdated += OnReloadProgressUpdated;
    
            ResetView();
        }
    
        private void OnDestroy()
        {
            if (_shootController == null)
                return;
    
            _shootController.ShotFired -= OnShotFired;
            _shootController.ReloadProgressUpdated -= OnReloadProgressUpdated;
        }
    
        private void OnShotFired()
        {
            if (_cooldownImage != null)
            {
                _cooldownImage.fillMethod = Image.FillMethod.Radial360;
                _cooldownImage.fillOrigin = _clockwise ? 2 : 0;
                _cooldownImage.fillAmount = 1f;
            }
        }
    
        private void OnReloadProgressUpdated(float progress)
        {
            if (_cooldownImage != null)
                _cooldownImage.fillAmount = 1f - progress;
        }
    
        private void ResetView()
        {
            if (_cooldownImage != null)
                _cooldownImage.fillAmount = 0f;
        }
    }
}
