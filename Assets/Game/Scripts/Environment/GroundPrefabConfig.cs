using System;
using Cysharp.Threading.Tasks;
using Game.Scripts.AddressablesContent;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game.Scripts.Environment
{
    [Serializable]
    public class GroundPrefabConfig
    {
        [SerializeField] private AssetReferenceGameObject _groundPrefabReference;
        [SerializeField] private int _poolSize;

        private Ground _resolvedPrefab;

        public Ground GroundPrefab => _resolvedPrefab;
        public int PoolSize => _poolSize;
        public AssetReferenceGameObject GroundPrefabReference => _groundPrefabReference;

        public async UniTask ResolveAsync(IAddressablesContentService content)
        {
            if (_resolvedPrefab != null)
                return;

            if (_groundPrefabReference == null || _groundPrefabReference.RuntimeKeyIsValid() == false)
                return;

            GameObject go = await content.LoadGameObjectAsync(_groundPrefabReference);
            _resolvedPrefab = go.GetComponent<Ground>() ?? go.GetComponentInChildren<Ground>(true);

            if (_resolvedPrefab == null)
                Debug.LogError($"[GroundPrefabConfig] No Ground on '{_groundPrefabReference.AssetGUID}' ({go.name}).");
        }
    }
}
