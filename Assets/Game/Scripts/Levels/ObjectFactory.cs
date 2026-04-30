using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Scripts.Levels
{
    public class ObjectFactory : IObjectFactory
    {
        private readonly IObjectResolver _resolver;

        public ObjectFactory(IObjectResolver resolver)
        {
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        }
        
        public T Create<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent = null) 
            where T : Component
        {
            return _resolver.Instantiate(prefab, position, rotation, parent);
        }

        public T Create<T>(T prefab, Transform parent = null) 
            where T : Component
        {
            return _resolver.Instantiate(prefab, parent);
        }
    }
}