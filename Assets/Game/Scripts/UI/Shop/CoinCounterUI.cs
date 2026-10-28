using Game.Scripts.Shop;
using TMPro;
using UnityEngine;
using VContainer;

namespace Game.Scripts.UI.Shop
{
    public class CoinCounterUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _label;

        private ICurrencyService _currency;

        [Inject]
        private void Construct(ICurrencyService currency)
        {
            Bind(currency);
        }

        public void Bind(ICurrencyService currency)
        {
            if (_currency != null)
                _currency.Changed -= OnCoinsChanged;

            _currency = currency;

            if (isActiveAndEnabled && _currency != null)
            {
                _currency.Changed += OnCoinsChanged;
                OnCoinsChanged(_currency.Coins);
            }
        }

        private void OnEnable()
        {
            if (_currency == null)
                return;

            _currency.Changed -= OnCoinsChanged;
            _currency.Changed += OnCoinsChanged;
            OnCoinsChanged(_currency.Coins);
        }

        private void OnDisable()
        {
            if (_currency != null)
                _currency.Changed -= OnCoinsChanged;
        }

        private void OnCoinsChanged(int coins)
        {
            if (_label != null)
                _label.text = coins.ToString();
        }
    }
}
