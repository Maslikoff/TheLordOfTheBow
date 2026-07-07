using System;

namespace Game.Scripts.StateServices
{
    public interface IModalCoordinator
    {
        ModalType CurrentModal { get; }
        bool HasPendingOrActive { get; }
        ModalShowResult RequestShow(ModalType type, ModalPriority priority, Action showAction);
        void NotifyClosed(ModalType type);
        void Reset();
    }
}