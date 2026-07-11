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
        [Inject] private LevelSessionService _levelSessionService;
        
        private Player _currentPlayer;
        private UniTaskCompletionSource _readyTcs;
        
        private bool _metaDataLoaded;
        private bool _isReady;
        
        public bool IsMetaDataLoaded => _metaDataLoaded;
        public bool IsReady => _isReady;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            Subscribe();
        }

        private void Start()
        {
            if (YG2.isSDKEnabled)
                OnSdkDataReady();
            else
                WaitForSdkWithTimeout().Forget();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void Subscribe()
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
        
        private void Unsubscribe()
        {
            YG2.onGetSDKData -= OnSdkDataReady;
            YG2.onHideWindowGame -= OnHideWindow;
            UnsubscribeFromPlayer();
            _disposables.Clear();
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
        
        public void LoadGameData() => OnSdkDataReady();
        public void ManualSave() => SaveGameData();

        public void SaveWaveCheckpoint(int levelIndex, int waveIndex)
        {
            YG2.saves.WriteWaveCheckpoint(levelIndex, waveIndex);
            if (YG2.isSDKEnabled)
                YG2.SaveProgress();
        }

        public int GetWaveCheckpointOrDefault(int levelIndex)
        {
            return YG2.saves.TryGetWaveCheckpoint(levelIndex, out int waveIndex)
                ? waveIndex
                : 0;
        }

        public void ClearWaveCheckpoint(int levelIndex)
        {
            YG2.saves.ClearWaveCheckpoint(levelIndex);
            if (YG2.isSDKEnabled)
                YG2.SaveProgress();
        }
        
        public void CommitSessionProgress()
        {
            if (_currentPlayer == null || _levelSessionService == null)
                return;

            _levelSessionService.CaptureSnapshot(_currentPlayer.Experience, _currentPlayer.BulletUpgrades);
            SavePlayerProgress();
        }

        public void SavePlayerProgress()
        {
            if (YG2.isSDKEnabled == false)
                return;

            try
            {
                WritePlayerProgressToSaves(YG2.saves);
                YG2.SaveProgress();
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Ошибка сохранения прогресса игрока: {e.Message}");
            }
        }

        public void SaveGameData()
        {
            if (YG2.isSDKEnabled == false)
            {
                Debug.LogWarning("[SaveSystem] SDK не готов — сохранение пропущено");
                return;
            }
            
            try
            {
                var saves = YG2.saves;
                WritePlayerProgressToSaves(saves);
                
                if (_levelService != null)
                    saves.WriteCurrentLevelIndex(_levelService.CurrentLevelIndex);
                
                YG2.SaveProgress();
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Ошибка сохранения: {e.Message}");
            }
        }

        private void WritePlayerProgressToSaves(SavesYG saves)
        {
            if (_currentPlayer == null)
                return;
            
            (int level, float experience) = GetExperienceToPersist();
            saves.WritePlayerLevel(level);
            saves.WritePlayerExperience(experience);
            
            PlayerBulletUpgradeCollection bulletCollection = _currentPlayer.BulletUpgrades;
            
            foreach (BulletType bulletType in Enum.GetValues(typeof(BulletType)))
            {
                PlayerBulletUpgradeEntry entry = bulletCollection.Get(bulletType);
                if (entry == null) continue;
                
                saves.WriteBulletUpgradeState(bulletType, new BulletUpgradeState(
                    entry.IsUnlocked, entry.DamageBonus, entry.LifeTimeBonus, entry.CountBonus));
            }
        }
        
        private (int level, float experience) GetExperienceToPersist()
        {
            if (_levelSessionService != null && _levelSessionService.HasSnapshot)
                return (_levelSessionService.SnapshotLevel, _levelSessionService.SnapshotExperience);
            
            return (_currentPlayer.Experience.CurrentLevel, _currentPlayer.Experience.CurrentExperience);
        }
        
        private void OnSdkDataReady()
        {
            try
            {
                LoadMetaData();
                _metaDataLoaded = true;
                if (_currentPlayer != null)
                    LoadPlayerData(_currentPlayer);
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
            if (_levelService == null) return;
            _levelService.SetCurrentLevelIndex(YG2.saves.CurrentLevelIndex);
        }
        
        private void OnPlayerSpawned(Player player)
        {
            UnsubscribeFromPlayer();
            _currentPlayer = player;
            
            if (_metaDataLoaded || YG2.isSDKEnabled)
                LoadPlayerData(player);
        }
        
        private void LoadPlayerData(Player player)
        {
            var saves = YG2.saves;
            player.Experience.LoadSaveData(saves.PlayerLevel, saves.PlayerExperience);
            
            PlayerBulletUpgradeCollection bulletCollection = player.BulletUpgrades;
            
            foreach (BulletType bulletType in Enum.GetValues(typeof(BulletType)))
            {
                PlayerBulletUpgradeEntry entry = bulletCollection.Get(bulletType);
                if (entry == null) continue;

                if (saves.TryGetBulletUpgradeState(bulletType, out BulletUpgradeState state))
                    entry.ApplySaveState(state);
            }

            bulletCollection.NotifyLoadedFromSave();
        }
        
        private void OnHideWindow() => SaveGameData();
        
        private void UnsubscribeFromPlayer()
        {
            _currentPlayer = null;
        }
        
        private void MarkReady()
        {
            if (_isReady) return;
            _isReady = true;
            _readyTcs?.TrySetResult();
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
                MarkReady();
        }
    }
}