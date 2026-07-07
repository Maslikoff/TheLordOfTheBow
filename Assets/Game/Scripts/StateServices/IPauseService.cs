namespace Game.Scripts.StateServices
{
    public interface IPauseService
    {
        /// <summary>Есть ли хотя бы один активный запрос паузы</summary>
        bool IsPaused { get; }
        /// <summary>Запросить паузу. owner — уникальный объект-владелец (обычно this панели)</summary>
        void Pause(object owner);
        /// <summary>Снять паузу для конкретного владельца</summary>
        void Resume(object owner);
        /// <summary>Сброс при перезагрузке сцены / уровня</summary>
        void Reset();
    }
}