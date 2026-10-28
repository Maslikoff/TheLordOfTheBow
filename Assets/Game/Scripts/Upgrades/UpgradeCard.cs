using Cysharp.Threading.Tasks;
using Game.Scripts.AddressablesContent;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Game.Localization;

namespace Game.Scripts.Upgrades
{
    public abstract class UpgradeCard : ScriptableObject
    {
        [Header("Basic Info")]
        [SerializeField] private string _cardName;
        [SerializeField] private string _cardNameEn;
        [SerializeField] private string _cardNameTr;
        [SerializeField] private string _description;
        [SerializeField] private string _descriptionEn;
        [SerializeField] private string _descriptionTr;
        [SerializeField] private AssetReferenceSprite _cardIconReference;

        [Header("Visual")]
        [SerializeField] private Color _cardColor = Color.white;

        private Sprite _resolvedIcon;

        public string CardName => GetLocalized(_cardName, _cardNameEn, _cardNameTr);
        public string Description => GetLocalized(_description, _descriptionEn, _descriptionTr);
        public Sprite CardIcon => _resolvedIcon;
        public Color CardColor => _cardColor;
        public AssetReferenceSprite CardIconReference => _cardIconReference;

        public abstract void Apply(PlayerUpgradeHolder playerUpgradeHolder);

        public async UniTask EnsureIconLoadedAsync(IAddressablesContentService content)
        {
            if (_resolvedIcon != null)
                return;

            if (_cardIconReference == null || _cardIconReference.RuntimeKeyIsValid() == false)
                return;

            _resolvedIcon = await content.LoadAssetAsync<Sprite>(_cardIconReference);
        }

        private static string GetLocalized(string ru, string en, string tr)
        {
            return Loc.Lang switch
            {
                "en" => string.IsNullOrEmpty(en) ? ru : en,
                "tr" => string.IsNullOrEmpty(tr) ? ru : tr,
                _ => ru
            };
        }
    }
}
