using Game.Scripts.Environment.Effect;
using Game.Scripts.ObjectPool;
using UnityEngine;
using VContainer;

namespace Game.Scripts.Spawners
{
    public class EffectSpawner : Spawner<GameEffect>
    {
        private EffectsPool _effectsPool;
        private IObjectResolver _resolver;

        [Inject]
        public void Construct(IObjectResolver resolver)
        {
            _resolver = resolver;
        }
        
        protected override void Initialize()
        {
            _effectsPool = GetComponent<EffectsPool>();
            
            _objectPool = _effectsPool;
        }
        
        protected override void SpawnObject() { }

        protected override bool CanSpawn() => true;

        public GameEffect SpawnEffect(EffectType type, Vector3 position)
        {
            if (_effectsPool == null)
            {
                Debug.LogError("[EffectSpawner] EffectsPool is null!");
                return null;
            }
            
            GameEffect effect = _effectsPool.GetEffect(type);
            
            if (effect != null)
            {
                effect.transform.position = position;
                effect.transform.rotation = Quaternion.identity;
                
                effect.Released += OnEffectReleased;
                IncreaseObjectCount();
            }
            
            return effect;
        }

        private void OnEffectReleased(IPoolable poolable)
        {
            if (poolable is GameEffect effect)
            {
                effect.Released -= OnEffectReleased;
                DecreaseObjectCount();
            }
        }
    }
}