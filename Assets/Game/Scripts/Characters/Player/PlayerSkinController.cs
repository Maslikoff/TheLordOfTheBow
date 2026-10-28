using System;
using Cysharp.Threading.Tasks;
using Game.Scripts.AddressablesContent;
using Game.Scripts.Shop;
using UnityEngine;

namespace Game.Scripts.Characters.Player
{
    public sealed class PlayerSkinController : MonoBehaviour
    {
        [SerializeField] private Transform _visualRoot;
        [SerializeField] private GameObject _defaultModel;

        private ICosmeticInventoryService _inventory;
        private ShopCatalogProvider _catalog;
        private IAddressablesContentService _content;
        private HitFeedback _hitFeedback;
        private GameObject _instance;
        private GameObject _loadedPrefab;
        private string _appliedId = string.Empty;
        private int _requestVersion;
        private bool _destroyed;

        public async UniTask InitializeAsync(ICosmeticInventoryService inventory,
            ShopCatalogProvider catalog, IAddressablesContentService content)
        {
            _inventory = inventory;
            _catalog = catalog;
            _content = content;
            _hitFeedback = GetComponent<HitFeedback>();
            _inventory.Changed -= OnEquipmentChanged;
            _inventory.Changed += OnEquipmentChanged;
            await ApplyEquipmentAsync();
        }

        private void OnEnable()
        {
            if (_inventory == null) return;
            _inventory.Changed -= OnEquipmentChanged;
            _inventory.Changed += OnEquipmentChanged;
            OnEquipmentChanged();
        }

        private void OnDisable()
        {
            ++_requestVersion;
            if (_inventory != null) _inventory.Changed -= OnEquipmentChanged;
        }

        private void OnEquipmentChanged() => ApplyEquipmentAsync().Forget();

        private async UniTask ApplyEquipmentAsync()
        {
            int version = ++_requestVersion;
            if (_destroyed || _visualRoot == null || _defaultModel == null) return;

            string id = _inventory.GetEquippedId(CosmeticCategory.Skin);
            ShopItemDefinition item = _catalog.Catalog != null ? _catalog.Catalog.FindById(id) : null;
            if (string.IsNullOrEmpty(id) || !_inventory.Owns(id) || item == null || item.IsDefault)
            {
                ShowDefault();
                return;
            }
            if (_appliedId == id && _instance != null) return;
            if (item.PrefabReference == null || !item.PrefabReference.RuntimeKeyIsValid())
            {
                ShowDefault();
                return;
            }

            GameObject loaded = null;
            GameObject next = null;
            try
            {
                // Finish even after destruction, then release: canceling the await alone
                // would abandon an Addressables operation that is still loading.
                loaded = await _content.LoadGameObjectAsync(item.PrefabReference);
                if (_destroyed || version != _requestVersion || !isActiveAndEnabled)
                    return;
                if (loaded == null) throw new InvalidOperationException("Empty skin prefab");

                next = Instantiate(loaded, _visualRoot, false);
                next.name = "EquippedSkin";
                foreach (Transform child in next.GetComponentsInChildren<Transform>(true))
                    child.gameObject.layer = _visualRoot.gameObject.layer;

                ClearCustomModel();
                _instance = next;
                _loadedPrefab = loaded;
                next = null;
                loaded = null;
                _defaultModel.SetActive(false);
                _appliedId = id;
                _hitFeedback?.RefreshRenderers();
            }
            catch (Exception exception)
            {
                if (!_destroyed && version == _requestVersion)
                    Debug.LogWarning($"[Skins] Could not load '{id}'. Select it again to retry: {exception.Message}");
            }
            finally
            {
                if (next != null) Destroy(next);
                if (loaded != null) _content.Release(loaded);
            }
        }

        private void ShowDefault()
        {
            ClearCustomModel();
            _defaultModel.SetActive(true);
            _appliedId = string.Empty;
            _hitFeedback?.RefreshRenderers();
        }

        private void ClearCustomModel()
        {
            if (_instance != null)
            {
                _instance.SetActive(false);
                _instance.transform.SetParent(null);
                Destroy(_instance);
                _instance = null;
            }
            if (_loadedPrefab != null)
            {
                ReleaseAfterDestroyAsync(_content, _loadedPrefab).Forget();
                _loadedPrefab = null;
            }
        }

        private static async UniTaskVoid ReleaseAfterDestroyAsync(IAddressablesContentService content, GameObject prefab)
        {
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            content.Release(prefab);
        }

        private void OnDestroy()
        {
            _destroyed = true;
            ++_requestVersion;
            if (_inventory != null) _inventory.Changed -= OnEquipmentChanged;
            ClearCustomModel();
        }
    }
}
