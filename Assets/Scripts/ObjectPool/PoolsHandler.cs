using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PoolEntry
{
    public string PoolName;
    public GameObject Prefab;
    [Min(0)] public int InitialSize = 10;
    [Min(0)] public int MaxSize;

    public bool AutoPrewarm = true;
}

public class PoolsHandler : MonoBehaviour
{
    [SerializeField] private List<PoolEntry> _pools = new();

    [SerializeField] private bool _initializeOnStart = true;

    private Dictionary<string, IObjectPool<GameObject>> _poolsDictionary = new();

    private void Start()
    {
        if (_initializeOnStart)
            InitializePools();
    }

    public void ClearAllPools()
    {
        foreach (var pool in _poolsDictionary.Values)
        {
            pool?.Clear();
        }
    }

    public GameObject Get(string poolName)
    {
        if (IsPoolExist(poolName, out var pool) == false)
            return null;

        GameObject obj = pool.Get();

        if (obj != null)
            obj.SetActive(true);

        return obj;
    }

    public void Return(string poolName, GameObject obj)
    {
        if (obj == null)
            return;

        if (IsPoolExist(poolName, out var pool) == false)
            return;

        PoolEntry poolEntry = _pools.Find(e => e.PoolName == poolName);

        if (poolEntry is { MaxSize: > 0 } && pool.PooledCount >= poolEntry.MaxSize)
        {
            Destroy(obj);

            return;
        }

        obj.SetActive(false);
        pool.Return(obj);
    }

    private void InitializePools()
    {
        _poolsDictionary.Clear();

        foreach (PoolEntry entry in _pools)
        {
            if (entry.Prefab == null)
            {
                Debug.LogError($"Pool {entry.PoolName} doesn't have a prefab");

                continue;
            }

            PoolConfig config = new PoolConfig()
            {
                InitialSize = entry.InitialSize,
                MaxSize = entry.MaxSize,
                AutoPrewarm = entry.AutoPrewarm,
            };

            //Factory
            var factory = new Func<GameObject>(() =>
            {
                GameObject obj = Instantiate(entry.Prefab);
                obj.SetActive(false);
                obj.transform.SetParent(gameObject.transform);

                return obj;
            });

            var objectPool = new ObjectPool<GameObject>(factory, config);
            _poolsDictionary[entry.PoolName] = objectPool;
        }
    }

    private bool IsPoolExist(string poolName, out IObjectPool<GameObject> pool)
    {
        bool exist = _poolsDictionary.TryGetValue(poolName, out pool);

        if (exist == false)
            Debug.LogError($"Pool {poolName} doesn't have a prefab");

        return exist;
    }
}