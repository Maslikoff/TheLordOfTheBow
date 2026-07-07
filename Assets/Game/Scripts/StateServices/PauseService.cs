using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts.StateServices
{
    public class PauseService : IPauseService
    {
        private readonly HashSet<object> _owners = new();

        public bool IsPaused => _owners.Count > 0;

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
            Time.timeScale = 1f;
        }

        private void ApplyTimeScale()
        {
            Time.timeScale = IsPaused ? 0f : 1f;
        }
    }
}
