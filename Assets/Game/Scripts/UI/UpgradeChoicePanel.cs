using System.Collections.Generic;
using Game.Scripts.Upgrades;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.UI
{
    public class UpgradeChoicePanel : MonoBehaviour
    {
        private const int CountCards = 3;
        
        [SerializeField] private UpgradeApplier _upgradeApplier;
        [SerializeField] private Experience.Experience _playerExperience;
        
        [Header("UI Settings")]
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Transform _cardsContainer;
        [SerializeField] private GameObject _cardPrefab;
        [SerializeField] private Button _skipButton;
        
        [Header("Upgrades Pool")]
        [SerializeField] private List<Upgrades.UpgradeCard> _allUpgrades = new();
        
        private List<UpgradeCardUI> _currentCards = new List<UpgradeCardUI>();
        
        private void Awake()
        {
            _panelRoot.SetActive(false);
            
            if (_skipButton != null)
                _skipButton.onClick.AddListener(HidePanel);
        }
        
        private void Start()
        {
            if (_playerExperience != null)
                _playerExperience.LevelUp += OnPlayerLevelUp;
        }
        
        private void OnDestroy()
        {
            if (_playerExperience != null)
                _playerExperience.LevelUp -= OnPlayerLevelUp;
        }
        
        private void OnPlayerLevelUp(int newLevel)
        {
            ShowUpgradeChoice();
        }
        
        public void ShowUpgradeChoice()
        {
            if (_upgradeApplier == null)
                return;
            
            Time.timeScale = 0f;
            _panelRoot.SetActive(true);
            
            foreach (var card in _currentCards)
            {
                if (card != null)
                    Destroy(card.gameObject);
            }
            _currentCards.Clear();
            
            var selectedCards = GetRandomUpgradeCards(CountCards);
            
            foreach (Upgrades.UpgradeCard upgrade in selectedCards)
            {
                var cardGO = Instantiate(_cardPrefab, _cardsContainer);
                var cardUI = cardGO.GetComponent<UpgradeCardUI>();
                cardUI.Initialize(upgrade, OnUpgradeSelected);
                _currentCards.Add(cardUI);
            }
        }
        
        private List<Upgrades.UpgradeCard> GetRandomUpgradeCards(int count)
        {
            
            var availableUpgrades = new List<Upgrades.UpgradeCard>(_allUpgrades);
            var selected = new List<Upgrades.UpgradeCard>();
            
            for (int i = 0; i < count && availableUpgrades.Count > 0; i++)
            {
                int randomIndex = Random.Range(0, availableUpgrades.Count);
                selected.Add(availableUpgrades[randomIndex]);
                availableUpgrades.RemoveAt(randomIndex);
            }
            
            return selected;
        }
        
        private void OnUpgradeSelected(Upgrades.UpgradeCard selectedUpgrade)
        {
            _upgradeApplier.ApplyUpgrade(selectedUpgrade);
            
            HidePanel();
        }
        
        private void HidePanel()
        {
            _panelRoot.SetActive(false);
            Time.timeScale = 1f;
        }
    }
}