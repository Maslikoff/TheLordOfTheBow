using System;
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
        
        private Action _onShotArrow;
        private Action _onShotFireArrow;
        private Action _onShotPoisonArrow;
        private Action<float> _onArrowReload;
        private Action<float> _onFireArrowReload;
        private Action<float> _onPoisonArrowReload;

        private void Awake()
        {
            _onShotArrow = () => OnShotFired(BulletType.Arrow);
            _onShotFireArrow = () => OnShotFired(BulletType.FireArrow);
            _onShotPoisonArrow = () => OnShotFired(BulletType.PoisonArrow);
            _onArrowReload = progress => OnReloadProgress(BulletType.Arrow, progress);
            _onFireArrowReload = progress => OnReloadProgress(BulletType.FireArrow, progress);
            _onPoisonArrowReload = progress => OnReloadProgress(BulletType.PoisonArrow, progress);
            
            InitializeDictionaries();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        public void Initialize(PlayerShoot shootController, PlayerBulletUpgradeCollection upgradeCollection)
        {
            UnsubscribeFromEvents();

            _shootController = shootController;
            _upgradeCollection = upgradeCollection;

            SubscribeToEvents();
            RefreshFromCollection();
        }
        
        public void RefreshFromCollection()
        {
            UpdateCellsVisibility();
            ResetAllViews();
        }
        
        private void InitializeDictionaries()
        {
            foreach (BulletUIConfig config in _bulletUIConfigs)
                _configs[config.BulletType] = config;
        }
        
        private void UpdateCellsVisibility()
        {
            if (_upgradeCollection == null)
                return;
            
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
            if (_shootController == null || _upgradeCollection == null)
                return;
            
            _shootController.ShotArrow += _onShotArrow;
            _shootController.ShotFireArrow += _onShotFireArrow;
            _shootController.ShotPoisonArrow += _onShotPoisonArrow;
            _shootController.ArrowReloadProgress += _onArrowReload;
            _shootController.FireArrowReloadProgress += _onFireArrowReload;
            _shootController.PoisonArrowReloadProgress += _onPoisonArrowReload;
            _upgradeCollection.BulletUnlocked += OnBulletUnlocked;
            _upgradeCollection.UpgradesLoaded += RefreshFromCollection;
        }
        
        private void UnsubscribeFromEvents()
        {
            if (_shootController == null || _upgradeCollection == null) 
                return;
            
            _shootController.ShotArrow -= _onShotArrow;
            _shootController.ShotFireArrow -= _onShotFireArrow;
            _shootController.ShotPoisonArrow -= _onShotPoisonArrow;
            _shootController.ArrowReloadProgress -= _onArrowReload;
            _shootController.FireArrowReloadProgress -= _onFireArrowReload;
            _shootController.PoisonArrowReloadProgress -= _onPoisonArrowReload;
            _upgradeCollection.BulletUnlocked -= OnBulletUnlocked;
            _upgradeCollection.UpgradesLoaded -= RefreshFromCollection;
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
