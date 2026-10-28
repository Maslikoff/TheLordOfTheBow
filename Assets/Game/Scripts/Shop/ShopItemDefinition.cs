using System;
using Cysharp.Threading.Tasks;
using Game.Scripts.AddressablesContent;
using UnityEngine;
using UnityEngine.AddressableAssets;
using YG;

namespace Game.Scripts.Shop
{
    [CreateAssetMenu(fileName = "ShopItem", menuName = "Game/Shop/Shop Item")]
    public class ShopItemDefinition : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private CosmeticCategory _category;
        [SerializeField] private bool _isDefault;
        [SerializeField] private string _displayNameRu;
        [SerializeField] private string _displayNameEn;
        [SerializeField] private string _displayNameTr;
        [SerializeField] private string _descriptionRu;
        [SerializeField] private string _descriptionEn;
        [SerializeField] private string _descriptionTr;
        [SerializeField] private int _coinPrice;
        [SerializeField] private bool _isIap;
        [SerializeField] private string _iapProductId;
        [SerializeField] private AssetReferenceSprite _iconReference;
        [SerializeField] private AssetReferenceGameObject _prefabReference;

        private Sprite _resolvedIcon;

        public string Id => _id;
        public CosmeticCategory Category => _category;
        public bool IsDefault => _isDefault;
        public int CoinPrice => Mathf.Max(0, _coinPrice);
        public bool IsIap => _isIap;
        public string IapProductId => _iapProductId;
        public AssetReferenceSprite IconReference => _iconReference;
        public AssetReferenceGameObject PrefabReference => _prefabReference;
        public Sprite Icon => _resolvedIcon;

        public string DisplayName => GetLocalized(_displayNameRu, _displayNameEn, _displayNameTr);
        public string Description => GetLocalized(_descriptionRu, _descriptionEn, _descriptionTr);

        public void ApplyStubData(
            string id,
            CosmeticCategory category,
            string nameRu,
            string nameEn,
            string nameTr,
            string descRu,
            string descEn,
            string descTr,
            int coinPrice,
            bool isIap,
            string iapProductId)
        {
            _id = id;
            _category = category;
            _displayNameRu = nameRu;
            _displayNameEn = nameEn;
            _displayNameTr = nameTr;
            _descriptionRu = descRu;
            _descriptionEn = descEn;
            _descriptionTr = descTr;
            _coinPrice = coinPrice;
            _isIap = isIap;
            _iapProductId = iapProductId;
        }

        public async UniTask EnsureIconLoadedAsync(IAddressablesContentService content)
        {
            if (_resolvedIcon != null)
                return;

            if (content == null)
                return;

            if (_iconReference == null || _iconReference.RuntimeKeyIsValid() == false)
                return;

            try
            {
                _resolvedIcon = await content.LoadAssetAsync<Sprite>(_iconReference);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Shop] Icon load failed for '{_id}': {e.Message}");
            }
        }

        private static string GetLocalized(string ru, string en, string tr)
        {
            return YG2.lang switch
            {
                "en" => string.IsNullOrEmpty(en) ? ru : en,
                "tr" => string.IsNullOrEmpty(tr) ? ru : tr,
                _ => ru
            };
        }
    }
}
