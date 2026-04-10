using Game.Scripts.Characters.PickupObjects;
using UnityEngine;

namespace Game.Scripts.ObjectPool
{
    public class HealthKitPool : ObjectPool<HealthKit>
    {
        [SerializeField] private HealthKit _healthKitPrefab;
        [SerializeField] private int _initialPoolSize = 5;
        
        protected override void Awake()
        {
            base.Awake();
            
            InitializePool();
        }
        
        private void InitializePool()
        {
            for (int i = 0; i < _initialPoolSize; i++)
            {
                HealthKit healthKit = CreateNewObject();
                ReturnToPool(healthKit);
            }
            
            _isInitialized = true;
        }
        
        protected override HealthKit CreateNewObject()
        {
            HealthKit healthKit = Instantiate(_healthKitPrefab, _poolParent);
            healthKit.gameObject.SetActive(false);
            
            healthKit.Released += OnHandleObjectReleased;
            
            return healthKit;
        }
        
        protected override void OnHandleObjectReleased(IPoolable poolable)
        {
            if (poolable is HealthKit healthKit)
                ReturnToPool(healthKit);
        }
        
        public override HealthKit GetFromPool()
        {
            if (_isInitialized == false)
                InitializePool();

            HealthKit healthKit = base.GetFromPool();
            
            healthKit.transform.localPosition = Vector3.zero;
            healthKit.transform.localRotation = Quaternion.identity;
            
            return healthKit;
        }

        public override void ReturnToPool(HealthKit healthKit)
        {
            if (healthKit == null) 
                return;
            
            healthKit.gameObject.SetActive(false);
            healthKit.transform.SetParent(_poolParent);
            healthKit.transform.localPosition = Vector3.zero;
            healthKit.transform.localRotation = Quaternion.identity;
            
            base.ReturnToPool(healthKit);
        }
    }
}