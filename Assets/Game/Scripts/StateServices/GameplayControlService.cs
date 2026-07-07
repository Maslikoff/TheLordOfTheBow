namespace Game.Scripts.StateServices
{
    public class GameplayControlService : IGameplayControlService
    {
        private readonly IGameStateService _gameState;
        private readonly IPauseService _pause;
        private readonly IModalCoordinator _modals;
        
        public GameplayControlService(
            IGameStateService gameState,
            IPauseService pause,
            IModalCoordinator modals)
        {
            _gameState = gameState;
            _pause = pause;
            _modals = modals;
            
            GameplayControlAccess.Instance = this;
        }
        
        public bool IsBlocked =>
            !_gameState.IsGameStarted
            || _pause.IsPaused
            || _modals.CurrentModal != ModalType.None;
        
        public void Initialize()
        {
            GameplayControlAccess.Instance = this;
        }
    }
}