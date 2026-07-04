using System;
using System.Collections.Generic;
using Game.Scripts.Levels;
using Game.Scripts.Upgrades;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using Random = UnityEngine.Random;

namespace Game.Scripts.UI
{
    public class UpgradeChoicePanel : MonoBehaviour
    {
        private const int CountCards = 3;
        
        [Header("UI Settings")]
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Transform _cardsContainer;
        [SerializeField] private UpgradeCardUI _cardPrefab;
        [SerializeField] private Button _skipButton;
        
        private List<Upgrades.UpgradeCard> _allUpgrades = new();
        private List<UpgradeCardUI> _currentCards = new List<UpgradeCardUI>();
        private UpgradeApplier _upgradeApplier;
        private Experience.Experience _playerExperience;
        
        private IObjectFactory _objectFactory;

        [Inject]
        public void Construct(IObjectFactory objectFactory)
        {
            _objectFactory = objectFactory ?? throw new ArgumentNullException(nameof(objectFactory));
        }
        
        private void Awake()
        {
            _panelRoot.SetActive(false);
            
            if (_skipButton != null)
                _skipButton.onClick.AddListener(HidePanel);
        }
        
        private void OnDestroy()
        {
            if (_playerExperience != null)
                _playerExperience.LevelUp -= OnPlayerLevelUp;
        }
        
        public void Initialize(Experience.Experience playerExperience, UpgradeApplier upgradeApplier)
        {
            if (_playerExperience != null)
                _playerExperience.LevelUp -= OnPlayerLevelUp;
            
            _playerExperience = playerExperience ?? throw new ArgumentNullException(nameof(playerExperience));
            _upgradeApplier = upgradeApplier ?? throw new ArgumentNullException(nameof(upgradeApplier));

            _playerExperience.LevelUp += OnPlayerLevelUp;
        }
        
        private void OnPlayerLevelUp(int newLevel)
        {
            ShowUpgradeChoice();
        }
        
        public void SetAvailableUpgrades(IReadOnlyList<Upgrades.UpgradeCard> upgrades)
        {
            _allUpgrades.Clear();
        
            if (upgrades != null && upgrades.Count > 0)
                _allUpgrades.AddRange(upgrades);
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
                var cardGO = _objectFactory.Create(_cardPrefab, _cardsContainer);
                cardGO.Initialize(upgrade, OnUpgradeSelected);
                _currentCards.Add(cardGO);
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