using System;

namespace Game.Scripts.Shop
{
    public interface IPaymentsService
    {
        event Action<string> PurchaseSucceeded;
        event Action<string> PurchaseFailed;

        void Initialize();
        void Buy(string productId);
        void ConsumePendingPurchases();
    }
}
