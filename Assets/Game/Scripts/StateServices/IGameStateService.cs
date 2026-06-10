using System;

namespace Game.Scripts.StateServices
{
    public interface IGameStateService
    {
        bool IsGameStarted { get; }
        event Action GameStarted;
        void StartGame();
    }
}