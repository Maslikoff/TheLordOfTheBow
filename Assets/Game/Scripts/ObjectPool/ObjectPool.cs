using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts.ObjectPool
{
    public abstract class ObjectPool<T> : MonoBehaviour where T : Component, IPoolable
    {
        private Queue<T> _pool = new();
        private Transform _poolParent;

        protected bool _isInitialized = false;

        protected virtual void Awake()
        {
            _poolParent = new GameObject($"{typeof(T).Name}Pool").transform;
            _poolParent.SetParent(transform);
        }

        protected virtual void OnDestroy()
        {
            foreach (var obj in _pool)
                if (obj != null)
                    obj.Released -= OnHandleObjectReleased;
        }

        protected void OnObjectGet(T obj)
        {
        }

        private void ObjectReturn(T obj)
        {
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
            ObjectReturn(obj);
        }

        protected abstract T CreateNewObject();

        protected virtual void OnHandleObjectReleased(IPoolable poolable)
        {
            if (poolable is T obj)
                ReturnToPool(obj);
        }
    }
}