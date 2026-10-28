using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Scripts.Characters.Player;
using Game.Scripts.Reward;
using Game.Scripts.Save;
using Game.Scripts.Shop;
using Game.Scripts.StateServices;
using Game.Scripts.UI;
using Game.Scripts.Wave;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Game.Scripts.Levels
{
    public class LevelPanels : MonoBehaviour
    {
        private readonly CompositeDisposable _disposables = new();

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
        [SerializeField] private TapToStartZone _tapToStart;

        [Header("Animation Settings")]
        [SerializeField] private float _fadeDuration = 0.5f;
        [SerializeField] private float _scaleDuration = 0.3f;
        [SerializeField] private Ease _ease = Ease.OutBack;

        private bool _isWinRewardFlowRunning;
        private bool _isTransitioning;

        private ILevelService _levelService;
        private LeaderboardService _leaderboardService;
        private WaveSystem _waveSystem;
        private ISaveSystem _saveSystem;
        private Player _currentPlayer;
        private Sequence _currentAnimation;
        private IPauseService _pauseService;
        private IModalCoordinator _modalCoordinator;
        private RewardFacade _rewardFacade;
        private LevelSessionService _levelSessionService;
        private UpgradeChoicePanel _upgradeChoicePanel;
        private ICurrencyService _currencyService;

        [Inject]
        private void Construct(
            ILevelService levelService,
            WaveSystem waveSystem,
            ISaveSystem saveSystem,
            IPauseService pauseService,
            IModalCoordinator modalCoordinator,
            LeaderboardService leaderboardService,
            RewardFacade rewardFacade,
            LevelSessionService levelSessionService,
            UpgradeChoicePanel upgradeChoicePanel,
            ICurrencyService currencyService)
        {
            _levelService = levelService;
            _waveSystem = waveSystem;
            _saveSystem = saveSystem;
            _pauseService = pauseService;
            _modalCoordinator = modalCoordinator;
            _leaderboardService = leaderboardService;
            _rewardFacade = rewardFacade;
            _levelSessionService = levelSessionService;
            _upgradeChoicePanel = upgradeChoicePanel;
            _currencyService = currencyService;
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

            if (_waveSystem != null)
                _waveSystem.AllWavesCompleted += OnOpenWinPanel;

            MessageBroker.Default.Receive<M_PlayerSpawned>()
                .Subscribe(msg => OnPlayerSpawned(msg.Player))
                .AddTo(_disposables);

            MessageBroker.Default.Receive<M_PlayerDeath>()
                .Subscribe(msg => OnPlayerDeath(msg.Player))
                .AddTo(_disposables);
        }

        private void OnDisable()
        {
            if (_waveSystem != null)
                _waveSystem.AllWavesCompleted -= OnOpenWinPanel;

            _disposables.Clear();
            _currentAnimation?.Kill();

            UnsubscribeButtons();

            if (_modalCoordinator != null)
            {
                if (_modalCoordinator.CurrentModal == ModalType.Win)
                    _pauseService?.Resume(this);

                if (_modalCoordinator.CurrentModal == ModalType.Lose)
                    _pauseService?.Resume(this);
            }

            UnsubscribeModalEvents();

            _pauseService?.Reset();
            _modalCoordinator?.Reset();

            _isWinRewardFlowRunning = false;

            if (_rewardButtonRestartLevel != null)
                _rewardButtonRestartLevel.interactable = true;

            if (_rewardButtonRestartLevelLose != null)
                _rewardButtonRestartLevelLose.interactable = true;
        }
        
        private void RollbackLevelProgress()
        {
            if (_currentPlayer == null)
                return;
            
            _saveSystem.RestartStage();
            _upgradeChoicePanel?.ResetPendingState();
        }

        private void SubscribeButtons()
        {
            if (_buttonNextLevel != null)
                _buttonNextLevel.onClick.AddListener(OnNextLevelClicked);

            if (_buttonPause != null)
                _buttonPause.onClick.AddListener(OnOpenPausePanel);

            if (_buttonPauseResumeLevel != null)
                _buttonPauseResumeLevel.onClick.AddListener(OnResumeButtonClick);

            if (_buttonRestartLevel != null)
                _buttonRestartLevel.onClick.AddListener(RestartFromFirstWave);

            if (_buttonRestartLevelLose != null)
                _buttonRestartLevelLose.onClick.AddListener(RestartFromFirstWave);

            if (_buttonPauseRestartLevel != null)
                _buttonPauseRestartLevel.onClick.AddListener(RestartFromFirstWave);

            if (_rewardButtonRestartLevel != null)
                _rewardButtonRestartLevel.onClick.AddListener(OnWinBonusRewardClicked);

            if (_rewardButtonRestartLevelLose != null)
                _rewardButtonRestartLevelLose.onClick.AddListener(OnReviveRewardClicked);
        }

        private void UnsubscribeButtons()
        {
            if (_buttonNextLevel != null)
                _buttonNextLevel.onClick.RemoveListener(OnNextLevelClicked);

            if (_buttonPause != null)
                _buttonPause.onClick.RemoveListener(OnOpenPausePanel);

            if (_buttonPauseResumeLevel != null)
                _buttonPauseResumeLevel.onClick.RemoveListener(OnResumeButtonClick);

            if (_buttonRestartLevel != null)
                _buttonRestartLevel.onClick.RemoveListener(RestartFromFirstWave);

            if (_buttonRestartLevelLose != null)
                _buttonRestartLevelLose.onClick.RemoveListener(RestartFromFirstWave);

            if (_buttonPauseRestartLevel != null)
                _buttonPauseRestartLevel.onClick.RemoveListener(RestartFromFirstWave);

            if (_rewardButtonRestartLevel != null)
                _rewardButtonRestartLevel.onClick.RemoveListener(OnWinBonusRewardClicked);

            if (_rewardButtonRestartLevelLose != null)
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
            if (_buttonPause != null && _modalCoordinator != null)
                _buttonPause.interactable = _modalCoordinator.CurrentModal != ModalType.Upgrade;
        }

        private async void OnNextLevelClicked()
        {
            if (_isTransitioning || _isWinRewardFlowRunning || _upgradeChoicePanel.HasPendingChoices) return;
            _isTransitioning = true;
            _saveSystem.CommitSessionProgress();
            await _levelService.LoadNextLevelAsync();
        }

        private void OnPlayerSpawned(Player player)
        {
            _currentPlayer = player;
        }

        private void OnOpenPausePanel()
        {
            if (_modalCoordinator == null || _modalCoordinator.CurrentModal == ModalType.Upgrade)
                return;

            _modalCoordinator.RequestShow(
                ModalType.Pause,
                ModalPriority.Pause,
                () => ShowPausePanelAsync().Forget());
        }

        private async UniTaskVoid ShowPausePanelAsync()
        {
            _tapToStart?.SetPauseBlocking(true);
            _pauseService.Pause(this);
            _pausePanel.transform.SetAsLastSibling();

            await UIPanelAnimator.Show(_pausePanel, _pausePanelCanvasGroup, _fadeDuration, _scaleDuration, _ease);
        }

        public void OnResumeButtonClick()
        {
            HidePanelAnimated(_pausePanel, _pausePanelCanvasGroup);
            _pauseService.Resume(this);
            _modalCoordinator.NotifyClosed(ModalType.Pause);
            _tapToStart?.SetPauseBlocking(false);
        }

        private void OnOpenWinPanel()
        {
            if (_currentPlayer != null && _currentPlayer.IsDead)
                return;

            _saveSystem.CommitSessionProgress();
            WriteOnLeaderboardFinished();

            _isWinRewardFlowRunning = false;

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
            
            _saveSystem.CommitSessionProgress();
        }

        private void OnPlayerDeath(Player player)
        {
            if (_currentPlayer == null || _currentPlayer != player)
                return;
            
            HandlePlayerDeath();
            
            if (_rewardButtonRestartLevelLose != null)
                _rewardButtonRestartLevelLose.interactable = true;

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
            TryProcessReviveRewardAsync().Forget();
        }

        private async UniTaskVoid TryProcessReviveRewardAsync()
        {
            if (_rewardFacade == null)
            {
                Debug.LogError("[LevelPanels] RewardFacade is not injected.");
                return;
            }

            if (_currentPlayer == null || !_currentPlayer.IsDead)
                return;

            if (_rewardButtonRestartLevelLose != null)
                _rewardButtonRestartLevelLose.interactable = false;

            try
            {
                bool revived = await _rewardFacade.TryReviveAsync(_currentPlayer);

                if (!revived)
                    return;
                
                RestoreProgressAfterRevive();

                HidePanelAnimated(_losePanel, _losePanelCanvasGroup);
                _modalCoordinator.NotifyClosed(ModalType.Lose);
                _pauseService.Resume(this);
            }
            finally
            {
                if (_rewardButtonRestartLevelLose != null)
                    _rewardButtonRestartLevelLose.interactable = true;
            }
        }

        private void OnWinBonusRewardClicked()
        {
            TryProcessWinRewardAsync().Forget();
        }

        private async UniTaskVoid TryProcessWinRewardAsync()
        {
            if (_rewardFacade == null)
            {
                Debug.LogError("[LevelPanels] RewardFacade is not injected.");
                return;
            }

            if (_currentPlayer == null || _currentPlayer.IsDead)
                return;

            if (_isWinRewardFlowRunning)
                return;

            _isWinRewardFlowRunning = true;

            if (_rewardButtonRestartLevel != null)
                _rewardButtonRestartLevel.interactable = false;

            try
            {
                bool granted = await _rewardFacade.TryWinBonusAsync(_currentPlayer.Experience);

                if (!granted)
                    return;

                HidePanelAnimated(_winPanel, _winPanelCanvasGroup);
                _modalCoordinator.NotifyClosed(ModalType.Win);
                _pauseService.Resume(this);
                await UniTask.WaitUntil(() => !_upgradeChoicePanel.HasPendingChoices);
                _saveSystem.CommitSessionProgress();
                _isTransitioning = true;
                await _levelService.LoadNextLevelAsync();
            }
            finally
            {
                _isWinRewardFlowRunning = false;

                if (_rewardButtonRestartLevel != null)
                    _rewardButtonRestartLevel.interactable = true;
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
            if (panel == null || canvasGroup == null)
                return;

            _currentAnimation?.Kill();

            _currentAnimation = DOTween.Sequence()
                .Join(canvasGroup.DOFade(0f, _fadeDuration).SetUpdate(true))
                .Join(panel.transform.DOScale(0.8f, _scaleDuration).SetEase(Ease.InBack).SetUpdate(true))
                .OnComplete(() => panel.SetActive(false));
        }

        private void HidePanelInstant(GameObject panel, CanvasGroup canvasGroup)
        {
            if (panel == null || canvasGroup == null)
                return;

            _currentAnimation?.Kill();
            _currentAnimation = (Sequence)UIPanelAnimator.Hide(panel, canvasGroup, _fadeDuration, _scaleDuration);
        }
        
        private void HandlePlayerDeath()
        {
            if (_currentPlayer == null)
                return;
            
            _saveSystem.RecordDefeat();
            _upgradeChoicePanel?.ResetPendingState();
        }

        private void WriteOnLeaderboardFinished()
        {
            SubmitLeaderboardLevel();
        }

        private void SubmitLeaderboardLevel()
        {
            int level = CampaignRules.Score(_levelService.CurrentLevelIndex, _levelService.Cycle);
            _leaderboardService.TrySubmitIfBest(level);
        }

        private void RestartFromFirstWave()
        {
            _tapToStart?.SetPauseBlocking(false);
            RollbackLevelProgress();

            int currentLevelIndex = _levelService.CurrentLevelIndex;
            _saveSystem.ClearWaveCheckpoint(currentLevelIndex);
            _levelService.RestartCurrentLevelAsync().Forget();
        }
        
        private void RestoreProgressAfterRevive()
        {
            if (_currentPlayer == null)
                return;
            _saveSystem.RestoreAfterRevive();
        }
    }
}
