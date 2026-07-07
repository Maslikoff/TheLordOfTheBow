using System;

namespace Game.Scripts.StateServices
{
    public struct PendingModalRequest
    {
        public ModalType Type;
        public ModalPriority Priority;
        public Action ShowAction;
    }
}