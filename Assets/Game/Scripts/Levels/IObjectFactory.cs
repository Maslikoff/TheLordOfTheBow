using UnityEngine;

namespace Game.Scripts.Levels
{
    public interface IObjectFactory
    {
        T Create<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent = null) 
            where T : Component;

        T Create<T>(T prefab, Transform parent = null)
            where T : Component;
    }
}