using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Scripts.Characters.Player;
using Game.Scripts.Save;
using Game.Scripts.StateServices;
using Game.Scripts.UI;
using Game.Scripts.Wave;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using YG;

namespace Game.Scripts.Levels
{
    public class LevelPanels : MonoBehaviour
    {
        private readonly CompositeDisposable _disposables = new();

        private const string ReviveRewardId = "revive";
        private const string WinBonusRewardId = "win_bonus_xp_next";
        private const int WinBonusXp = 25;

        [Header("Win Panel")]
        [SerializeField] private CanvasGroup _winPanelCanvasGroup;
        [SerializeField] private GameObject _winPanel;
        [SerializeField] private Button _buttonNextLevel;
        [SerializeField] private Button _buttonRestartLevel;
        [SerializeField] private Button _rewardButtonRestartLevel;

        [Header("Lose Panel")]
        [SerializeField] private CanvasGroup _losePanelCanvasGroup;
        [SerializeField] private GameObject _losePanel;
        [SerializeField] private Button _buttonRestartLevelLose;
        [SerializeField] private Button _rewardButtonRestartLevelLose;

        [Header("Pause")]
        [SerializeField] private CanvasGroup _pausePanelCanvasGroup;
        [SerializeField] private GameObject _pausePanel;
        [SerializeField] private Button _buttonPause;
        [SerializeField] private Button _buttonPauseResumeLevel;
        [SerializeField] private Button _buttonPauseRestartLevel;

        [Header("Animation Settings")]
        [SerializeField] private float _fadeDuration = 0.5f;
        [SerializeField] private float _scaleDuration = 0.3f;
        [SerializeField] private Ease _ease = Ease.OutBack;

        private LeaderboardService _leaderboardService;
        private ILevelService _levelService;
        private WaveSystem _waveSystem;
        private IPlayerProvider _playerProvider;
        private ISaveSystem _saveSystem;
        private Player _currentPlayer;
        private Sequence _currentAnimation;
        private IPauseService _pauseService;
        private IModalCoordinator _modalCoordinator;

        private bool _isWinRewardFlowRunning;
        private bool _winRewardGranted;
        private string _pendingRewardId;

        [Inject]
        private void Construct(
            ILevelService levelService,
            WaveSystem waveSystem,
            IPlayerProvider player,
            ISaveSystem saveSystem,
            IPauseService pauseService,
            IModalCoordinator modalCoordinator,
            LeaderboardService leaderboardService)
        {
            _levelService = levelService;
            _waveSystem = waveSystem;
            _playerProvider = player;
            _saveSystem = saveSystem;
            _pauseService = pauseService;
            _modalCoordinator = modalCoordinator;
            _leaderboardService = leaderboardService;
        }

        private void Awake()
        {
            HidePanelInstant(_winPanel, _winPanelCanvasGroup);
            HidePanelInstant(_losePanel, _losePanelCanvasGroup);
            HidePanelInstant(_pausePanel, _pausePanelCanvasGroup);
        }

        private void OnEnable()
        {
            _pauseService?.Reset();
            _modalCoordinator?.Reset();

            SubscribeModalEvents();
            SubscribeButtons();
            UpdatePauseButtonState();

            _waveSystem.AllWavesCompleted += OnOpenWinPanel;

            YG2.onCloseRewardedAdv += OnCloseRewardedAdv;
            YG2.onErrorRewardedAdv += OnErrorRewardedAdv;

            MessageBroker.Default.Receive<M_PlayerSpawned>()
                .Subscribe(msg => OnPlayerSpawned(msg.Player))
                .AddTo(_disposables);

            MessageBroker.Default.Receive<M_PlayerDeath>()
                .Subscribe(msg => OnPlayerDeath(msg.Player))
                .AddTo(_disposables);
        }

