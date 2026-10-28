using Cysharp.Threading.Tasks;
using Game.Scripts.AddressablesContent;
using Game.Scripts.Characters.PickupObjects;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;
using VContainer.Unity;

namespace Game.Scripts.ObjectPool
{
    public class HealthKitPool : ObjectPool<HealthKit>
    {
        [SerializeField] private AssetReferenceGameObject _healthKitPrefabReference;
        [SerializeField] private int _initialPoolSize = 5;

        private IObjectResolver _resolver;
        private HealthKit _healthKitPrefab;

        [Inject]
        private void Construct(IObjectResolver resolver)
        {
            _resolver = resolver;
        }

        protected override void Awake()
        {
            base.Awake();
        }

        public async UniTask WarmupAsync(IAddressablesContentService content)
        {
            if (_isInitialized)
                return;

            GameObject go = await content.LoadGameObjectAsync(_healthKitPrefabReference);
            _healthKitPrefab = go.GetComponent<HealthKit>();
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
            HealthKit healthKit = _resolver.Instantiate(
                _healthKitPrefab,
                _poolParent.position,
                _poolParent.rotation,
                _poolParent);
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
            {
                Debug.LogError("HealthKitPool is not warmed up.");
                return null;
            }

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
