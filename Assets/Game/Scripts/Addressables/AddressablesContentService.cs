using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Game.Scripts.AddressablesContent
{
    public sealed class AddressablesContentService : IAddressablesContentService
    {
        private const string InitHelp =
            "Addressables init failed. In Editor run: Game → Addressables → Setup Groups And Mark Assets, " +
            "then Game → Addressables → Use Asset Database Play Mode " +
            "(or Addressables Groups → Play Mode Script → Use Asset Database). " +
            "For WebGL player builds, first Build → New Build → Default Build Script.";

        private readonly HashSet<AsyncOperationHandle> _handles = new();
        private bool _initialized;

        public bool IsInitialized => _initialized;

        public async UniTask InitializeAsync(CancellationToken cancellationToken = default)
        {
            if (_initialized)
                return;

            Debug.Log("[Addressables] InitializeAsync...");

            try
            {
                var initHandle = Addressables.InitializeAsync();
                Track(initHandle);
                await AwaitHandle(initHandle, cancellationToken);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(InitHelp, e);
            }

#if !UNITY_EDITOR
            await TryUpdateCatalogsAsync(cancellationToken);
#else
            // Asset Database play mode: skip remote catalog checks.
            Debug.Log("[Addressables] Editor: skipping CheckForCatalogUpdates.");
#endif

            _initialized = true;
            Debug.Log("[Addressables] Ready.");
        }

        public async UniTask<T> LoadAssetAsync<T>(string address, CancellationToken cancellationToken = default)
            where T : Object
        {
            EnsureInitialized();
            var handle = Addressables.LoadAssetAsync<T>(address);
            Track(handle);
            await AwaitHandle(handle, cancellationToken);
            return handle.Result;
        }

        public async UniTask<T> LoadAssetAsync<T>(AssetReference reference,
            CancellationToken cancellationToken = default)
            where T : Object
        {
            if (reference == null || reference.RuntimeKeyIsValid() == false)
                throw new ArgumentException("AssetReference is null or invalid.", nameof(reference));

            EnsureInitialized();

            // Reuse an in-flight / completed AssetReference handle to avoid
            // "Attempting to load AssetReference that has already been loaded".
            if (reference.OperationHandle.IsValid())
            {
                var existing = reference.OperationHandle.Convert<T>();
                await AwaitHandle(existing, cancellationToken);
                return existing.Result;
            }

            if (reference.Asset is T cached)
                return cached;

            // Load by RuntimeKey so concurrent callers sharing the same
            // AssetReference instance do not trip AssetReference's single-handle rule.
            var handle = Addressables.LoadAssetAsync<T>(reference.RuntimeKey);
            Track(handle);
            try
            {
                await AwaitHandle(handle, cancellationToken);
            }
            catch
            {
                _handles.Remove(handle);
                if (handle.IsValid()) Addressables.Release(handle);
                throw;
            }
            return handle.Result;
        }

        public async UniTask<GameObject> LoadGameObjectAsync(AssetReferenceGameObject reference,
            CancellationToken cancellationToken = default)
        {
            return await LoadAssetAsync<GameObject>(reference, cancellationToken);
        }

        public async UniTask<IList<T>> LoadByLabelAsync<T>(string label,
            CancellationToken cancellationToken = default)
            where T : Object
        {
            EnsureInitialized();
            var handle = Addressables.LoadAssetsAsync<T>(label, null);
            Track(handle);
            await AwaitHandle(handle, cancellationToken);
            return handle.Result;
        }

        public async UniTask DownloadDependenciesAsync(string key,
            CancellationToken cancellationToken = default)
        {
            EnsureInitialized();

#if UNITY_EDITOR
            // No remote download needed when Play Mode uses Asset Database.
            return;
#else
            var sizeHandle = Addressables.GetDownloadSizeAsync(key);
            Track(sizeHandle);
            await AwaitHandle(sizeHandle, cancellationToken);

            if (sizeHandle.Result <= 0)
                return;

            Debug.Log($"[Addressables] Downloading dependencies for '{key}' ({sizeHandle.Result} bytes)...");
            var downloadHandle = Addressables.DownloadDependenciesAsync(key);
            Track(downloadHandle);
            await AwaitHandle(downloadHandle, cancellationToken);
#endif
        }

        public void Release(AssetReference reference)
        {
            if (reference == null || reference.Asset == null)
                return;

            reference.ReleaseAsset();
        }

        public void Release(Object asset)
        {
            if (asset == null)
                return;

            AsyncOperationHandle? owned = null;
            foreach (var handle in _handles)
            {
                if (handle.IsValid() && ReferenceEquals(handle.Result, asset))
                {
                    owned = handle;
                    break;
                }
            }
            if (owned.HasValue)
            {
                _handles.Remove(owned.Value);
                Addressables.Release(owned.Value);
                return;
            }
            Addressables.Release(asset);
        }

        private async UniTask TryUpdateCatalogsAsync(CancellationToken cancellationToken)
        {
            var checkHandle = Addressables.CheckForCatalogUpdates(false);
            Track(checkHandle);
            await AwaitHandle(checkHandle, cancellationToken);

            IList<string> catalogs = checkHandle.Result;
            if (catalogs == null || catalogs.Count == 0)
                return;

            Debug.Log($"[Addressables] Updating {catalogs.Count} catalog(s)...");
            var updateHandle = Addressables.UpdateCatalogs(catalogs, false);
            Track(updateHandle);
            await AwaitHandle(updateHandle, cancellationToken);
        }

        private void EnsureInitialized()
        {
            if (_initialized == false)
                throw new InvalidOperationException("AddressablesContentService is not initialized.");
        }

        private void Track(AsyncOperationHandle handle)
        {
            _handles.Add(handle);
        }

        private void Track<T>(AsyncOperationHandle<T> handle)
        {
            _handles.Add(handle);
        }

        private static async UniTask AwaitHandle(AsyncOperationHandle handle,
            CancellationToken cancellationToken)
        {
            await handle.ToUniTask(cancellationToken: cancellationToken);

            if (handle.Status == AsyncOperationStatus.Failed)
                throw handle.OperationException ?? new Exception("Addressables operation failed.");
        }

        private static async UniTask AwaitHandle<T>(AsyncOperationHandle<T> handle,
            CancellationToken cancellationToken)
        {
            await handle.ToUniTask(cancellationToken: cancellationToken);

            if (handle.Status == AsyncOperationStatus.Failed)
                throw handle.OperationException ?? new Exception("Addressables operation failed.");
        }
    }
}
