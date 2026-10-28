using System;
using Cysharp.Threading.Tasks;
using Game.Scripts.Characters.Player;
using Game.Scripts.Levels;
using Game.Scripts.Shop;
using UniRx;
using UnityEngine;
using VContainer;
using YG;

namespace Game.Scripts.Save
{
    public class SaveSystem : MonoBehaviour, ISaveSystem, ISaveLoadGate
    {
        private readonly CompositeDisposable _disposables = new();
        [Inject] private ILevelService _levelService;
        [Inject] private LevelSessionService _session;
        [Inject] private ICurrencyService _currency;
        [Inject] private ICosmeticInventoryService _cosmetics;
        private Player _player;
        private UniTaskCompletionSource _ready;
        private bool _victoryCommitted;
        private bool _defeated;
        public bool IsMetaDataLoaded { get; private set; }
        public bool IsReady { get; private set; }
        private CampaignSaveData Data => YG2.saves.EnsureCampaign();

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            YG2.onGetSDKData += OnSdkDataReady;
            YG2.onHideWindowGame += SaveGameData;
            MessageBroker.Default.Receive<M_PlayerSpawned>().Subscribe(x => OnPlayerSpawned(x.Player)).AddTo(_disposables);
            MessageBroker.Default.Receive<M_SaveRequested>().Subscribe(_ => SavePlayerProgress()).AddTo(_disposables);
        }

        private void Start() => WaitForSdk().Forget();
        private async UniTaskVoid WaitForSdk()
        {
            float until = Time.realtimeSinceStartup + 5;
            while (!YG2.isSDKEnabled && Time.realtimeSinceStartup < until) await UniTask.Yield();
            if (!IsReady) OnSdkDataReady();
        }

        private void OnDestroy()
        {
            YG2.onGetSDKData -= OnSdkDataReady;
            YG2.onHideWindowGame -= SaveGameData;
            _disposables.Dispose();
        }

        public UniTask WaitUntilReadyAsync()
        {
            if (IsReady) return UniTask.CompletedTask;
            _ready ??= new UniTaskCompletionSource();
            return _ready.Task;
        }

        private void OnSdkDataReady()
        {
            // A late SDK notification must not rewind a running attempt.
            if (IsReady && _player != null) return;
            var data = Data;
            _levelService.SetCampaignPosition(data.StageIndex, data.Cycle, data.UnlockedStage);
            var saves = YG2.saves;
            _currency.Load(saves.Coins);
            _cosmetics.Load(saves.OwnedSkins, saves.OwnedProjectiles, saves.OwnedHats,
                saves.EquippedSkin, saves.EquippedProjectile, saves.EquippedHat);
            IsMetaDataLoaded = IsReady = true;
            _ready?.TrySetResult();
        }

        private void OnPlayerSpawned(Player player)
        {
            _player = player;
            _victoryCommitted = _defeated = false;
            _session.Reset();
            var data = Data;
            var snapshot = data.HasCheckpoint(_levelService.CurrentLevelIndex, _levelService.Cycle) ? data.Checkpoint : data.Committed;
            snapshot?.Apply(player, _currency);
        }

        private PlayerProgressSnapshot Capture()
        {
            var state = PlayerProgressSnapshot.Capture(_player, _currency);
            if (Data.Committed?.LegacyBonuses != null) state.LegacyBonuses = Data.Committed.Clone().LegacyBonuses;
            return state;
        }

        public int BeginStage(Player player)
        {
            _player = player;
            var data = Data;
            int stage = _levelService.CurrentLevelIndex;
            int cycle = _levelService.Cycle;
            bool resume = data.HasCheckpoint(stage, cycle) && data.WaveIndex >= 0 && data.WaveIndex < _levelService.CurrentConfig.WaveConfig.Waves.Count;
            _player.Campaign.BeginStage(_levelService.UnlockedStage, !resume);
            if (resume)
            {
                _session.Entry = (data.Entry ?? data.Checkpoint).Clone();
                _session.Wave = data.Checkpoint.Clone();
                _session.WaveIndex = data.WaveIndex;
            }
            else
            {
                _session.Entry = Capture();
                _session.Wave = _session.Entry.Clone();
                _session.WaveIndex = 0;
                data.Entry = _session.Entry.Clone();
                data.Checkpoint = _session.Entry.Clone();
                data.CheckpointStage = stage;
                data.CheckpointCycle = cycle;
                data.WaveIndex = 0;
            }
            data.StageIndex = stage;
            data.Cycle = cycle;
            data.UnlockedStage = _player.Campaign.UnlockedStage;
            SaveGameData();
            return _session.WaveIndex;
        }

