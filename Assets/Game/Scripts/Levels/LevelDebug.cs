using Game.Scripts.Characters.Player;
using Game.Scripts.Wave;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Game.Scripts.Levels
{
    public class LevelDebug : MonoBehaviour
    {
        [Header("Win Panel")]
        [SerializeField] private GameObject _winPanel;
        [SerializeField] private Button _buttonNextLevel;
        [SerializeField] private Button _buttonRestartLevel;
        
        [Header("Lose Panel")]
        [SerializeField] private GameObject _losePanel;
        [SerializeField] private Button _buttonRestartLevelLose;
        
        [Header("Pause")]
        [SerializeField] private GameObject _pausePanel;
        [SerializeField] private Button _buttonPause;
        [SerializeField] private Button _buttonPauseResumeLevel;
        [SerializeField] private Button _buttonPauseRestartLevel;

        private ILevelService _levelService;
        private WaveSystem _waveSystem;
        private IPlayerProvider _playerProvider;

        [Inject]
        private void Construct(ILevelService levelService, WaveSystem waveSystem, IPlayerProvider player)
        {
            _levelService = levelService;
            _waveSystem = waveSystem;
            _playerProvider = player;
        }

        private void OnEnable()
        {
            _winPanel.SetActive(false);
            _losePanel.SetActive(false);
            _pausePanel.SetActive(false);
            Time.timeScale = 1;
            
            _buttonNextLevel.onClick.AddListener(() => _levelService.LoadNextLevelAsync());
            _buttonRestartLevel.onClick.AddListener(() => _levelService.RestartCurrentLevelAsync());
            _buttonRestartLevelLose.onClick.AddListener(() => _levelService.RestartCurrentLevelAsync());
            
            _buttonPause.onClick.AddListener(OnOpenPausePanel);
            _buttonPauseResumeLevel.onClick.AddListener(OnResumeButtonClick);
            _buttonPauseRestartLevel.onClick.AddListener(() => _levelService.RestartCurrentLevelAsync());

            _waveSystem.AllWavesCompleted += OnOpenWinPanel;
            _playerProvider.PlayerSpawned += OnPlayerSpawned;
            
            if (_playerProvider.Player != null)
                OnPlayerSpawned(_playerProvider.Player);
        }
        
        private void Start()
        {
            if (_playerProvider.Player != null)
                _playerProvider.Player.PlayerHealth.Death += OnOpenLosePanel;
        }

        private void OnDisable()
        {
            _waveSystem.AllWavesCompleted -= OnOpenWinPanel;
            _playerProvider.PlayerSpawned -= OnPlayerSpawned;
        
            if (_playerProvider.Player != null)
                _playerProvider.Player.PlayerHealth.Death -= OnOpenLosePanel;
        }

        private void OnPlayerSpawned(Player player)
        {
            player.PlayerHealth.Death += OnOpenLosePanel;
        }

        private void OnOpenPausePanel()
        {
            Time.timeScale = 0;
            _pausePanel.SetActive(true);
        }

        private void OnResumeButtonClick()
        {
            Time.timeScale = 1;
            _pausePanel.SetActive(false);
        }

        private void OnOpenWinPanel()
        {
            if (_playerProvider.Player != null && _playerProvider.Player.IsDead)
                return;

            _winPanel.SetActive(true);
            Time.timeScale = 0;
        }

        private void OnOpenLosePanel()
        {
            _losePanel.SetActive(true);
            Time.timeScale = 0;
        }
    }
}