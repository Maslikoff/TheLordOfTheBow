using System;
using System.Collections;
using Game.Scripts.ObjectPool;
using UnityEngine;

namespace Game.Scripts.Characters.PickupObjects
{
    public class HealthKit : MonoBehaviour, IPoolable
    {
        [SerializeField] private int _healCount = 15;
        [SerializeField] private float _healRotateSpeed = 5f;

        private Coroutine _rotationCoroutine;
        private Transform _transform;

        public event Action<IPoolable> Released;

        private void OnEnable()
        {
            StartCoroutine(RotateCoroutine());
        }

        private void Awake()
        {
            _transform = transform;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.TryGetComponent(out Health player))
            {
                if(player.CurrentCount < player.MaxCount)
                {
                    player.Heal(_healCount);
                    Release();
                }
            }
        }

        private void OnDestroy()
        {
            if (_rotationCoroutine != null)
            {
                StopCoroutine(_rotationCoroutine);
                _rotationCoroutine = null;
            }
        }

        public void Release()
        {
            Released?.Invoke(this);
        }

        private IEnumerator RotateCoroutine()
        {
            while (enabled)
            {
                _transform.Rotate(Vector3.up, _healRotateSpeed * Time.deltaTime);
                yield return null;
            }
        }
    }
}