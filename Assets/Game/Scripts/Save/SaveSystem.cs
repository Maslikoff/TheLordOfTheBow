using System;
using Cysharp.Threading.Tasks;
using Game.Scripts.Characters.Bullets;
using Game.Scripts.Characters.Player;
using Game.Scripts.Levels;
using Game.Scripts.Upgrades;
using UniRx;
using UnityEngine;
using VContainer;
using YG;

namespace Game.Scripts.Save
{
    public class SaveSystem : MonoBehaviour, ISaveSystem, ISaveLoadGate
    {
        private const float SdkWaitTimeoutSeconds = 5f;
        
        private readonly CompositeDisposable _disposables = new();
        
        [Inject] private ILevelService _levelService;
        [Inject] private IPlayerProgressService _playerProgressService;
        
        private Player _currentPlayer;
        private UniTaskCompletionSource _readyTcs;
        
        private bool _metaDataLoaded;
        private bool _isReady;
        
        public bool IsMetaDataLoaded => _metaDataLoaded;
        public bool IsReady => _isReady;

        private void OnEnable()
        {
            YG2.onGetSDKData += OnSdkDataReady;
            YG2.onHideWindowGame += OnHideWindow;
            
            MessageBroker.Default.Receive<M_PlayerSpawned>()
                .Subscribe(msg => OnPlayerSpawned(msg.Player))
                .AddTo(_disposables);
            MessageBroker.Default.Receive<M_SaveRequested>()
                .Subscribe(_ => SavePlayerProgress())
                .AddTo(_disposables);
        }

        private void OnDisable()
        {
            YG2.onGetSDKData -= OnSdkDataReady;
            YG2.onHideWindowGame -= OnHideWindow;
            
            UnsubscribeFromPlayer();
            
            _disposables.Clear();
        }

