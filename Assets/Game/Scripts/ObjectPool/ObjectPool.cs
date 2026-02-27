using System.Collections.Generic;
using Game.Scripts.Characters.Enemy;
using UnityEngine;

namespace Game.Scripts.ObjectPool
{
    public abstract class ObjectPool<T> : MonoBehaviour where T : Component, IPoolable
    {
        [SerializeField] protected T _prefab;
        [SerializeField] protected int _poolSize = 10;

        protected Queue<T> _pool = new Queue<T>();
        protected Transform _poolParent;

        private bool _isInitialized = false;

        public int GetPoolSize() => _pool.Count;

        protected virtual void Awake()
        {
            _poolParent = new GameObject($"{typeof(T).Name}Pool").transform;
            _poolParent.SetParent(transform);
        }

        protected virtual void Start()
        {
            if (_isInitialized == false)
            {
                InitializePool();
                _isInitialized = true;
            }
        }

        protected virtual void OnDestroy()
        {
            foreach (var obj in _pool)
                if (obj != null)
                    obj.Released -= HandleObjectReleased;
        }

        protected virtual void OnObjectGet(T obj)
        {
        }

        protected virtual void OnObjectReturn(T obj)
        {
        }

        protected void InitializePool()
        {
            for (int i = 0; i < _poolSize; i++)
            {
                T obj = CreateNewObject();
                ReturnToPool(obj);
            }
        }

        protected virtual T CreateNewObject()
        {
            T obj = Instantiate(_prefab, _poolParent);
            obj.gameObject.SetActive(false);

            obj.Released += HandleObjectReleased;

            return obj;
        }

        public virtual T GetFromPool()
        {
            T obj;

            if (_pool.Count > 0)
                obj = _pool.Dequeue();
            else
                obj = CreateNewObject();

            obj.gameObject.SetActive(true);
            OnObjectGet(obj);

            return obj;
        }

        public virtual void ReturnToPool(T obj)
        {
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
            OnObjectReturn(obj);
        }

        private void HandleObjectReleased(IPoolable poolable)
        {
            if (poolable is T obj)
                ReturnToPool(obj);
        }
    }
}