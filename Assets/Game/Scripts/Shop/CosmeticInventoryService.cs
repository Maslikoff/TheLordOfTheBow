using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Scripts.Save;

namespace Game.Scripts.Shop
{
    public sealed class CosmeticInventoryService : ICosmeticInventoryService
    {
        private readonly ICurrencyService _currency;
        private readonly HashSet<string> _ownedSkins = new();
        private readonly HashSet<string> _ownedProjectiles = new();
        private readonly HashSet<string> _ownedHats = new();

        private string _equippedSkin = string.Empty;
        private string _equippedProjectile = string.Empty;
        private string _equippedHat = string.Empty;

        public CosmeticInventoryService(ICurrencyService currency)
        {
            _currency = currency ?? throw new ArgumentNullException(nameof(currency));
        }

        public event Action Changed;

        public bool Owns(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
                return false;

            return _ownedSkins.Contains(itemId)
                   || _ownedProjectiles.Contains(itemId)
                   || _ownedHats.Contains(itemId);
        }

        public string GetEquippedId(CosmeticCategory category)
        {
            return category switch
            {
                CosmeticCategory.Skin => _equippedSkin,
                CosmeticCategory.Projectile => _equippedProjectile,
                CosmeticCategory.Hat => _equippedHat,
                _ => string.Empty
            };
        }

        public IReadOnlyCollection<string> GetOwnedIds(CosmeticCategory category)
        {
            return GetOwnedSet(category);
        }

        public void Load(
            IReadOnlyList<string> ownedSkins,
            IReadOnlyList<string> ownedProjectiles,
            IReadOnlyList<string> ownedHats,
            string equippedSkin,
            string equippedProjectile,
            string equippedHat)
        {
            _ownedSkins.Clear();
            _ownedProjectiles.Clear();
            _ownedHats.Clear();

            AddRange(_ownedSkins, ownedSkins);
            AddRange(_ownedProjectiles, ownedProjectiles);
            AddRange(_ownedHats, ownedHats);

            _equippedSkin = equippedSkin ?? string.Empty;
            _equippedProjectile = equippedProjectile ?? string.Empty;
            _equippedHat = equippedHat ?? string.Empty;

            Changed?.Invoke();
            LogCurrentEquipment();
        }

        public bool TryBuyWithCoins(ShopItemDefinition item)
        {
            if (item == null || item.IsIap || item.IsDefault)
                return false;

            if (Owns(item.Id))
                return false;

            if (_currency.TrySpend(item.CoinPrice) == false)
            {
                Debug.Log($"[Shop] Not enough coins for {item.Id}. Need {item.CoinPrice}, have {_currency.Coins}");
                return false;
            }

            UnlockInternal(item);
            Debug.Log($"[Shop] Bought with coins: {item.Category}/{item.Id} for {item.CoinPrice}");
            Persist();
            Changed?.Invoke();
            return true;
        }

        public void UnlockFromIap(ShopItemDefinition item)
        {
            if (item == null)
                return;

            if (Owns(item.Id))
            {
                Debug.Log($"[Shop] IAP already owned: {item.Id}");
                return;
            }

            UnlockInternal(item);
            Debug.Log($"[Shop] Unlocked from IAP: {item.Category}/{item.Id} (product {item.IapProductId})");
            Persist();
            Changed?.Invoke();
        }

        public bool TryEquip(CosmeticCategory category, string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                SetEquipped(category, string.Empty);
                Persist();
                Changed?.Invoke();
                Debug.Log($"[Cosmetics] Equipped {category}: default");
                return true;
            }

            if (GetOwnedSet(category).Contains(itemId) == false)
            {
                Debug.LogWarning($"[Cosmetics] Cannot equip unowned item: {itemId}");
                return false;
            }

            SetEquipped(category, itemId);
            Persist();
            Changed?.Invoke();
            Debug.Log($"[Cosmetics] Equipped {category}: {itemId}");
            return true;
        }

        public void LogCurrentEquipment()
        {
            Debug.Log(
                $"[Cosmetics] Current equipment — Skin: {OrDefault(_equippedSkin)}, " +
                $"Projectile: {OrDefault(_equippedProjectile)}, Hat: {OrDefault(_equippedHat)}");
        }

        private void UnlockInternal(ShopItemDefinition item)
        {
            GetOwnedSet(item.Category).Add(item.Id);

            if (item.Category == CosmeticCategory.Skin || string.IsNullOrEmpty(GetEquippedId(item.Category)))
                SetEquipped(item.Category, item.Id);
        }

        private void SetEquipped(CosmeticCategory category, string itemId)
        {
            switch (category)
            {
                case CosmeticCategory.Skin:
                    _equippedSkin = itemId ?? string.Empty;
                    break;
                case CosmeticCategory.Projectile:
                    _equippedProjectile = itemId ?? string.Empty;
                    break;
                case CosmeticCategory.Hat:
                    _equippedHat = itemId ?? string.Empty;
                    break;
            }
        }

        private HashSet<string> GetOwnedSet(CosmeticCategory category)
        {
            return category switch
            {
                CosmeticCategory.Skin => _ownedSkins,
                CosmeticCategory.Projectile => _ownedProjectiles,
                CosmeticCategory.Hat => _ownedHats,
                _ => _ownedSkins
            };
        }

        private void Persist()
        {
            Saves.Data.WriteCosmeticInventory(
                ToList(_ownedSkins),
                ToList(_ownedProjectiles),
                ToList(_ownedHats),
                _equippedSkin,
                _equippedProjectile,
                _equippedHat);

            Saves.Save();
        }

        private static void AddRange(HashSet<string> target, IReadOnlyList<string> source)
        {
            if (source == null)
                return;

            for (int i = 0; i < source.Count; i++)
            {
                if (string.IsNullOrEmpty(source[i]) == false)
                    target.Add(source[i]);
            }
        }

        private static List<string> ToList(HashSet<string> set)
        {
            return new List<string>(set);
        }

        private static string OrDefault(string id) => string.IsNullOrEmpty(id) ? "default" : id;
    }
}
