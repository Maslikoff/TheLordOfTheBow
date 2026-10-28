using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts.Shop
{
    [CreateAssetMenu(fileName = "ShopCatalog", menuName = "Game/Shop/Shop Catalog")]
    public class ShopCatalog : ScriptableObject
    {
        [SerializeField] private List<ShopItemDefinition> _items = new();

        public IReadOnlyList<ShopItemDefinition> Items => _items;

        public ShopItemDefinition FindById(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            for (int i = 0; i < _items.Count; i++)
            {
                ShopItemDefinition item = _items[i];
                if (item != null && item.Id == id)
                    return item;
            }

            return null;
        }

        public ShopItemDefinition FindByIapProductId(string productId)
        {
            if (string.IsNullOrEmpty(productId))
                return null;

            for (int i = 0; i < _items.Count; i++)
            {
                ShopItemDefinition item = _items[i];
                if (item != null && item.IsIap && item.IapProductId == productId)
                    return item;
            }

            return null;
        }

        public void SetItems(List<ShopItemDefinition> items)
        {
            _items = items ?? new List<ShopItemDefinition>();
        }
    }
}
