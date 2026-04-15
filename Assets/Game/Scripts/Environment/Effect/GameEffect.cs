using System;
using System.Collections;
using Game.Scripts.ObjectPool;
using UnityEngine;

namespace Game.Scripts.Environment.Effect
{
    public class GameEffect : MonoBehaviour, IPoolable
    {
        [SerializeField] private EffectType _effectType;
        [SerializeField] private float _lifeTime = 3f;
        [SerializeField] private ParticleSystem _particleSystem;
        
        private Coroutine _lifeTimeCoroutine;
        
        public EffectType EffectType => _effectType;
        
        public event Action<IPoolable> Released;

        private void OnValidate()
        {
            _particleSystem ??= GetComponent<ParticleSystem>();
        }

        private void OnEnable()
        {
            _particleSystem.Play();

            _lifeTimeCoroutine = StartCoroutine(LifetimeRoutine());
        }

        private void OnDisable()
        {
            if (_lifeTimeCoroutine != null)
            {
                StopCoroutine(_lifeTimeCoroutine);
                _lifeTimeCoroutine = null;
            }
            
            if(_particleSystem != null)
                _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private IEnumerator LifetimeRoutine()
        {
            yield return new WaitForSeconds(_lifeTime);
            Release();
        }

        public void Release()
        {
            Released?.Invoke(this);
        }
    }
}