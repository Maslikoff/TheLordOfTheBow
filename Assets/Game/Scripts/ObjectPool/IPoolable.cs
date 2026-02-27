using System;

namespace Game.Scripts.ObjectPool
{
    public interface IPoolable
    {
        event Action<IPoolable> Released;
        void Release();
    }
}