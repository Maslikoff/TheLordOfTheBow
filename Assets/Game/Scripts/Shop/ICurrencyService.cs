using System;

namespace Game.Scripts.Shop
{
    public interface ICurrencyService
    {
        int Coins { get; }
        event Action<int> Changed;

        void Load(int coins);
        void Add(int amount);
        bool TrySpend(int amount);
        void Persist();
    }
}
