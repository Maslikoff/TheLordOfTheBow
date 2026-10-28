using System;
using JTLStudio.SDK;
using UnityEngine;

namespace Game.Scripts.Save
{
    /// <summary>
    /// Leaderboard submit/load via the JTL SDK. On platforms without leaderboards
    /// (YouTube Playables) SetScore is dropped by the provider and Load returns an empty page.
    /// </summary>
    public class LeaderboardService : MonoBehaviour
    {
        [Header("Leaderboard")]
        [SerializeField] private string _technoName = "BestLevel";
        [Tooltip("Минимальный интервал между отправками в секундах.")]
        [SerializeField] private float _minSendIntervalSeconds = 1.1f;

        [Header("Local Cache")]
        [Tooltip("Локально кешируем лучший счёт в PlayerPrefs, чтобы не спамить повторные отправки.")]
        [SerializeField] private bool _useLocalBestCache = true;

        [Header("Load")]
        [SerializeField] private int _topCount = 10;
        [SerializeField] private int _aroundCount = 5;

        private float _lastSendRealtime = -999f;
        private int _bestScore;
        private bool _isSdkReady;

        private string LocalBestKey => $"LB_BEST_{_technoName}";
        public int BestScore => _bestScore;
        public bool IsSdkReady => _isSdkReady;

        public event Action<int> BestScoreChanged;
        public event Action<int> ScoreSubmitted;
        public event Action<LeaderboardPage> LeaderboardLoaded;

        private void OnEnable()
        {
            Saves.Loaded += OnSdkDataReady;

            if (Saves.IsReady)
                OnSdkDataReady();
        }

        private void OnDisable()
        {
            Saves.Loaded -= OnSdkDataReady;
        }

        private void OnSdkDataReady()
        {
            _isSdkReady = true;

            _bestScore = _useLocalBestCache ? Mathf.Max(0, PlayerPrefs.GetInt(LocalBestKey, 0)) : 0;

            BestScoreChanged?.Invoke(_bestScore);
            TrySubmitSavedLevelProgress();
        }

        private void TrySubmitSavedLevelProgress()
        {
            if (Saves.IsReady == false)
                return;

            int savedLevel = Mathf.Max(1, Saves.Data.CurrentLevelIndex + 1);
            TrySubmitIfBest(savedLevel);
        }

        public bool TrySubmitIfBest(int candidateScore)
        {
            candidateScore = Mathf.Max(0, candidateScore);

            if (_isSdkReady == false)
                return false;

            float now = Time.realtimeSinceStartup;
            if (now - _lastSendRealtime < _minSendIntervalSeconds)
                return false;

            if (candidateScore <= _bestScore)
                return false;

            // Не открываем AuthDialog автоматически — только явная кнопка (п. 1.2.1 Яндекс Игр).
            SubmitScore(candidateScore);

            _lastSendRealtime = now;
            _bestScore = candidateScore;

            if (_useLocalBestCache)
            {
                PlayerPrefs.SetInt(LocalBestKey, _bestScore);
                PlayerPrefs.Save();
            }

            ScoreSubmitted?.Invoke(_bestScore);
            BestScoreChanged?.Invoke(_bestScore);

            return true;
        }

        public bool ForceSubmit(int score)
        {
            score = Mathf.Max(0, score);

            if (_isSdkReady == false)
                return false;

            float now = Time.realtimeSinceStartup;

            if (now - _lastSendRealtime < _minSendIntervalSeconds)
                return false;

            SubmitScore(score);
            _lastSendRealtime = now;

            if (score > _bestScore)
            {
                _bestScore = score;

                if (_useLocalBestCache)
                {
                    PlayerPrefs.SetInt(LocalBestKey, _bestScore);
                    PlayerPrefs.Save();
                }

                BestScoreChanged?.Invoke(_bestScore);
            }

            ScoreSubmitted?.Invoke(score);

            return true;
        }

        public void RefreshLeaderboard()
        {
            if (_isSdkReady == false || JTLSDK.IsCreated == false)
                return;

            if (JTLSDK.Leaderboards.CanLoad == false)
            {
                LeaderboardLoaded?.Invoke(new LeaderboardPage(Array.Empty<LeaderboardEntry>(), null));
                return;
            }

            JTLSDK.Leaderboards.Load(_technoName, _topCount, _aroundCount, page => LeaderboardLoaded?.Invoke(page));
        }

        /// <summary>
        /// Вызывать только с отдельной кнопки «Авторизоваться» (п. 1.2.1). No-op там, где авторизация недоступна.
        /// </summary>
        public void RequestAuthorization()
        {
            if (JTLSDK.IsCreated == false)
                return;

            if (JTLSDK.Player.IsAuthorized == false)
                JTLSDK.Player.Authorize(_ => { });
        }

        public void ResetLocalBest()
        {
            _bestScore = 0;

            if (_useLocalBestCache)
            {
                PlayerPrefs.DeleteKey(LocalBestKey);
                PlayerPrefs.Save();
            }

            BestScoreChanged?.Invoke(_bestScore);
        }

        private void SubmitScore(int score)
        {
            if (JTLSDK.IsCreated)
                JTLSDK.Leaderboards.SetScore(_technoName, score);
        }
    }
}
