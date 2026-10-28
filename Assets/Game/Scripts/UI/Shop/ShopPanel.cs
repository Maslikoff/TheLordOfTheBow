using System.Collections.Generic;
using Game.Scripts.AddressablesContent;
using Game.Scripts.Shop;
using Game.Scripts.StateServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using YG;

namespace Game.Scripts.UI.Shop
{
    public class ShopPanel : MonoBehaviour
    {
        private const int Columns = 2;

        [SerializeField] private GameObject _root;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _tabSkins;
        [SerializeField] private Button _tabProjectiles;
        [SerializeField] private Button _tabHats;
        [SerializeField] private Image _tabSkinsImage;
        [SerializeField] private Image _tabProjectilesImage;
        [SerializeField] private Image _tabHatsImage;
        [SerializeField] private TextMeshProUGUI _tabSkinsLabel;
        [SerializeField] private TextMeshProUGUI _tabProjectilesLabel;
        [SerializeField] private TextMeshProUGUI _tabHatsLabel;
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private RectTransform _itemsParent;
        [SerializeField] private ShopItemView _itemPrefab;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private RectTransform _viewport;
        [SerializeField] private GridLayoutGroup _grid;
        [SerializeField] private Sprite _tabSelectedSprite;
        [SerializeField] private Sprite _tabIdleSprite;
        [SerializeField] private GameObject[] _hideWhileOpen;
        [SerializeField] private float _spacing = 16f;
        [SerializeField] private float _padding = 20f;
        [SerializeField] private float _cellHeightRatio = 1.42f;

        private ShopService _shop;
        private TapToStartZone _tapToStart;
        private IAddressablesContentService _content;
        private ICurrencyService _currency;
        private IGameStateService _gameState;
        private readonly List<ShopItemView> _pool = new();
        private CosmeticCategory _currentTab = CosmeticCategory.Skin;
        private Vector2 _lastViewportSize;
        private bool _isOpen;

        public bool IsOpen => _isOpen;

        [Inject]
        private void Construct(
            ShopService shop,
            TapToStartZone tapToStart,
            IAddressablesContentService content,
            ICurrencyService currency,
            IGameStateService gameState)
        {
            _shop = shop;
            _tapToStart = tapToStart;
            _content = content;
            _currency = currency;
            _gameState = gameState;

            BindCoinCounters();
        }

        private void Awake()
        {
            if (_root == null)
                _root = gameObject;

            if (_tabProjectiles != null)
                _tabProjectiles.gameObject.SetActive(false);
            if (_tabHats != null)
                _tabHats.gameObject.SetActive(false);

            if (_viewport == null && _scrollRect != null)
                _viewport = _scrollRect.viewport;

            if (_grid == null && _itemsParent != null)
                _grid = _itemsParent.GetComponent<GridLayoutGroup>();
        }

        private void OnEnable()
        {
            if (_closeButton != null)
                _closeButton.onClick.AddListener(Close);
            if (_tabSkins != null)
                _tabSkins.onClick.AddListener(() => ShowTab(CosmeticCategory.Skin));
            if (_tabProjectiles != null)
                _tabProjectiles.onClick.AddListener(() => ShowTab(CosmeticCategory.Projectile));
            if (_tabHats != null)
                _tabHats.onClick.AddListener(() => ShowTab(CosmeticCategory.Hat));

            if (_shop != null)
                _shop.Inventory.Changed += RefreshItems;
            if (_currency != null)
                _currency.Changed += OnCoinsChanged;
        }

        private void OnDisable()
        {
            if (_closeButton != null)
                _closeButton.onClick.RemoveListener(Close);
            if (_tabSkins != null)
                _tabSkins.onClick.RemoveAllListeners();
            if (_tabProjectiles != null)
                _tabProjectiles.onClick.RemoveAllListeners();
            if (_tabHats != null)
                _tabHats.onClick.RemoveAllListeners();

            if (_shop != null)
                _shop.Inventory.Changed -= RefreshItems;
            if (_currency != null)
                _currency.Changed -= OnCoinsChanged;
        }

        private void LateUpdate()
        {
            if (_isOpen == false || _viewport == null)
                return;

            Vector2 size = _viewport.rect.size;
            if ((size - _lastViewportSize).sqrMagnitude < 1f)
                return;

            _lastViewportSize = size;
            ApplyGridMetrics();
        }

        public void Open()
        {
            _isOpen = true;
            if (_root != null)
                _root.SetActive(true);

            _tapToStart?.SetShopBlocking(true);
            RefreshHud();
            LocalizeChrome();
            Canvas.ForceUpdateCanvases();
            ShowTab(_currentTab);
        }

        public void Close()
        {
            _isOpen = false;
            if (_root != null)
                _root.SetActive(false);

            _tapToStart?.SetShopBlocking(false);
            RefreshHud();
        }

        public void OnBuyClicked(ShopItemDefinition item)
        {
            _shop?.Purchase(item);
            RefreshItems();
        }

