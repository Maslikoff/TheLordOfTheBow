using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Scripts.PoolSystem
{
    [DisallowMultipleComponent]
    public class PoolService : MonoBehaviour
    {
        public static PoolService Instance { get; private set; }
    
        private readonly Dictionary<PoolType, IPool> _pools = new();
        private IObjectResolver _container;
    
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
    
            Instance = this;
        }
        
        [Inject]
        public void Construct(IObjectResolver container)
        {
            _container = container;
        }
    
        public void TryCreatePool<T>(PoolType poolType, T prefab, Transform parent, int initialSize, int maxSize)
            where T : Component, IPoolable
        {
            if (_pools.ContainsKey(poolType))
                return;
    
            _pools[poolType] = new Pool<T>(prefab, parent, initialSize, maxSize, _container);
        }
    
        public T Get<T>(PoolType poolType)
            where T : Component, IPoolable
        {
            if (_pools.TryGetValue(poolType, out IPool pool) == false)
                return null;
    
            return (T)pool.Get();
        }
    
        public void Release(PoolType poolType, IPoolable item)
        {
            if (item == null)
                return;
    
            if (_pools.TryGetValue(poolType, out IPool pool) == false)
                return;
    
            pool.Release(item);
        }
    }
}