        private void OnDisable()
        {
            _waveSystem.AllWavesCompleted -= OnOpenWinPanel;
            _disposables.Clear();
            _currentAnimation?.Kill();

            UnsubscribeButtons();

            if (_modalCoordinator.CurrentModal == ModalType.Win)
                _pauseService.Resume(this);

            if (_modalCoordinator.CurrentModal == ModalType.Lose)
                _pauseService.Resume(this);

            UnsubscribeModalEvents();

            YG2.onCloseRewardedAdv -= OnCloseRewardedAdv;
            YG2.onErrorRewardedAdv -= OnErrorRewardedAdv;

            _pauseService.Reset();
            _modalCoordinator.Reset();

            _isWinRewardFlowRunning = false;
            _winRewardGranted = false;
            _pendingRewardId = null;

            if (_rewardButtonRestartLevel != null)
                _rewardButtonRestartLevel.interactable = true;
            
            if (_rewardButtonRestartLevelLose != null)
                _rewardButtonRestartLevelLose.interactable = true;
        }

        private void SubscribeButtons()
        {
            _buttonNextLevel.onClick.AddListener(OnNextLevelClicked);

            _buttonPause.onClick.AddListener(OnOpenPausePanel);
            _buttonPauseResumeLevel.onClick.AddListener(OnResumeButtonClick);

            _buttonRestartLevel.onClick.AddListener(RestartFromFirstWave);
            _buttonRestartLevelLose.onClick.AddListener(RestartFromFirstWave);
            _buttonPauseRestartLevel.onClick.AddListener(RestartFromFirstWave);

            _rewardButtonRestartLevel.onClick.AddListener(OnWinBonusRewardClicked);
            _rewardButtonRestartLevelLose.onClick.AddListener(OnReviveRewardClicked);
        }

        private void UnsubscribeButtons()
        {
            _buttonNextLevel.onClick.RemoveListener(OnNextLevelClicked);

            _buttonPause.onClick.RemoveListener(OnOpenPausePanel);
            _buttonPauseResumeLevel.onClick.RemoveListener(OnResumeButtonClick);

            _buttonRestartLevel.onClick.RemoveListener(RestartFromFirstWave);
            _buttonRestartLevelLose.onClick.RemoveListener(RestartFromFirstWave);
            _buttonPauseRestartLevel.onClick.RemoveListener(RestartFromFirstWave);

            _rewardButtonRestartLevel.onClick.RemoveListener(OnWinBonusRewardClicked);
            _rewardButtonRestartLevelLose.onClick.RemoveListener(OnReviveRewardClicked);
        }

        private void SubscribeModalEvents()
        {
            if (_modalCoordinator == null)
                return;

            _modalCoordinator.ModalOpened += OnModalStateChanged;
            _modalCoordinator.ModalClosed += OnModalStateChanged;
        }

        private void UnsubscribeModalEvents()
        {
            if (_modalCoordinator == null)
                return;

            _modalCoordinator.ModalOpened -= OnModalStateChanged;
            _modalCoordinator.ModalClosed -= OnModalStateChanged;
        }

        private void OnModalStateChanged(ModalType _)
        {
            UpdatePauseButtonState();
        }

        private void UpdatePauseButtonState()
        {
            if (_buttonPause != null)
                _buttonPause.interactable = _modalCoordinator.CurrentModal != ModalType.Upgrade;
        }

        private async void OnNextLevelClicked()
        {
            int currentLevelIndex = _levelService.CurrentLevelIndex;
            _saveSystem.ClearWaveCheckpoint(currentLevelIndex);

            _saveSystem.SavePlayerProgress();
            await _levelService.LoadNextLevelAsync();
            _saveSystem.SaveGameData();
        }

        private void OnPlayerSpawned(Player player)
        {
            _currentPlayer = player;
        }

        private void OnOpenPausePanel()
        {
            if (_modalCoordinator.CurrentModal == ModalType.Upgrade)
                return;

            _modalCoordinator.RequestShow(
                ModalType.Pause,
                ModalPriority.Pause,
                () => ShowPausePanelAsync().Forget());
        }

