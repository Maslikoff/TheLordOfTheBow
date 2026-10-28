using System;
using UnityEngine;

namespace Game.Scripts.Shop
{
    public sealed class ShopService
    {
        private readonly ShopCatalogProvider _catalogProvider;
        private readonly ICosmeticInventoryService _inventory;
        private readonly IPaymentsService _payments;

        public ShopService(
            ShopCatalogProvider catalogProvider,
            ICosmeticInventoryService inventory,
            IPaymentsService payments)
        {
            _catalogProvider = catalogProvider ?? throw new ArgumentNullException(nameof(catalogProvider));
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            _payments = payments ?? throw new ArgumentNullException(nameof(payments));
        }

        public ShopCatalog Catalog => _catalogProvider.Catalog;
        public ICosmeticInventoryService Inventory => _inventory;

        public void Initialize()
        {
            _payments.PurchaseSucceeded -= OnPurchaseSucceeded;
            _payments.PurchaseSucceeded += OnPurchaseSucceeded;
            _payments.Initialize();
        }

        public void Purchase(ShopItemDefinition item)
        {
            if (item == null)
                return;

            if (item.IsDefault)
            {
                Equip(item);
                return;
            }

            if (_inventory.Owns(item.Id))
            {
                Debug.Log($"[Shop] Already owned: {item.Id}");
                return;
            }

            if (item.IsIap)
            {
                _payments.Buy(item.IapProductId);
                return;
            }

            _inventory.TryBuyWithCoins(item);
        }

        public bool Equip(ShopItemDefinition item)
        {
            if (item == null)
                return false;

            return _inventory.TryEquip(item.Category, item.IsDefault ? string.Empty : item.Id);
        }

        private void OnPurchaseSucceeded(string productId)
        {
            ShopCatalog catalog = Catalog;
            if (catalog == null)
            {
                Debug.LogWarning("[Shop] Purchase succeeded but catalog is not loaded");
                return;
            }

            ShopItemDefinition item = catalog.FindByIapProductId(productId);
            if (item == null)
            {
                Debug.LogWarning($"[Shop] No catalog item for IAP product {productId}");
                return;
            }

            _inventory.UnlockFromIap(item);
        }
    }
}
