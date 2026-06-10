using System;
using UnityEngine;

namespace Game.Scripts.StateServices
{
    public class GameStateService : IGameStateService
    {
        public bool IsGameStarted { get; private set; }
        public event Action GameStarted;
    
        public void StartGame()
        {
            if (IsGameStarted) return;
        
            IsGameStarted = true;
            GameStarted?.Invoke();
            Debug.Log("Game Started!");
        }
    }
}