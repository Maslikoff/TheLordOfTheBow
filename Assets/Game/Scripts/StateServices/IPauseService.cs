namespace Game.Scripts.StateServices
{
    public interface IPauseService
    {
        bool IsPaused { get; }

        void Pause(object owner);

        void Resume(object owner);

        void Reset();
    }
}
