using System;
using System.Collections.Generic;
using Game.Scripts.Levels;
using Game.Scripts.StateServices;
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
        private int _pendingLevelUps;
        
        private IObjectFactory _objectFactory;
        private IPauseService _pauseService;
        private IModalCoordinator _modalCoordinator;

        [Inject]
        public void Construct(IObjectFactory objectFactory, IPauseService pauseService, IModalCoordinator modalCoordinator)
        {
            _objectFactory = objectFactory ?? throw new ArgumentNullException(nameof(objectFactory));
            _pauseService = pauseService ?? throw new ArgumentNullException(nameof(pauseService));
            _modalCoordinator = modalCoordinator ?? throw new ArgumentNullException(nameof(modalCoordinator));
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
            
            _pauseService?.Resume(this);
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
            if (_modalCoordinator.CurrentModal == ModalType.Upgrade)
            {
                _pendingLevelUps++;
                return;
            }
            
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
            
            _modalCoordinator.RequestShow(
                ModalType.Upgrade,
                ModalPriority.Upgrade,
                OpenPanelInternal);
        }
        
        private void OpenPanelInternal()
        {
            _pauseService.Pause(this);
            _panelRoot.SetActive(true);
            _panelRoot.transform.SetAsLastSibling();
            
            foreach (var card in _currentCards)
                if (card != null) Destroy(card.gameObject);
            
            _currentCards.Clear();
            
            var selectedCards = GetRandomUpgradeCards(CountCards);
            
            foreach (var upgrade in selectedCards)
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
            _pauseService.Resume(this);
            
            if (_pendingLevelUps > 0)
            {
                _pendingLevelUps--;
                OpenPanelInternal();
                
                return;
            }
            _modalCoordinator.NotifyClosed(ModalType.Upgrade);
        }
    }
}