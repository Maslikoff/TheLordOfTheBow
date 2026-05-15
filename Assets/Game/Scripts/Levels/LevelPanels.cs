using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Scripts.Characters.Player;
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
        private Player _currentPlayer;
        private Sequence _currentAnimation;

        [Inject]
        private void Construct(ILevelService levelService, WaveSystem waveSystem, IPlayerProvider player)
        {
            _levelService = levelService;
            _waveSystem = waveSystem;
            _playerProvider = player;
        }

        private void Awake()
        {
            HidePanelInstant(_winPanel, _winPanelCanvasGroup);
            HidePanelInstant(_losePanel, _losePanelCanvasGroup);
            HidePanelInstant(_pausePanel, _pausePanelCanvasGroup);
        }

        private void OnEnable()
        {
            Time.timeScale = 1;
            
            SubscribeButtons();

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
        }

        private void SubscribeButtons()
        {
            _buttonNextLevel.onClick.AddListener(() => _levelService.LoadNextLevelAsync());
            _buttonRestartLevel.onClick.AddListener(() => _levelService.RestartCurrentLevelAsync());
            _buttonRestartLevelLose.onClick.AddListener(() => _levelService.RestartCurrentLevelAsync());

            _buttonPause.onClick.AddListener(OnOpenPausePanel);
            _buttonPauseResumeLevel.onClick.AddListener(OnResumeButtonClick);
            _buttonPauseRestartLevel.onClick.AddListener(() => _levelService.RestartCurrentLevelAsync());
        }

        private void OnPlayerSpawned(Player player)
        {
            _currentPlayer = player;
        }

        private async void OnOpenPausePanel()
        {
            await ShowPanelAnimated(_pausePanel, _pausePanelCanvasGroup);
            Time.timeScale = 0;
        }

        private void OnResumeButtonClick()
        {
            Time.timeScale = 1;
            HidePanelAnimated(_pausePanel, _pausePanelCanvasGroup);
        }

        private async void OnOpenWinPanel()
        {
            if (_playerProvider.Player != null && _playerProvider.Player.IsDead)
                return;

            await ShowPanelAnimated(_winPanel, _winPanelCanvasGroup);
            Time.timeScale = 0;
        }

        private async void OnPlayerDeath(Player player)
        {
            if (_currentPlayer == null || _currentPlayer != player) 
                return;
            
            await ShowPanelAnimated(_losePanel, _losePanelCanvasGroup);
            Time.timeScale = 0;
        }

        private async UniTask ShowPanelAnimated(GameObject panel, CanvasGroup canvasGroup)
        {
            if (panel == null || canvasGroup == null) return;
        
            _currentAnimation?.Kill();
        
            panel.SetActive(true);
            canvasGroup.alpha = 0f;
            panel.transform.localScale = Vector3.one * 0.8f;
        
            await DOTween.Sequence()
                .Join(canvasGroup.DOFade(1f, _fadeDuration).SetUpdate(true))
                .Join(panel.transform.DOScale(1f, _scaleDuration).SetEase(_ease).SetUpdate(true))
                .AsyncWaitForCompletion();
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