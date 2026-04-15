using System.Collections.Generic;
using Game.Scripts.Environment.Effect;
using UnityEngine;

namespace Game.Scripts.ObjectPool
{
    public class EffectsPool : ObjectPool<GameEffect>
    {
        [SerializeField] private List<EffectConfig> _effectConfigs = new();
        
        private Dictionary<EffectType, Queue<GameEffect>> _typePools = new();
        private Dictionary<EffectType, Transform> _typeParents = new();
        private Dictionary<EffectType, GameEffect> _typePrefabs = new();

        protected override void Awake()
        {
            base.Awake();
            
            InitializePools();
        }

        protected override void OnDestroy()
        {
            foreach (var pool in _typePools.Values)
            {
                foreach (var effect in pool)
                {
                    if (effect != null)
                        effect.Released -= OnHandleObjectReleased;
                }
            }
            
            base.OnDestroy();
        }
        
        protected override GameEffect CreateNewObject() => null;
        
        public override GameEffect GetFromPool() => null;

        private void InitializePools()
        {
            foreach (var config in _effectConfigs)
            {
                if (config.Prefab == null)
                {
                    Debug.LogError($"Prefab for effect type {config.Type} is null!");
                    continue;
                }
                
                _typeParents[config.Type] = _poolParent;
                _typePrefabs[config.Type] = config.Prefab;
                
                var pool = new Queue<GameEffect>();
                _typePools[config.Type] = pool;
                
                for (int i = 0; i < config.PoolSize; i++)
                {
                    GameEffect effect = CreateEffectForType(config.Type);
                    ReturnToTypePool(effect, config.Type);
                }
            }
            
            _isInitialized = true;
        }
        
        public GameEffect GetEffect(EffectType type)
        {
            if (_isInitialized == false)
                InitializePools();
            
            if (_typePools.TryGetValue(type, out Queue<GameEffect> pool) == false)
            {
                Debug.LogError($"No pool for effect type {type}!");
                return null;
            }
            
            GameEffect effect;
            
            if (pool.Count > 0)
                effect = pool.Dequeue();
            else
                effect = CreateEffectForType(type);
            
            if (effect != null)
            {
                effect.gameObject.SetActive(true);
                OnObjectGet(effect);
            }
            
            return effect;
        }
        
        private GameEffect CreateEffectForType(EffectType type)
        {
            if (_typePrefabs.TryGetValue(type, out GameEffect prefab) == false)
            {
                Debug.LogError($"No prefab for effect type {type}!");
                return null;
            }
            
            GameEffect effect = Instantiate(prefab, _typeParents[type]);
            effect.gameObject.SetActive(false);
            effect.Released += OnHandleObjectReleased;
            
            return effect;
        }
        
        private void ReturnToTypePool(GameEffect effect, EffectType type)
        {
            if (effect == null)
                return;
            
            effect.gameObject.SetActive(false);
            effect.transform.SetParent(_typeParents[type]);
            effect.transform.localPosition = Vector3.zero;
            effect.transform.localRotation = Quaternion.identity;
            
            _typePools[type].Enqueue(effect);
        }
        
        protected override void OnHandleObjectReleased(IPoolable poolable)
        {
            if (poolable is GameEffect effect)
                ReturnToTypePool(effect, effect.EffectType);
        }
    }
}