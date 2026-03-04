using System;
using Game.Scripts.ObjectPool;
using UnityEngine;

namespace Game.Scripts.Environment
{
    public class Ground : MonoBehaviour, IPoolable
    {
        [SerializeField] private float _speed;
        
        private Transform _transform;
        
        public event Action<IPoolable> Released;

        private void Awake()
        {
            _transform = transform;
        }

        private void Update()
        {
            _transform.Translate(Vector3.forward * -(_speed * Time.deltaTime));
        }

        public void Release()
        {
            Released?.Invoke(this);
        }
    }
}