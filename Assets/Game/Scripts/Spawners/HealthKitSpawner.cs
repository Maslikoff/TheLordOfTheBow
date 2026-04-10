using Game.Scripts.Characters.PickupObjects;
using Game.Scripts.ObjectPool;
using UnityEngine;

namespace Game.Scripts.Spawners
{
    public class HealthKitSpawner : Spawner<HealthKit>
    {
        [SerializeField] private HealthKitPool _healthKitPool;

        protected override void OnDisable()
        {
            StopAllCoroutines();
        }

        protected override bool CanSpawn() => base.CanSpawn() && _healthKitPool != null;

        protected override void Initialize()
        {
            if (_healthKitPool == null)
                _healthKitPool = GetComponent<HealthKitPool>();
            
            _objectPool = _healthKitPool;
        }

        protected override void SpawnObject()
        {
            if (_healthKitPool == null)
                return;
            
            HealthKit healthKit = _healthKitPool.GetFromPool();
            
            if (healthKit != null)
            {
                Vector3 spawnPos = GetRandomSpawnPosition();
                spawnPos.y = 1f;
        
                healthKit.transform.position = spawnPos;
                healthKit.transform.rotation = Quaternion.identity;
        
                healthKit.Released += OnHealthKitReleased;
                
                IncreaseObjectCount();
            }
        }
        
        private void OnHealthKitReleased(IPoolable poolable)
        {
            if (poolable is HealthKit healthKit)
            {
                healthKit.Released -= OnHealthKitReleased;
                
                DecreaseObjectCount();
            }
        }
    }
}