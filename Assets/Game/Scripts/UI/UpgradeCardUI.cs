using Cysharp.Threading.Tasks;
using Game.Scripts.AddressablesContent;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VContainer;
using Game.Localization;
using UpgradeCardData = Game.Scripts.Upgrades.UpgradeCard;

namespace Game.Scripts.UI
{
    public class UpgradeCardUI : MonoBehaviour
    {
        [SerializeField] private Image _cardBackground;
        [SerializeField] private Image _cardIcon;
        [SerializeField] private TextMeshProUGUI _cardNameText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private Button _selectButton;

        private UpgradeCardData _upgradeData;
        private System.Action<UpgradeCardData> _selectedCallback;
        private IAddressablesContentService _content;
        private Game.Scripts.Upgrades.PlayerUpgradeHolder _holder;

        [Inject]
        private void Construct(IAddressablesContentService content)
        {
            _content = content;
        }

        private void Awake()
        {
            if (_selectButton != null)
                _selectButton.onClick.AddListener(OnCardClicked);
        }

        private void OnEnable()
        {
            Loc.Changed += OnLanguageSwitched;
            RefreshTexts();
        }

        private void OnDisable()
        {
            Loc.Changed -= OnLanguageSwitched;
        }

        private void OnDestroy()
        {
            if (_selectButton != null)
                _selectButton.onClick.RemoveListener(OnCardClicked);
        }

        public void Initialize(UpgradeCardData upgradeData, System.Action<UpgradeCardData> selectedCallback, Game.Scripts.Upgrades.PlayerUpgradeHolder holder = null)
        {
            _holder = holder;
            _upgradeData = upgradeData;
            _selectedCallback = selectedCallback;

            _cardBackground.color = upgradeData.CardColor;
            RefreshTexts();
            BindIconAsync(upgradeData).Forget();
        }

        private async UniTaskVoid BindIconAsync(UpgradeCardData upgradeData)
        {
            if (_content != null)
                await upgradeData.EnsureIconLoadedAsync(_content);

            if (_cardIcon != null)
                _cardIcon.sprite = upgradeData.CardIcon;
        }

        private void OnLanguageSwitched(string _)
        {
            RefreshTexts();
        }

        private void RefreshTexts()
        {
            if (_upgradeData == null)
                return;

            if (_descriptionText != null)
                _descriptionText.text = _upgradeData is Game.Scripts.UpgradeCard.CampaignUpgradeCard card ? card.Describe(_holder) : _upgradeData.Description;
            if (_cardNameText != null)
                _cardNameText.text = _upgradeData.CardName;
        }

        private void OnCardClicked()
        {
            _selectedCallback?.Invoke(_upgradeData);
        }
    }
}
