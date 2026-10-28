using Game.Scripts.Wave;
using UnityEngine;
using TMPro;
using Game.Localization;

namespace Game.Scripts.UI
{
    public class WaveUI : MonoBehaviour
    {
        [VContainer.Inject] private Game.Scripts.Levels.ILevelService _levels;
        [SerializeField] private TextMeshProUGUI _waveNumberText;
        [SerializeField] private TextMeshProUGUI _enemiesCountText;
        [SerializeField] private WaveSystem _waveSystem;

        [Header("Format Strings")]
        [SerializeField] private string _waveFormat = "{0}/{1}";
        [SerializeField] private string _enemiesFormat = "{0}/{1}";
        [SerializeField] private bool _useLocalizedFormats;

        [Header("Localized Wave Formats")]
        [SerializeField] private string _waveFormatRu = "Волна: {0}/{1}";
        [SerializeField] private string _waveFormatEn = "Wave: {0}/{1}";
        [SerializeField] private string _waveFormatTr = "Dalga: {0}/{1}";

        [Header("Localized Enemies Formats")]
        [SerializeField] private string _enemiesFormatRu = "Врагов: {0}/{1} (Осталось: {2})";
        [SerializeField] private string _enemiesFormatEn = "Enemies: {0}/{1} (Alive: {2})";
        [SerializeField] private string _enemiesFormatTr = "Düşmanlar: {0}/{1} (Kalan: {2})";

        private int _lastCurrent;
        private int _lastTotal;
        private int _lastRemaining;
        private int _lastWaveCurrent;
        private int _lastWaveTotal;

        private void OnEnable()
        {
            Loc.Changed += OnLanguageSwitched;

            if (_waveSystem != null)
            {
                _waveSystem.WaveStarted += OnWaveStarted;
                _waveSystem.WaveCompleted += OnWaveCompleted;
                _waveSystem.EnemiesCountChanged += OnEnemiesCountChanged;
                _waveSystem.AllWavesCompleted += OnAllWavesCompleted;

                RefreshFromWaveState();
            }
        }

        private void OnDisable()
        {
            Loc.Changed -= OnLanguageSwitched;

            if (_waveSystem != null)
            {
                _waveSystem.WaveStarted -= OnWaveStarted;
                _waveSystem.WaveCompleted -= OnWaveCompleted;
                _waveSystem.EnemiesCountChanged -= OnEnemiesCountChanged;
                _waveSystem.AllWavesCompleted -= OnAllWavesCompleted;
            }
        }

        private void OnLanguageSwitched(string _)
        {
            UpdateWaveText(_lastWaveCurrent, _lastWaveTotal);
            UpdateEnemiesText(_lastCurrent, _lastTotal, _lastRemaining);
        }
        
        private void RefreshFromWaveState()
        {
            if (_waveSystem == null)
                return;
            
            UpdateWaveText(_waveSystem.UiWaveIndex, _waveSystem.TotalWaves);
            
            if (_waveSystem.IsWaveInProgress == false)
            {
                UpdateEnemiesText(0, _waveSystem.UiWaveEnemyTotal, 0);
                return;
            }
            
            int remaining = _waveSystem.EnemiesRemaining;
            UpdateEnemiesText(remaining, _waveSystem.TotalEnemiesInWave, remaining);
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
            UpdateEnemiesText(0, 0, 0);
        }

        private void UpdateWaveText(int current, int total)
        {
            _lastWaveCurrent = current;
            _lastWaveTotal = total;
            if (_waveNumberText != null)
                _waveNumberText.text = _levels == null ? string.Format(GetWaveFormat(), current, total) : Loc.Lang switch
                {
                    "en" => $"Cycle {_levels.Cycle + 1} · Stage {_levels.CurrentLevel}\nWave {current}/{total}",
                    "tr" => $"Tur {_levels.Cycle + 1} · Bölüm {_levels.CurrentLevel}\nDalga {current}/{total}",
                    _ => $"Круг {_levels.Cycle + 1} · Этап {_levels.CurrentLevel}\nВолна {current}/{total}"
                };
        }

        private void UpdateEnemiesText(int current, int total, int remaining)
        {
            _lastCurrent = current;
            _lastTotal = total;
            _lastRemaining = remaining;

            if (_enemiesCountText == null)
                return;

            _enemiesCountText.text = string.Format(GetEnemiesFormat(), current, total, remaining);
        }

        private string GetWaveFormat()
        {
            if (!_useLocalizedFormats)
                return _waveFormat;

            return Loc.Lang switch
            {
                "en" => _waveFormatEn,
                "tr" => _waveFormatTr,
                _ => _waveFormatRu
            };
        }

        private string GetEnemiesFormat()
        {
            if (!_useLocalizedFormats)
                return _enemiesFormat;

            return Loc.Lang switch
            {
                "en" => _enemiesFormatEn,
                "tr" => _enemiesFormatTr,
                _ => _enemiesFormatRu
            };
        }
    }
}
