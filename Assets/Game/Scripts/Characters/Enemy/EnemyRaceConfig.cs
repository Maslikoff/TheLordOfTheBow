using System;
using Cysharp.Threading.Tasks;
using Game.Scripts.AddressablesContent;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game.Scripts.Characters.Enemy
{
    [Serializable]
    public class EnemyRaceConfig
    {
        [SerializeField] private Race _race;
        [SerializeField] private AssetReferenceGameObject _prefabReference;
        [SerializeField] private int _poolSize;
        [SerializeField] [Range(0, 100)] private float _spawnWeight;

        private Enemy _resolvedPrefab;

        public Race Race => _race;
        public Enemy Prefab => _resolvedPrefab;
        public int PoolSize => _poolSize;
        public float SpawnWeight => _spawnWeight;
        public AssetReferenceGameObject PrefabReference => _prefabReference;

        public async UniTask ResolveAsync(IAddressablesContentService content)
        {
            if (_resolvedPrefab != null)
                return;

            if (_prefabReference == null || _prefabReference.RuntimeKeyIsValid() == false)
                return;

            GameObject go = await content.LoadGameObjectAsync(_prefabReference);
            _resolvedPrefab = go.GetComponent<Enemy>() ?? go.GetComponentInChildren<Enemy>(true);

            if (_resolvedPrefab == null)
                Debug.LogError($"[EnemyRaceConfig] No Enemy on '{_prefabReference.AssetGUID}' ({go.name}).");
        }
    }
}
