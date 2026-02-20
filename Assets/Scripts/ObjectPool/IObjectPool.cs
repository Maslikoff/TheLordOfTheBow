using System;

public interface IObjectPool<T> : IDisposable where T : class
{
    int PooledCount {  get; }

    T Get();
    void Return(T obj);
    void Prewarm(int count);
    void Clear();
}