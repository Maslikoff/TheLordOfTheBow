using Game.Scripts.Upgrades;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game.Scripts.UI
{
    public class UpgradeCardUI : MonoBehaviour
    {
        [SerializeField] private Image _cardBackground;
        [SerializeField] private Image _cardIcon;
        [SerializeField] private TextMeshProUGUI _cardNameText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private Button _selectButton;
        
        private UpgradeCard _upgradeData;
        private System.Action<UpgradeCard> _selectedCallback;
        
        private void Awake()
        {
            if (_selectButton != null)
                _selectButton.onClick.AddListener(OnCardClicked);
        }
        
         private void OnDestroy()
         {
             if (_selectButton != null)
                 _selectButton.onClick.RemoveListener(OnCardClicked);
         }

        public void Initialize(UpgradeCard upgradeData, System.Action<UpgradeCard> selectedCallback)
        {
            _upgradeData = upgradeData;
            _selectedCallback = selectedCallback;
            
            _cardBackground.color = upgradeData.CardColor;
            _cardIcon.sprite = upgradeData.CardIcon;
            _descriptionText.text = upgradeData.Description;
            _cardNameText.text = upgradeData.CardName;
        }
        
        private void OnCardClicked()
        {
            _selectedCallback?.Invoke(_upgradeData);
        }
    }
}