        private async UniTaskVoid ShowPausePanelAsync()
        {
            _pauseService.Pause(this);
            _pausePanel.transform.SetAsLastSibling();

            await UIPanelAnimator.Show(_pausePanel, _pausePanelCanvasGroup, _fadeDuration, _scaleDuration, _ease);
        }

        private void OnResumeButtonClick()
        {
            HidePanelAnimated(_pausePanel, _pausePanelCanvasGroup);
            _pauseService.Resume(this);
            _modalCoordinator.NotifyClosed(ModalType.Pause);
        }

        private void OnOpenWinPanel()
        {
            if (_currentPlayer != null && _currentPlayer.IsDead)
                return;

            WriteOnLeaderboardFinished();

            _isWinRewardFlowRunning = false;
            _winRewardGranted = false;
            _pendingRewardId = null;
            if (_rewardButtonRestartLevel != null)
                _rewardButtonRestartLevel.interactable = true;

            _modalCoordinator.RequestShow(
                ModalType.Win,
                ModalPriority.GameOver,
                () => ShowWinPanelAsync().Forget());
        }

        private async UniTaskVoid ShowWinPanelAsync()
        {
            _pauseService.Pause(this);
            _winPanel.transform.SetAsLastSibling();

            await UIPanelAnimator.Show(_winPanel, _winPanelCanvasGroup, _fadeDuration, _scaleDuration, _ease);

            _saveSystem.SavePlayerProgress();
        }

        private void OnPlayerDeath(Player player)
        {
            if (_currentPlayer == null || _currentPlayer != player)
                return;

            WriteOnLeaderboardFinished();

            if (_rewardButtonRestartLevelLose != null)
                _rewardButtonRestartLevelLose.interactable = true;
            _pendingRewardId = null;

            _modalCoordinator.RequestShow(
                ModalType.Lose,
                ModalPriority.GameOver,
                () => ShowLosePanelAsync().Forget());
        }

        private async UniTaskVoid ShowLosePanelAsync()
        {
            _pauseService.Pause(this);
            _losePanel.transform.SetAsLastSibling();

            await UIPanelAnimator.Show(_losePanel, _losePanelCanvasGroup, _fadeDuration, _scaleDuration, _ease);
        }

        private void OnReviveRewardClicked()
        {
            if (_currentPlayer == null || !_currentPlayer.IsDead)
                return;

            _pendingRewardId = ReviveRewardId;

            if (_rewardButtonRestartLevelLose != null)
                _rewardButtonRestartLevelLose.interactable = false;

            YG2.RewardedAdvShow(ReviveRewardId, OnReviveRewardGranted);
        }

        private void OnReviveRewardGranted()
        {
            _pendingRewardId = null;

            if (_rewardButtonRestartLevelLose != null)
                _rewardButtonRestartLevelLose.interactable = true;

            _currentPlayer.TryRevive(0.4f);

            HidePanelAnimated(_losePanel, _losePanelCanvasGroup);
            _pauseService.Resume(this);
            _modalCoordinator.NotifyClosed(ModalType.Lose);
        }

        private void OnWinBonusRewardClicked()
        {
            if (_currentPlayer == null || _currentPlayer.IsDead)
                return;

            if (_isWinRewardFlowRunning)
                return;

            _isWinRewardFlowRunning = true;
            _winRewardGranted = false;
            _pendingRewardId = WinBonusRewardId;

            if (_rewardButtonRestartLevel != null)
                _rewardButtonRestartLevel.interactable = false;

            YG2.RewardedAdvShow(WinBonusRewardId, OnWinBonusRewardGranted);
        }

        private void OnWinBonusRewardGranted()
        {
            _winRewardGranted = true;
            _pendingRewardId = null;
            ProcessWinRewardAndGoNextAsync().Forget();
        }

