using System;
using UnityEngine;
using YG;

namespace Game.Scripts.Shop
{
    public sealed class CurrencyService : ICurrencyService
    {
        private int _coins;
        [VContainer.Inject] private Game.Scripts.Levels.LevelSessionService _session;

        public int Coins => _coins;

        public event Action<int> Changed;

        public void Load(int coins)
        {
            _coins = Mathf.Max(0, coins);
            Changed?.Invoke(_coins);
        }

        public void Add(int amount)
        {
            if (amount <= 0)
                return;

            // Session currency: persist only on Commit / shop spend / explicit Persist.
            _coins += amount;
            Changed?.Invoke(_coins);
        }

        public bool TrySpend(int amount)
        {
            if (amount <= 0)
                return true;

            if (_coins < amount)
                return false;

            _coins -= amount;
            // Cosmetics survive a retry, so their cost must survive every rollback boundary too.
            var campaign = YG2.saves.Campaign;
            void Debit(Game.Scripts.Save.PlayerProgressSnapshot snapshot)
            {
                if (snapshot != null) snapshot.Coins = Mathf.Max(0, snapshot.Coins - amount);
            }
            if (campaign != null)
            {
                Debit(campaign.Committed);
                Debit(campaign.Entry);
                Debit(campaign.Checkpoint);
            }
            if (_session != null)
            {
                Debit(_session.Entry);
                Debit(_session.Wave);
                Debit(_session.PreDeath);
            }
            Persist();
            Changed?.Invoke(_coins);
            return true;
        }

        public void Persist()
        {
            YG2.saves.WriteCoins(_coins);

            if (YG2.isSDKEnabled)
                YG2.SaveProgress();
        }
    }
}
