using System;
using UnityEngine;
using YG;
using YG.Utils.LB;

namespace Game.Scripts.Save
{
    public class LeaderboardService : MonoBehaviour
    {
        [Header("YG Leaderboard")]
        [SerializeField] private string _technoName = "BestLevel";
        [Tooltip("Минимальный интервал между отправками в секундах (в доке: не чаще ~1 раза/сек).")]
        [SerializeField] private float _minSendIntervalSeconds = 1.1f;
        [Tooltip("Авто-открывать окно авторизации перед отправкой, если игрок не авторизован.")]
        [SerializeField] private bool _requestAuthIfNeeded = true;
        
        [Header("Local Cache")]
        [Tooltip("Если true, локально сохраняем best score в PlayerPrefs, чтобы не спамить повторные отправки.")]
        [SerializeField] private bool _useLocalBestCache = true;
        
        private float _lastSendRealtime = -999f;
        private int _bestScore;
        private bool _isSdkReady;
        
        private string LocalBestKey => $"LB_BEST_{_technoName}";
        public int BestScore => _bestScore;
        public bool IsSdkReady => _isSdkReady;
        
        public event Action<int> BestScoreChanged;
        public event Action<int> ScoreSubmitted;
        public event Action<LBData> LeaderboardLoaded;
        private void OnEnable()
        {
            YG2.onGetSDKData += OnSdkDataReady;
            
            if (YG2.isSDKEnabled)
                OnSdkDataReady();
            
            YG2.onGetLeaderboard += OnGetLeaderboard;
        }
        private void OnDisable()
        {
            YG2.onGetSDKData -= OnSdkDataReady;
            YG2.onGetLeaderboard -= OnGetLeaderboard;
        }
        
        private void OnSdkDataReady()
        {
            _isSdkReady = true;
            
            if (_useLocalBestCache)
                _bestScore = Mathf.Max(0, PlayerPrefs.GetInt(LocalBestKey, 0));
            else
                _bestScore = 0;
            
            BestScoreChanged?.Invoke(_bestScore);
            TrySubmitSavedLevelProgress();
        }

        private void TrySubmitSavedLevelProgress()
        {
            if (YG2.isSDKEnabled == false || YG2.saves == null)
                return;

            int savedLevel = Mathf.Max(1, YG2.saves.CurrentLevelIndex + 1);
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
            
            if (_requestAuthIfNeeded && YG2.player.auth == false)
                YG2.OpenAuthDialog();
            
            YG2.SetLeaderboard(_technoName, candidateScore);
            
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
            
            if (_requestAuthIfNeeded && YG2.player.auth == false)
                YG2.OpenAuthDialog();
            
            YG2.SetLeaderboard(_technoName, score);
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
            if (_isSdkReady == false)
                return;
            
            YG2.GetLeaderboard(_technoName);
        }
        
        private void OnGetLeaderboard(LBData data)
        {
            if (data == null)
                return;
            
            if (string.Equals(data.technoName, _technoName, StringComparison.Ordinal))
                LeaderboardLoaded?.Invoke(data);
        }
        
        public void RequestAuthorization()
        {
            if (_isSdkReady == false)
                return;
            
            if (!YG2.player.auth)
                YG2.OpenAuthDialog();
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
    }
}