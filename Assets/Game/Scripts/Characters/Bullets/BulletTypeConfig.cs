using System;
using Cysharp.Threading.Tasks;
using Game.Scripts.AddressablesContent;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game.Scripts.Characters.Bullets
{
    [Serializable]
    public class BulletTypeConfig
    {
        [SerializeField] private BulletType _type;
        [SerializeField] private AssetReferenceGameObject _prefabReference;
        [SerializeField] private int _poolSize;

        private Bullet _resolvedPrefab;

        public BulletType Type => _type;
        public Bullet Prefab => _resolvedPrefab;
        public int PoolSize => _poolSize;
        public AssetReferenceGameObject PrefabReference => _prefabReference;

        public async UniTask ResolveAsync(IAddressablesContentService content)
        {
            if (_resolvedPrefab != null)
                return;

            if (_prefabReference == null || _prefabReference.RuntimeKeyIsValid() == false)
                return;

            GameObject go = await content.LoadGameObjectAsync(_prefabReference);
            if (go == null)
            {
                Debug.LogError($"[BulletTypeConfig] Failed to load bullet '{_type}' ({_prefabReference.AssetGUID}).");
                return;
            }

            _resolvedPrefab = go.GetComponent<Bullet>() ?? go.GetComponentInChildren<Bullet>(true);

            if (_resolvedPrefab == null)
                Debug.LogError($"[BulletTypeConfig] No Bullet on '{_prefabReference.AssetGUID}' ({go.name}).");
        }
    }
}