        private async UniTaskVoid ProcessWinRewardAndGoNextAsync()
        {
            try
            {
                if (_currentPlayer == null || _currentPlayer.IsDead)
                    return;

                _currentPlayer.Experience.AddExperience(WinBonusXp);
                _saveSystem.SavePlayerProgress();

                int currentLevelIndex = _levelService.CurrentLevelIndex;
                _saveSystem.ClearWaveCheckpoint(currentLevelIndex);

                HidePanelAnimated(_winPanel, _winPanelCanvasGroup);
                _pauseService.Resume(this);
                _modalCoordinator.NotifyClosed(ModalType.Win);

                await _levelService.LoadNextLevelAsync();
                _saveSystem.SaveGameData();
            }
            finally
            {
                _isWinRewardFlowRunning = false;
            }
        }

        private void OnCloseRewardedAdv()
        {
            if (_pendingRewardId == WinBonusRewardId && !_winRewardGranted)
            {
                _pendingRewardId = null;
                _isWinRewardFlowRunning = false;

                if (_rewardButtonRestartLevel != null)
                    _rewardButtonRestartLevel.interactable = true;
            }

            if (_pendingRewardId == ReviveRewardId)
            {
                _pendingRewardId = null;

                if (_rewardButtonRestartLevelLose != null)
                    _rewardButtonRestartLevelLose.interactable = true;
            }
        }

        private void OnErrorRewardedAdv()
        {
            if (_pendingRewardId == WinBonusRewardId)
            {
                _pendingRewardId = null;
                _isWinRewardFlowRunning = false;

                if (_rewardButtonRestartLevel != null)
                    _rewardButtonRestartLevel.interactable = true;
            }

            if (_pendingRewardId == ReviveRewardId)
            {
                _pendingRewardId = null;

                if (_rewardButtonRestartLevelLose != null)
                    _rewardButtonRestartLevelLose.interactable = true;
            }
        }

        private async UniTask ShowPanelAnimated(GameObject panel, CanvasGroup canvasGroup, bool disableRaycastsUntilVisible)
        {
            panel.SetActive(true);
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = !disableRaycastsUntilVisible;
            canvasGroup.blocksRaycasts = !disableRaycastsUntilVisible;
            panel.transform.localScale = Vector3.one * 0.8f;

            await DOTween.Sequence()
                .Join(canvasGroup.DOFade(1f, _fadeDuration).SetUpdate(true))
                .Join(panel.transform.DOScale(1f, _scaleDuration).SetEase(_ease).SetUpdate(true))
                .AsyncWaitForCompletion();

            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        private void HidePanelAnimated(GameObject panel, CanvasGroup canvasGroup)
        {
            if (panel == null || canvasGroup == null) return;

            _currentAnimation?.Kill();

            _currentAnimation = DOTween.Sequence()
                .Join(canvasGroup.DOFade(0f, _fadeDuration))
                .Join(panel.transform.DOScale(0.8f, _scaleDuration).SetEase(Ease.InBack))
                .OnComplete(() => panel.SetActive(false));
        }

        private void HidePanelInstant(GameObject panel, CanvasGroup canvasGroup)
        {
            if (panel == null || canvasGroup == null) return;

            _currentAnimation?.Kill();
            _currentAnimation = (Sequence)UIPanelAnimator.Hide(panel, canvasGroup, _fadeDuration, _scaleDuration);
        }

        private void WriteOnLeaderboardFinished()
        {
            int score = _waveSystem.CurrentWaveIndex;
            _leaderboardService.TrySubmitIfBest(score);
        }

        private void RestartFromFirstWave()
        {
            int currentLevelIndex = _levelService.CurrentLevelIndex;
            _saveSystem.ClearWaveCheckpoint(currentLevelIndex);
            _levelService.RestartCurrentLevelAsync().Forget();
        }
    }
}