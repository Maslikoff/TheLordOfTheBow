using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts.StateServices
{
    public class PauseService : IPauseService
    {
        private readonly HashSet<object> _owners = new();
        
        public bool IsPaused => _owners.Count > 0;
        
        public event Action<bool> PauseChanged;
        
        public void Pause(object owner)
        {
            if (owner == null) 
                return;
            
            if (_owners.Add(owner))
                ApplyTimeScale();
        }

        public void Resume(object owner)
        {
            if (owner == null) 
                return;
            
            if (_owners.Remove(owner))
                ApplyTimeScale();
        }

        public void Reset()
        {
            _owners.Clear();
            ApplyTimeScale();
        }
        
        private void ApplyTimeScale()
        {
            bool paused = IsPaused;
            Time.timeScale = paused ? 0f : 1f;
            
            PauseChanged?.Invoke(paused);
        }
    }
}