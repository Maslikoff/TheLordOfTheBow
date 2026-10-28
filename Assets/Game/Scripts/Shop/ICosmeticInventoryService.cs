using System;
using System.Collections.Generic;

namespace Game.Scripts.Shop
{
    public interface ICosmeticInventoryService
    {
        event Action Changed;

        bool Owns(string itemId);
        string GetEquippedId(CosmeticCategory category);
        IReadOnlyCollection<string> GetOwnedIds(CosmeticCategory category);

        void Load(
            IReadOnlyList<string> ownedSkins,
            IReadOnlyList<string> ownedProjectiles,
            IReadOnlyList<string> ownedHats,
            string equippedSkin,
            string equippedProjectile,
            string equippedHat);

        bool TryBuyWithCoins(ShopItemDefinition item);
        void UnlockFromIap(ShopItemDefinition item);
        bool TryEquip(CosmeticCategory category, string itemId);
        void LogCurrentEquipment();
    }
}
