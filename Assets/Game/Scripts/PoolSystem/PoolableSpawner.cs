using System;
using UnityEngine;
using Game.Scripts.ObjectPool;

namespace Game.Scripts.PoolSystem
{
    [Serializable]
    public class PoolableSpawner<T> where T : Component, IPoolable
    {
        [SerializeField] private PoolConfig<T> _config;
    
        private bool _isInitialized = false;
        
        public bool IsInitialized { get; private set; }
    
        public void Initialize()
        {
            if (_isInitialized)
                return;
    
            PoolService.Instance.TryCreatePool(
                _config.PoolType,
                _config.Prefab,
                _config.Parent,
                _config.InitialSize,
                _config.MaxSize
            );
    
            _isInitialized = true;
        }
    
        public T Spawn(Vector3 position)
        {
            if (_isInitialized == false)
                throw new Exception(nameof(_isInitialized));
    
            T item = PoolService.Instance.Get<T>(_config.PoolType);
    
            if (item == null)
                return null;
    
            item.Released += OnReleased;
            item.transform.position = position;
    
            return item;
        }
    
        private void OnReleased(IPoolable poolable)
        {
            poolable.Released -= OnReleased;
    
            PoolService.Instance.Release(_config.PoolType, poolable);
        }
    }
}