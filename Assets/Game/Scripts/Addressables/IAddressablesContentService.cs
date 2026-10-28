using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game.Scripts.AddressablesContent
{
    public interface IAddressablesContentService
    {
        bool IsInitialized { get; }

        UniTask InitializeAsync(CancellationToken cancellationToken = default);

        UniTask<T> LoadAssetAsync<T>(string address, CancellationToken cancellationToken = default)
            where T : Object;

        UniTask<T> LoadAssetAsync<T>(AssetReference reference, CancellationToken cancellationToken = default)
            where T : Object;

        UniTask<GameObject> LoadGameObjectAsync(AssetReferenceGameObject reference,
            CancellationToken cancellationToken = default);

        UniTask<IList<T>> LoadByLabelAsync<T>(string label, CancellationToken cancellationToken = default)
            where T : Object;

        UniTask DownloadDependenciesAsync(string key, CancellationToken cancellationToken = default);

        void Release(AssetReference reference);

        void Release(Object asset);
    }
}
