using System;
using UnityEngine;

[Serializable]
public class PoolConfig<T> where T : Component, IPoolable
{
    [SerializeField] private PoolType _poolType;
    [SerializeField] private T _prefab;
    [SerializeField] private Transform _parent;
    [SerializeField] private int _initialSize = 10;
    [SerializeField] private int _maxSize = 50;

    public PoolType PoolType => _poolType;
    public T Prefab => _prefab;
    public Transform Parent => _parent;
    public int InitialSize => _initialSize;
    public int MaxSize => _maxSize;
}