using System;
using System.Collections.Generic;

public class ObjectPool<T> : IObjectPool<T> where T : class
{
    private readonly Stack<T> _pool;
    private readonly Func<T> _factory;
    private readonly PoolConfig _config;

    public int PooledCount => _pool.Count;

    public ObjectPool(Func<T> factory, PoolConfig config)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _config = config ?? new PoolConfig();

        int initialCapacity = _config.InitialSize > 0 ? _config.InitialSize : 10;
        _pool = new Stack<T>(initialCapacity);

        if (_config.AutoPrewarm && _config.InitialSize > 0)
            Prewarm(_config.InitialSize);
    }

    public T Get()
    {
        var item = _pool.Count > 0 ? _pool.Pop() : _factory();

        return item;
    }

    public void Return(T obj)
    {
        if (obj == null)
            return;

        if (_config.MaxSize > 0 && _pool.Count >= _config.MaxSize)
        {
            if (obj is IDisposable disposable)
                disposable.Dispose();

            return;
        }

        _pool.Push(obj);
    }

    public void Prewarm(int count)
    {
        if (count <= 0)
            return;

        int targetCount = _config.MaxSize > 0 ? Math.Min(count, _config.MaxSize - _pool.Count) : count;

        for (int i = 0; i < targetCount; i++)
        {
            T item = _factory();
            _pool.Push(item);
        }
    }

    public void Clear()
    {
        while (_pool.Count > 0)
        {
            _pool.Pop();
        }
    }

    public void Dispose()
    {
        Clear();
    }
}