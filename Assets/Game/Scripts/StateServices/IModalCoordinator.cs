using System;

namespace Game.Scripts.StateServices
{
    public interface IModalCoordinator
    {
        ModalType CurrentModal { get; }
        event Action<ModalType> ModalOpened;
        event Action<ModalType> ModalClosed;
        ModalShowResult RequestShow(ModalType type, ModalPriority priority, Action showAction);
        void NotifyClosed(ModalType type);
        void Reset();
    }
}