        public void OnEquipClicked(ShopItemDefinition item)
        {
            _shop?.Equip(item);
            RefreshItems();
        }

        private void ShowTab(CosmeticCategory category)
        {
            _currentTab = category;
            ApplyTabVisuals();
            RefreshItems();
        }

        private void OnCoinsChanged(int _)
        {
            if (_isOpen)
                RefreshItems();
        }

        private void RefreshItems()
        {
            RecycleAll();

            if (_shop?.Catalog == null || _itemsParent == null || _itemPrefab == null)
                return;

            ApplyGridMetrics();
            if (_viewport != null)
                _lastViewportSize = _viewport.rect.size;

            ICosmeticInventoryService inventory = _shop.Inventory;
            foreach (ShopItemDefinition item in _shop.Catalog.Items)
            {
                if (item == null || item.Category != _currentTab)
                    continue;

                ShopItemView view = Rent();
                view.Bind(item, this, inventory, _content, _currency);
            }

            if (_scrollRect != null)
            {
                _scrollRect.velocity = Vector2.zero;
                _scrollRect.normalizedPosition = new Vector2(0f, 1f);
            }

            Canvas.ForceUpdateCanvases();
            if (_itemsParent != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(_itemsParent);
        }

        private void ApplyGridMetrics()
        {
            if (_grid == null || _viewport == null)
                return;

            float viewportWidth = _viewport.rect.width;
            if (viewportWidth < 1f)
                viewportWidth = 980f;

            float inner = viewportWidth - _padding * 2f - _spacing * (Columns - 1);
            float cellWidth = Mathf.Max(140f, inner / Columns);
            float cellHeight = Mathf.Max(220f, cellWidth * _cellHeightRatio);
            // Keep the action button visible on short, landscape viewports.
            // Very small viewports retain scrolling instead of crushing the controls.
            float availableHeight = _viewport.rect.height - _padding * 2f;
            if (availableHeight >= 280f)
                cellHeight = Mathf.Min(cellHeight, availableHeight);

            _grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            _grid.constraintCount = Columns;
            _grid.cellSize = new Vector2(cellWidth, cellHeight);
            _grid.spacing = new Vector2(_spacing, _spacing);
            _grid.padding = new RectOffset(
                Mathf.RoundToInt(_padding),
                Mathf.RoundToInt(_padding),
                Mathf.RoundToInt(_padding),
                Mathf.RoundToInt(_padding));
            _grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            _grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            _grid.childAlignment = TextAnchor.UpperCenter;
        }

        private ShopItemView Rent()
        {
            for (int i = 0; i < _pool.Count; i++)
            {
                ShopItemView existing = _pool[i];
                if (existing == null || existing.gameObject.activeSelf)
                    continue;

                existing.gameObject.SetActive(true);
                return existing;
            }

            ShopItemView created = Instantiate(_itemPrefab, _itemsParent);
            created.gameObject.SetActive(true);
            _pool.Add(created);
            return created;
        }

        private void RecycleAll()
        {
            for (int i = 0; i < _pool.Count; i++)
            {
                if (_pool[i] != null)
                    _pool[i].gameObject.SetActive(false);
            }
        }

        private void ApplyTabVisuals()
        {
            ApplyTab(_tabSkinsImage, CosmeticCategory.Skin);
            ApplyTab(_tabProjectilesImage, CosmeticCategory.Projectile);
            ApplyTab(_tabHatsImage, CosmeticCategory.Hat);
        }

        private void ApplyTab(Image image, CosmeticCategory category)
        {
            if (image == null)
                return;

            bool selected = _currentTab == category;
            Sprite sprite = selected ? _tabSelectedSprite : _tabIdleSprite;
            if (sprite != null)
                image.sprite = sprite;
        }

        private void LocalizeChrome()
        {
            if (_title != null)
                _title.text = L("Магазин", "Shop", "Mağaza");
            if (_tabSkinsLabel != null)
                _tabSkinsLabel.text = L("Скины", "Skins", "Skinler");
            if (_tabProjectilesLabel != null)
                _tabProjectilesLabel.text = L("Снаряды", "Projectiles", "Mermiler");
            if (_tabHatsLabel != null)
                _tabHatsLabel.text = L("Шапки", "Hats", "Şapkalar");
        }

        private void RefreshHud()
        {
            bool show = _isOpen == false
                        && (_gameState == null || _gameState.IsGameStarted == false);

            if (_hideWhileOpen == null)
                return;

            for (int i = 0; i < _hideWhileOpen.Length; i++)
            {
                if (_hideWhileOpen[i] != null)
                    _hideWhileOpen[i].SetActive(show);
            }
        }

        private void BindCoinCounters()
        {
            if (_currency == null)
                return;

            Transform root = transform.root;
            CoinCounterUI[] counters = root.GetComponentsInChildren<CoinCounterUI>(true);
            for (int i = 0; i < counters.Length; i++)
                counters[i].Bind(_currency);
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
