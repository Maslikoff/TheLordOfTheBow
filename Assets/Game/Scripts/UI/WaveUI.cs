using Game.Scripts.Wave;
using UnityEngine;
using TMPro;

namespace Game.Scripts.UI
{
    public class WaveUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _waveNumberText;
        [SerializeField] private TextMeshProUGUI _enemiesCountText;
        [SerializeField] private WaveSystem _waveSystem;

        [Header("Format Strings")] 
        [SerializeField] private string _waveFormat = "Wave: {0}/{1}";
        [SerializeField] private string _enemiesFormat = "Enemies: {0}/{1} (Alive: {2})";

        private void Start()
        {
            if (_waveSystem != null)
            {
                _waveSystem.WaveStarted += OnWaveStarted;
                _waveSystem.WaveCompleted += OnWaveCompleted;
                _waveSystem.EnemiesCountChanged += OnEnemiesCountChanged;
                _waveSystem.AllWavesCompleted += OnAllWavesCompleted;

                UpdateWaveText(_waveSystem.CurrentWaveIndex, _waveSystem.TotalWaves);
                 UpdateEnemiesText(
                     _waveSystem.TotalEnemiesInWave - _waveSystem.EnemiesRemaining,
                     _waveSystem.TotalEnemiesInWave, 
                     _waveSystem.EnemiesRemaining
                     );
            }
        }

        private void OnDestroy()
        {
            if (_waveSystem != null)
            {
                _waveSystem.WaveStarted -= OnWaveStarted;
                _waveSystem.WaveCompleted -= OnWaveCompleted;
                _waveSystem.EnemiesCountChanged -= OnEnemiesCountChanged;
                _waveSystem.AllWavesCompleted -= OnAllWavesCompleted;
            }
        }

        private void OnWaveStarted(int waveIndex)
        {
            UpdateWaveText(waveIndex + 1, _waveSystem.TotalWaves);
        }

        private void OnWaveCompleted(int waveIndex)
        {
            Debug.Log($"Wave {waveIndex + 1} completed!");
        }

        private void OnEnemiesCountChanged(int current, int total, int remaining)
        {
            UpdateEnemiesText(current, total, remaining);
        }

        private void OnAllWavesCompleted()
        {
            if (_waveNumberText != null)
                _waveNumberText.text = "All waves completed!";

            UpdateEnemiesText(0, 0, 0);
        }

        private void UpdateWaveText(int current, int total)
        {
            if (_waveNumberText != null)
                _waveNumberText.text = string.Format(_waveFormat, current, total);
        }

        private void UpdateEnemiesText(int current, int total, int remaining)
        {
            if (_enemiesCountText != null)
                _enemiesCountText.text = string.Format(_enemiesFormat, current, total, remaining);
        }
    }
}