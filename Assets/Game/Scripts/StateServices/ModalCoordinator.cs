using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Scripts.StateServices
{
    public class ModalCoordinator : IModalCoordinator
    {
        private readonly List<PendingModalRequest> _queue = new();
        
        private ModalType _currentModal = ModalType.None;
        
        public ModalType CurrentModal => _currentModal;
        public bool HasPendingOrActive => _currentModal != ModalType.None || _queue.Count > 0;
        
        public ModalShowResult RequestShow(ModalType type, ModalPriority priority, Action showAction)
        {
            if (type == ModalType.None || showAction == null)
                return ModalShowResult.AlreadyPending;
            
            if (_currentModal == type)
                return ModalShowResult.AlreadyPending;
            
            if (_queue.Any(r => r.Type == type))
                return ModalShowResult.AlreadyPending;
            
            if (_currentModal != ModalType.None)
            {
                _queue.Add(new PendingModalRequest
                {
                    Type = type,
                    Priority = priority,
                    ShowAction = showAction,
                });
                
                Debug.Log($"[ModalCoordinator] {type} queued (current: {_currentModal})");
                return ModalShowResult.Queued;
            }
            
            ShowImmediate(type, showAction);
            return ModalShowResult.Shown;
        }

        public void NotifyClosed(ModalType type)
        {
            if (_currentModal != type)
            {
                Debug.LogWarning($"[ModalCoordinator] NotifyClosed({type}) but current is {_currentModal}");
                return;
            }
            
            _currentModal = ModalType.None;
            ProcessQueue();
        }

        public void Reset()
        {
            _currentModal = ModalType.None;
            _queue.Clear();
        }
        
        private void ShowImmediate(ModalType type, Action showAction)
        {
            _currentModal = type;
            Debug.Log($"[ModalCoordinator] Showing {type}");
            showAction.Invoke();
        }
        private void ProcessQueue()
        {
            if (_currentModal != ModalType.None || _queue.Count == 0)
                return;
            
            int bestIndex = 0;
            
            for (int i = 1; i < _queue.Count; i++)
            {
                if (_queue[i].Priority > _queue[bestIndex].Priority)
                    bestIndex = i;
            }
            
            var next = _queue[bestIndex];
            _queue.RemoveAt(bestIndex);
            ShowImmediate(next.Type, next.ShowAction);
        }
    }
}