        public void SaveWaveCheckpoint(int levelIndex, int waveIndex)
        {
            if (_player == null || _defeated || _victoryCommitted) return;
            var data = Data;
            _session.Wave = Capture();
            _session.WaveIndex = waveIndex;
            data.Entry = _session.Entry.Clone();
            data.Checkpoint = _session.Wave.Clone();
            data.CheckpointStage = levelIndex;
            data.CheckpointCycle = _levelService.Cycle;
            data.WaveIndex = waveIndex;
            SaveGameData();
        }

        public int GetWaveCheckpointOrDefault(int levelIndex) => Data.HasCheckpoint(levelIndex, _levelService.Cycle) ? Data.WaveIndex : 0;
        public void ClearWaveCheckpoint(int levelIndex)
        {
            if (Data.CheckpointStage == levelIndex && Data.CheckpointCycle == _levelService.Cycle) Data.ClearCheckpoint();
        }

        public void RestartStage()
        {
            if (_player == null || _session.Entry == null) return;
            _session.Entry.Apply(_player, _currency);
            var data = Data;
            data.StageIndex = _levelService.CurrentLevelIndex;
            data.Cycle = _levelService.Cycle;
            data.Committed = _session.Entry.Clone();
            data.ClearCheckpoint();
            _session.Wave = _session.Entry.Clone();
            _session.PreDeath = null;
            _victoryCommitted = _defeated = false;
            SaveGameData();
        }

        public void RecordDefeat()
        {
            if (_player == null || _session.Entry == null) return;
            _session.PreDeath = Capture();
            _defeated = true;
            Data.Committed = _session.Entry.Clone();
            Data.ClearCheckpoint();
            SaveGameData();
        }

        public void RestoreAfterRevive()
        {
            if (_session.PreDeath == null || _player == null) return;
            _session.PreDeath.Apply(_player, _currency, false);
            _session.PreDeath = null;
            _defeated = false;
            Data.Entry = _session.Entry.Clone();
            Data.Checkpoint = _session.Wave.Clone();
            Data.CheckpointStage = _levelService.CurrentLevelIndex;
            Data.CheckpointCycle = _levelService.Cycle;
            Data.WaveIndex = _session.WaveIndex;
            SaveGameData();
        }

        public void CommitSessionProgress()
        {
            if (_player == null || _player.IsDead) return;
            var data = Data;
            data.Committed = Capture();
            if (!_victoryCommitted)
            {
                var next = CampaignRules.Next(_levelService.CurrentLevelIndex, _levelService.Cycle);
                data.StageIndex = next.index;
                data.Cycle = next.cycle;
                data.UnlockedStage = Mathf.Max(data.UnlockedStage, next.index + 1);
                data.ClearCheckpoint();
                _victoryCommitted = true;
            }
            SaveGameData();
        }

        public void SavePlayerProgress() => SaveGameData();
        public void ManualSave() => SaveGameData();
        public void LoadGameData() => OnSdkDataReady();
        private void OnApplicationPause(bool paused) { if (paused) SaveGameData(); }
        private void OnApplicationQuit() => SaveGameData();

        public void SaveGameData()
        {
            if (!IsReady) return;
            var data = Data;
            // Persist only a coherent boundary, never live XP with older weapon/health state.
            var state = data.Checkpoint ?? data.Committed;
            if (state != null)
            {
                YG2.saves.WritePlayerLevel(state.Level);
                YG2.saves.WritePlayerExperience(state.Experience);
                YG2.saves.WriteCoins(state.Coins);
                foreach (var b in state.Bullets)
                    YG2.saves.WriteBulletUpgradeState(b.BulletType, new Upgrades.BulletUpgradeState(b.IsUnlocked,
                        b.DamageBonus, b.LifeTimeBonus, b.CountBonus, b.PoisonDamageBonus));
            }
            YG2.saves.WriteCurrentLevelIndex(data.StageIndex);
            if (YG2.isSDKEnabled) YG2.SaveProgress();
        }
    }
}
