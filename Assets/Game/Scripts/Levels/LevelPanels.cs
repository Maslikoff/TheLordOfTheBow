using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Scripts.Characters.Player;
using Game.Scripts.Save;
using Game.Scripts.StateServices;
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
        
        [Header("Win Panel")] 
        [SerializeField] private CanvasGroup _winPanelCanvasGroup;
        [SerializeField] private GameObject _winPanel;
        [SerializeField] private Button _buttonNextLevel;
        [SerializeField] private Button _buttonRestartLevel;

        [Header("Lose Panel")]
        [SerializeField] private CanvasGroup _losePanelCanvasGroup;
        [SerializeField] private GameObject _losePanel;
        [SerializeField] private Button _buttonRestartLevelLose;

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

        private ILevelService _levelService;
        private WaveSystem _waveSystem;
        private IPlayerProvider _playerProvider;
        private ISaveSystem _saveSystem;
        private Player _currentPlayer;
        private Sequence _currentAnimation;
        private IPauseService _pauseService;
        private IModalCoordinator _modalCoordinator;

        [Inject]
        private void Construct(
            ILevelService levelService,
            WaveSystem waveSystem,
            IPlayerProvider player,
            ISaveSystem saveSystem,
            IPauseService pauseService,
            IModalCoordinator modalCoordinator)
        {
            _levelService = levelService;
            _waveSystem = waveSystem;
            _playerProvider = player;
            _saveSystem = saveSystem;
            _pauseService = pauseService;
            _modalCoordinator = modalCoordinator;
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
            
            _pauseService.Reset();
            _modalCoordinator.Reset();
        }

        private void SubscribeButtons()
        {
            _buttonNextLevel.onClick.AddListener(OnNextLevelClicked);
            _buttonRestartLevel.onClick.AddListener(() => _levelService.RestartCurrentLevelAsync());
            _buttonRestartLevelLose.onClick.AddListener(() => _levelService.RestartCurrentLevelAsync());

            _buttonPause.onClick.AddListener(OnOpenPausePanel);
            _buttonPauseResumeLevel.onClick.AddListener(OnResumeButtonClick);
            _buttonPauseRestartLevel.onClick.AddListener(() => _levelService.RestartCurrentLevelAsync());
        }

        private void UnsubscribeButtons()
        {
            _buttonNextLevel.onClick.RemoveListener(OnNextLevelClicked);
            _buttonRestartLevel.onClick.RemoveAllListeners();
            _buttonRestartLevelLose.onClick.RemoveAllListeners();
            _buttonPause.onClick.RemoveListener(OnOpenPausePanel);
            _buttonPauseResumeLevel.onClick.RemoveListener(OnResumeButtonClick);
            _buttonPauseRestartLevel.onClick.RemoveAllListeners();
        }
        
        private void SubscribeModalEvents()
        {
            _modalCoordinator.ModalOpened += _ => UpdatePauseButtonState();
            _modalCoordinator.ModalClosed += _ => UpdatePauseButtonState();
        }
        private void UnsubscribeModalEvents()
        {
            _modalCoordinator.ModalOpened -= _ => UpdatePauseButtonState();
            _modalCoordinator.ModalClosed -= _ => UpdatePauseButtonState();
        }
        private void UpdatePauseButtonState()
        {
            if (_buttonPause != null)
                _buttonPause.interactable = _modalCoordinator.CurrentModal != ModalType.Upgrade;
        }

        private async void OnNextLevelClicked()
        {
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
            await ShowPanelAnimated(_pausePanel, _pausePanelCanvasGroup, false);
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

            _modalCoordinator.RequestShow(
                ModalType.Win,
                ModalPriority.GameOver,
                () => ShowWinPanelAsync().Forget());
        }
        
        private async UniTaskVoid ShowWinPanelAsync()
        {
            _pauseService.Pause(this);
            _winPanel.transform.SetAsLastSibling();
            
            await ShowPanelAnimated(_winPanel, _winPanelCanvasGroup, true);
            
            _saveSystem.SavePlayerProgress();
            YG2.InterstitialAdvShow();
        }

        private void OnPlayerDeath(Player player)
        {
            if (_currentPlayer == null || _currentPlayer != player) 
                return;
            
            _modalCoordinator.RequestShow(
                ModalType.Lose,
                ModalPriority.GameOver,
                () => ShowLosePanelAsync().Forget());
        }
        
        private async UniTaskVoid ShowLosePanelAsync()
        {
            _pauseService.Pause(this);
            _losePanel.transform.SetAsLastSibling();
            
            await ShowPanelAnimated(_losePanel, _losePanelCanvasGroup, true);
            
            YG2.InterstitialAdvShow();
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
        
            panel.SetActive(false);
            canvasGroup.alpha = 0f;
            panel.transform.localScale = Vector3.one;
        }
    }
}