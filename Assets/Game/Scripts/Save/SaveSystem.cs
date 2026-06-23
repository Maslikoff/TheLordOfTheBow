using System;
using Game.Scripts.Characters.Bullets;
using Game.Scripts.Levels;
using Game.Scripts.Upgrades;
using UnityEngine;
using VContainer;
using YG;

namespace Game.Scripts.Save
{
    public class SaveSystem : MonoBehaviour, ISaveSystem
    {
        [Inject] private Experience.Experience _playerExperience;
        [Inject] private ILevelService _levelService;
        [Inject] private PlayerBulletUpgradeCollection _bulletCollection;

        private void OnEnable()
        {
            YG2.onGetSDKData += LoadGameData;
        }

        private void OnDisable()
        {
            YG2.onGetSDKData -= LoadGameData;
        }

        private void Start()
        {
            if (YG2.isSDKEnabled)
                LoadGameData();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
                SaveGameData();
        }

        private void OnApplicationQuit()
        {
            SaveGameData();
        }

        public void LoadGameData()
        {
            Debug.Log("[SaveSystem] Загрузка данных игры...");
            
            try
            {
                var saves = YG2.saves;

                if (_playerExperience != null)
                {
                    int level = saves.PlayerLevel;
                    float experience = saves.PlayerExperience;
                    _playerExperience.LoadSaveData(level, experience);
                    Debug.Log($"[SaveSystem] Загружен уровень игрока: {level}, опыт: {experience}");
                }

                if (_levelService != null)
                {
                    int levelIndex = saves.CurrentLevelIndex;
                    if (_levelService is LevelService service)
                    {
                        service.SetCurrentLevelIndex(levelIndex);
                        Debug.Log($"[SaveSystem] Загружен индекс уровня: {levelIndex}");
                    }
                }

                if (_bulletCollection != null)
                {
                    var bulletTypes = Enum.GetValues(typeof(BulletType));
                    foreach (BulletType bulletType in bulletTypes)
                    {
                        BulletUpgradeState state = saves.GetBulletUpgradeState(bulletType);
                        var entry = _bulletCollection.Get(bulletType);
                        
                        if (entry != null)
                        {
                            if (state.IsUnlocked)
                                entry.Unlock(); 
                            
                            //entry.SetDamageBonus(state.DamageBonus);
                            //entry.SetLifeTimeBonus(state.LifeTimeBonus);
                            //entry.SetCountBonus(state.CountBonus);
                            
                            Debug.Log($"[SaveSystem] Загружена прокачка пули {bulletType}: разблокирована={state.IsUnlocked}, урон={state.DamageBonus}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Ошибка загрузки: {e.Message}");
            }
        }

        public void SaveGameData()
        {
            Debug.Log("[SaveSystem] Сохранение данных игры...");
            
            try
            {
                var saves = YG2.saves;

                if (_playerExperience != null)
                {
                    saves.WritePlayerLevel(_playerExperience.CurrentLevel);
                    saves.WritePlayerExperience(_playerExperience.CurrentExperience);
                    Debug.Log($"[SaveSystem] Сохранён уровень игрока: {_playerExperience.CurrentLevel}, опыт: {_playerExperience.CurrentExperience}");
                }

                if (_levelService != null)
                {
                    saves.WriteCurrentLevelIndex(_levelService.CurrentLevel - 1);
                    Debug.Log($"[SaveSystem] Сохранён индекс уровня: {_levelService.CurrentLevel - 1}");
                }

                if (_bulletCollection != null)
                {
                    var bulletTypes = Enum.GetValues(typeof(BulletType));
                    foreach (BulletType bulletType in bulletTypes)
                    {
                        var entry = _bulletCollection.Get(bulletType);
                        if (entry != null)
                        {
                            var state = new BulletUpgradeState(
                                entry.IsUnlocked//,
                                //entry.DamageBonus,
                                //entry.LifeTimeBonus,
                                //entry.CountBonus
                            );
                            saves.WriteBulletUpgradeState(bulletType, state);
                            Debug.Log($"[SaveSystem] Сохранена прокачка пули {bulletType}: разблокирована={state.IsUnlocked}, урон={state.DamageBonus}");
                        }
                    }
                }

                YG2.SaveProgress();
                Debug.Log("[SaveSystem] Данные успешно сохранены в облако!");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Ошибка сохранения: {e.Message}");
            }
        }

        public void ManualSave()
        {
            SaveGameData();
        }
    }
}