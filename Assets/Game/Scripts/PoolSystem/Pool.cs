using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using Game.Scripts.ObjectPool;
using Object = UnityEngine.Object;

namespace Game.Scripts.PoolSystem
{
    public class Pool<T> : IPool where T : Component, IPoolable
    {
        private readonly Stack<T> _stack = new();

        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly int _maxSize;
        private readonly IObjectResolver _container;

        public Pool(T prefab, Transform parent, int initialSize, int maxSize, IObjectResolver container)
        {
            _prefab = prefab ? prefab : throw new ArgumentNullException(nameof(prefab));
            _parent = parent;
            _maxSize = maxSize;
            _container = container;

            Prewarm(initialSize);
        }

        public IPoolable Get()
        {
            T item = _stack.Count > 0 ? _stack.Pop() : Create();

            if (_container != null)
                InjectDependencies(item.gameObject);

            item.gameObject.SetActive(true);

            return item;
        }

        public void Release(IPoolable poolable)
        {
            if (poolable == null)
                return;

            if (poolable is not T item)
                return;

            var gameObject = item.gameObject;

            gameObject.SetActive(false);

            if (_maxSize > 0 && _stack.Count >= _maxSize)
            {
                Object.Destroy(gameObject);
                return;
            }

            _stack.Push(item);
        }

        private T Create()
        {
            T instance = Object.Instantiate(_prefab, _parent);
            instance.gameObject.SetActive(false);

            if (_container != null)
                InjectDependencies(instance.gameObject);

            return instance;
        }

        private void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
                _stack.Push(Create());
        }

        public void Clear()
        {
            while (_stack.Count > 0)
            {
                T item = _stack.Pop();

                if (item)
                    Object.Destroy(item.gameObject);
            }
        }

        private void InjectDependencies(GameObject gameObject)
        {
            if (_container == null)
                return;

            var monoBehaviours = gameObject.GetComponents<MonoBehaviour>();
            foreach (var mb in monoBehaviours)
            {
                if (mb != null)
                    _container.Inject(mb);
            }
        }
    }
}