        private void Start()
        {
            if (YG2.isSDKEnabled)
                OnSdkDataReady();
            else
                WaitForSdkWithTimeout().Forget();
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

        public UniTask WaitUntilReadyAsync()
        {
            if (_isReady)
                return UniTask.CompletedTask;
            _readyTcs ??= new UniTaskCompletionSource();
            return _readyTcs.Task;
        }
        
        public void LoadGameData()
        {
            OnSdkDataReady();
        }

        public void ManualSave()
        {
            SaveGameData();
        }

        public void SavePlayerProgress()
        {
            try
            {
                if (_currentPlayer != null)
                    _playerProgressService.CaptureFrom(_currentPlayer);
                else if (_playerProgressService.HasSessionProgress)
                    _playerProgressService.SyncToSaves(YG2.saves);
                else
                    return;

                WritePlayerProgressToSaves(YG2.saves);

                if (YG2.isSDKEnabled)
                    YG2.SaveProgress();
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Ошибка сохранения прогресса игрока: {e.Message}");
            }
        }

        public void SaveGameData()
        {
            SavePlayerProgress();
            
            if (YG2.isSDKEnabled == false)
            {
                Debug.LogWarning("[SaveSystem] SDK не готов — индекс уровня сохранён только в памяти");
                return;
            }
            
            Debug.Log("[SaveSystem] Сохранение данных игры...");
            
            try
            {
                if (_levelService != null)
                {
                    YG2.saves.WriteCurrentLevelIndex(_levelService.CurrentLevelIndex);
                    Debug.Log($"[SaveSystem] Сохранён индекс уровня: {_levelService.CurrentLevelIndex}");
                }
                
                YG2.SaveProgress();
                
                Debug.Log("[SaveSystem] Данные успешно сохранены в облако!");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Ошибка сохранения: {e.Message}");
            }
        }

        private void WritePlayerProgressToSaves(SavesYG saves)
        {
            if (_playerProgressService.HasSessionProgress)
            {
                _playerProgressService.SyncToSaves(saves);
                return;
            }

            if (_currentPlayer == null)
                return;
            
            saves.WritePlayerLevel(_currentPlayer.Experience.CurrentLevel);
            saves.WritePlayerExperience(_currentPlayer.Experience.CurrentExperience);
            
            Debug.Log($"[SaveSystem] Сохранён уровень игрока: {_currentPlayer.Experience.CurrentLevel}, " +
                      $"опыт: {_currentPlayer.Experience.CurrentExperience}");
            
            PlayerBulletUpgradeCollection bulletCollection = _currentPlayer.BulletUpgrades;
            
            foreach (BulletType bulletType in Enum.GetValues(typeof(BulletType)))
            {
                PlayerBulletUpgradeEntry entry = bulletCollection.Get(bulletType);
                
                if (entry == null)
                    continue;
                
                var state = new BulletUpgradeState(
                    entry.IsUnlocked,
                    entry.DamageBonus,
                    entry.LifeTimeBonus,
                    entry.CountBonus);
                saves.WriteBulletUpgradeState(bulletType, state);
                
                Debug.Log($"[SaveSystem] Сохранена прокачка пули {bulletType}: разблокирована={state.IsUnlocked}, " +
                          $"урон={state.DamageBonus}");
            }
        }
        
        private void OnSdkDataReady()
        {
            Debug.Log("[SaveSystem] SDK данные получены — загрузка...");
            
            try
            {
                _playerProgressService.LoadFromSaves(YG2.saves);
                LoadMetaData();
                _metaDataLoaded = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Ошибка загрузки: {e.Message}");
            }
            finally
            {
                MarkReady();
            }
        }
        
        private void LoadMetaData()
        {
            if (_levelService == null)
                return;
            
            int levelIndex = YG2.saves.CurrentLevelIndex;
            _levelService.SetCurrentLevelIndex(levelIndex);
            
            Debug.Log($"[SaveSystem] Загружен индекс уровня: {levelIndex}");
        }
        
        private void OnPlayerSpawned(Player player)
        {
            UnsubscribeFromPlayer();
            _currentPlayer = player;
            _currentPlayer.Experience.LevelUp += OnPlayerLevelUp;
            
            LoadPlayerData(player);
        }
        
        private void LoadPlayerData(Player player)
        {
            Debug.Log("[SaveSystem] Загрузка данных игрока...");
            
            if (_playerProgressService.HasSessionProgress)
            {
                _playerProgressService.ApplyTo(player);
                _playerProgressService.SyncToSaves(YG2.saves);
                
                Debug.Log("[SaveSystem] Применён сессионный прогресс игрока");
                return;
            }

            if (_metaDataLoaded || YG2.isSDKEnabled)
                ApplyCloudSavesToPlayer(player);
        }

        private void ApplyCloudSavesToPlayer(Player player)
        {
            var saves = YG2.saves;
            player.Experience.LoadSaveData(saves.PlayerLevel, saves.PlayerExperience);
            
            Debug.Log($"[SaveSystem] Загружен уровень игрока: {saves.PlayerLevel}, опыт: {saves.PlayerExperience}");
            
            PlayerBulletUpgradeCollection bulletCollection = player.BulletUpgrades;
            
            foreach (BulletType bulletType in Enum.GetValues(typeof(BulletType)))
            {
                BulletUpgradeState state = saves.GetBulletUpgradeState(bulletType);
                PlayerBulletUpgradeEntry entry = bulletCollection.Get(bulletType);
                
                if (entry == null)
                    continue;
                
                entry.ApplySaveState(state);
                
                Debug.Log($"[SaveSystem] Загружена прокачка пули {bulletType}: разблокирована={state.IsUnlocked}, урон={state.DamageBonus}");
            }
            
            bulletCollection.NotifyLoadedFromSave();
            _playerProgressService.CaptureFrom(player);
        }
        
        private void OnPlayerLevelUp(int level)
        {
            SavePlayerProgress();
        }
        
        private void OnHideWindow()
        {
            SaveGameData();
        }
        
        private void UnsubscribeFromPlayer()
        {
            if (_currentPlayer == null)
                return;
            
            _currentPlayer.Experience.LevelUp -= OnPlayerLevelUp;
            _currentPlayer = null;
        }
        
        private void MarkReady()
        {
            if (_isReady)
                return;
            
            _isReady = true;
            _readyTcs?.TrySetResult();
            
            Debug.Log("[SaveSystem] Облачные данные готовы — можно стартовать игру");
        }
        
        private async UniTaskVoid WaitForSdkWithTimeout()
        {
            float elapsed = 0f;
            
            while (!YG2.isSDKEnabled && elapsed < SdkWaitTimeoutSeconds)
            {
                await UniTask.Yield();
                elapsed += Time.unscaledDeltaTime;
            }
            
            if (YG2.isSDKEnabled)
                OnSdkDataReady();
            else
            {
                _playerProgressService.LoadFromSaves(YG2.saves);
                MarkReady();
            }
        }
    }
}
