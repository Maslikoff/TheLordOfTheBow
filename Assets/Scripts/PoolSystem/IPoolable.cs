using System;

public interface IPoolable
{
    event Action<IPoolable> Released;

    void Release();
}