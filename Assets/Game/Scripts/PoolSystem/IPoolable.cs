using System;

namespace Game.Scripts.PoolSystem
{
    public interface IPoolable
    {
        event Action<IPoolable> Released;
    
        void Release();
    }
}
