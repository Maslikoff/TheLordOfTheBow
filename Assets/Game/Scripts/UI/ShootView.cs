using System.Collections.Generic;
using Game.Scripts.Characters.Bullets;
using Game.Scripts.Characters.Player;
using Game.Scripts.Upgrades;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.UI
{
    public class ShootView : MonoBehaviour
    {
        
        [SerializeField] private List<BulletUIConfig> _bulletUIConfigs;
        [SerializeField] private PlayerShoot _shootController;
        [SerializeField] private PlayerBulletUpgradeCollection _upgradeCollection;

        [SerializeField] private bool _clockwise = true;
        
        private Dictionary<BulletType, BulletUIConfig> _configs = new();
        private Dictionary<BulletType, System.Action> _shotEvents = new();
        private Dictionary<BulletType, System.Action<float>> _reloadEvents = new();

        private void Start()
        {
            InitializeDictionaries();
            UpdateCellsVisibility();
            SubscribeToEvents();
            ResetAllViews();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void InitializeDictionaries()
        {
            foreach (BulletUIConfig config in _bulletUIConfigs)
            {
                _configs[config.BulletType] = config;
                _shotEvents[config.BulletType] = () => OnShotFired(config.BulletType);
                _reloadEvents[config.BulletType] = (progress) => OnReloadProgress(config.BulletType, progress);
            }
        }
        
        private void UpdateCellsVisibility()
        {
            foreach (BulletUIConfig config in _bulletUIConfigs)
            {
                if (config.BulletType == BulletType.Arrow) 
                    continue;
                    
                bool isUnlocked = _upgradeCollection.IsUnlocked(config.BulletType);
                
                if (config.CellObject != null)
                    config.CellObject.gameObject.SetActive(isUnlocked);
                else if (config.CooldownImage != null)
                    config.CooldownImage.gameObject.SetActive(isUnlocked);
            }
        }
        
        private void OnShotFired(BulletType bulletType)
        {
            if (IsBulletVisible(bulletType) == false) 
                return;
            
            BulletUIConfig config = _configs[bulletType];

            if (config.CooldownImage == null) 
                return;
            
            config.CooldownImage.fillMethod = Image.FillMethod.Radial360;
            config.CooldownImage.fillOrigin = _clockwise ? 2 : 0;
            config.CooldownImage.fillAmount = 1f;
        }
        
        private void OnReloadProgress(BulletType bulletType, float progress)
        {
            if (IsBulletVisible(bulletType) == false) 
                return;
            
            BulletUIConfig config = _configs[bulletType];
            
            if (config.CooldownImage != null)
                config.CooldownImage.fillAmount = 1f - progress;
        }
        
        private void ResetAllViews()
        {
            foreach (BulletUIConfig config in _bulletUIConfigs)
            {
                if (config.CooldownImage != null && IsBulletVisible(config.BulletType))
                    config.CooldownImage.fillAmount = 0f;
            }
        }
        
        private void SubscribeToEvents()
        {
            _shootController.ShotArrow += () => OnShotFired(BulletType.Arrow);
            _shootController.ShotFireArrow += () => OnShotFired(BulletType.FireArrow);
            _shootController.ShotPoisonArrow += () => OnShotFired(BulletType.PoisonArrow);
            
            _shootController.ArrowReloadProgress += (p) => OnReloadProgress(BulletType.Arrow, p);
            _shootController.FireArrowReloadProgress += (p) => OnReloadProgress(BulletType.FireArrow, p);
            _shootController.PoisonArrowReloadProgress += (p) => OnReloadProgress(BulletType.PoisonArrow, p);
            
            _upgradeCollection.BulletUnlocked += OnBulletUnlocked;
        }
        
        private void UnsubscribeFromEvents()
        {
            if (_shootController == null || _upgradeCollection == null) 
                return;
            
            _shootController.ShotArrow -= () => OnShotFired(BulletType.Arrow);
            _shootController.ShotFireArrow -= () => OnShotFired(BulletType.FireArrow);
            _shootController.ShotPoisonArrow -= () => OnShotFired(BulletType.PoisonArrow);
                
            _shootController.ArrowReloadProgress -= (p) => OnReloadProgress(BulletType.Arrow, p);
            _shootController.FireArrowReloadProgress -= (p) => OnReloadProgress(BulletType.FireArrow, p);
            _shootController.PoisonArrowReloadProgress -= (p) => OnReloadProgress(BulletType.PoisonArrow, p);
            
            _upgradeCollection.BulletUnlocked -= OnBulletUnlocked;
        }
        
        private bool IsBulletVisible(BulletType bulletType)
        {
            if (bulletType == BulletType.Arrow) 
                return true;
            
            if (_configs.TryGetValue(bulletType, out BulletUIConfig config) == false) 
                return false;
            
            if (config.CellObject != null)
                return config.CellObject.gameObject.activeSelf;
            
            if (config.CooldownImage != null)
                return config.CooldownImage.gameObject.activeSelf;
            
            return false;
        }
        
        private void UpdateCellVisibility(BulletUIConfig config)
        {
            if (config.BulletType == BulletType.Arrow) 
                return;
                
            bool isUnlocked = _upgradeCollection.IsUnlocked(config.BulletType);
            
            if (config.CellObject != null)
                config.CellObject.gameObject.SetActive(isUnlocked);
            else if (config.CooldownImage != null)
                config.CooldownImage.gameObject.SetActive(isUnlocked);
        }
        
        private void OnBulletUnlocked(BulletType bulletType)
        {
            if (_configs.TryGetValue(bulletType, out BulletUIConfig config))
                UpdateCellVisibility(config);
        }
    }
}