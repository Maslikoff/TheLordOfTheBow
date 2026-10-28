using System;
using Cysharp.Threading.Tasks;
using Game.Scripts.AddressablesContent;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game.Scripts.Environment.Effect
{
    [Serializable]
    public class EffectConfig
    {
        [SerializeField] private EffectType _type;
        [SerializeField] private AssetReferenceGameObject _prefabReference;
        [SerializeField] private int _poolSize = 5;

        [NonSerialized] private GameEffect _resolvedPrefab;

        public EffectType Type => _type;
        public GameEffect Prefab => _resolvedPrefab;
        public int PoolSize => _poolSize;
        public AssetReferenceGameObject PrefabReference => _prefabReference;

        public async UniTask ResolveAsync(IAddressablesContentService content)
        {
            if (_resolvedPrefab != null)
                return;

            if (_prefabReference == null || _prefabReference.RuntimeKeyIsValid() == false)
                return;

            GameObject go = await content.LoadGameObjectAsync(_prefabReference);
            _resolvedPrefab = go.GetComponent<GameEffect>();
        }
    }
}
