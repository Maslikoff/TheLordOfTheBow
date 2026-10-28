using Cysharp.Threading.Tasks;
using Game.Scripts.AddressablesContent;
using Game.Scripts.Shop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YG;
#if Payments_yg
using YG.Utils.Pay;
#endif

namespace Game.Scripts.UI.Shop
{
    public class ShopItemView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Image _iconPlaceholder;
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private TextMeshProUGUI _status;
        [SerializeField] private Button _actionButton;
        [SerializeField] private TextMeshProUGUI _actionLabel;
        [SerializeField] private TextMeshProUGUI _priceLabel;
        [SerializeField] private Image _coinIcon;
        [SerializeField] private Sprite _skinPlaceholder;
        [SerializeField] private Sprite _projectilePlaceholder;
        [SerializeField] private Sprite _hatPlaceholder;

        private ShopItemDefinition _item;
        private ShopPanel _panel;
        private IAddressablesContentService _content;
        private ICurrencyService _currency;
        private int _bindVersion;

        public void Bind(
            ShopItemDefinition item,
            ShopPanel panel,
            ICosmeticInventoryService inventory,
            IAddressablesContentService content,
            ICurrencyService currency)
        {
            _item = item;
            _panel = panel;
            _content = content;
            _currency = currency;

            if (_title != null)
                _title.text = item != null ? item.DisplayName : "—";

            ShowCategoryPlaceholder(item);
            BindIconAsync().Forget();
            Refresh(inventory);
        }

        public void Refresh(ICosmeticInventoryService inventory)
        {
            if (_item == null || inventory == null)
                return;

            bool owned = _item.IsDefault || inventory.Owns(_item.Id);
            bool equipped = inventory.GetEquippedId(_item.Category) == (_item.IsDefault ? string.Empty : _item.Id);
            bool canAfford = _item.IsIap || _currency == null || _currency.Coins >= _item.CoinPrice;

            if (_priceLabel != null)
            {
                _priceLabel.gameObject.SetActive(owned == false);
                if (owned == false)
                    _priceLabel.text = FormatPrice(_item);
            }

            if (_coinIcon != null)
                _coinIcon.enabled = owned == false && _item.IsIap == false;

            if (_status != null)
            {
                if (equipped)
                    _status.text = L("Надето", "Equipped", "Takılı");
                else if (_item.IsDefault)
                    _status.text = L("Бесплатно", "Free", "Ücretsiz");
                else if (owned)
                    _status.text = L("Куплено", "Owned", "Sahip");
                else
                    _status.text = string.Empty;
            }

            if (_actionButton == null)
                return;

            _actionButton.onClick.RemoveAllListeners();

            if (owned)
            {
                if (_actionLabel != null)
                    _actionLabel.text = equipped
                        ? L("Надето", "Equipped", "Takılı")
                        : L("Выбрать", "Select", "Seç");
                // Re-selecting also retries a previously failed remote visual load.
                _actionButton.interactable = true;
                _actionButton.onClick.AddListener(() => _panel.OnEquipClicked(_item));
            }
            else
            {
                if (_actionLabel != null)
                    _actionLabel.text = L("Купить", "Buy", "Satın al");
                _actionButton.interactable = canAfford || _item.IsIap;
                _actionButton.onClick.AddListener(() => _panel.OnBuyClicked(_item));
            }
        }

        private void ShowCategoryPlaceholder(ShopItemDefinition item)
        {
            Sprite placeholder = PlaceholderFor(item != null ? item.Category : CosmeticCategory.Skin);

            if (_iconPlaceholder != null)
            {
                if (placeholder != null)
                    _iconPlaceholder.sprite = placeholder;
                _iconPlaceholder.enabled = true;
                _iconPlaceholder.preserveAspect = true;
            }

            if (_icon != null)
            {
                _icon.enabled = false;
                _icon.sprite = null;
            }
        }

        private Sprite PlaceholderFor(CosmeticCategory category)
        {
            return category switch
            {
                CosmeticCategory.Projectile => _projectilePlaceholder,
                CosmeticCategory.Hat => _hatPlaceholder,
                _ => _skinPlaceholder
            };
        }

        private async UniTaskVoid BindIconAsync()
        {
            int version = ++_bindVersion;
            ShopItemDefinition item = _item;
            if (item == null)
                return;

            await item.EnsureIconLoadedAsync(_content);

            if (this == null || version != _bindVersion || _item != item)
                return;

            if (item.Icon == null)
                return;

            if (_icon != null)
            {
                _icon.sprite = item.Icon;
                _icon.enabled = true;
                _icon.preserveAspect = true;
            }

            if (_iconPlaceholder != null)
                _iconPlaceholder.enabled = false;
        }

        private static string FormatPrice(ShopItemDefinition item)
        {
            if (item.IsIap == false)
                return item.CoinPrice.ToString();

#if Payments_yg
            Purchase purchase = YG2.PurchaseByID(item.IapProductId);
            if (purchase != null && string.IsNullOrEmpty(purchase.price) == false)
                return purchase.price;
#endif
            return string.IsNullOrEmpty(item.IapProductId) ? "IAP" : item.IapProductId;
        }

        private static string L(string ru, string en, string tr)
        {
            return YG2.lang switch
            {
                "en" => en,
                "tr" => tr,
                _ => ru
            };
        }
    }
}
