using System;
using System.Collections.Generic;
using System.Linq;
using Game.Scripts.Levels;
using Game.Scripts.Shop;
using Game.Scripts.StateServices;
using Game.Scripts.UpgradeCard;
using Game.Scripts.Upgrades;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VContainer;
using YG;
using Random = UnityEngine.Random;

namespace Game.Scripts.UI
{
    public class UpgradeChoicePanel : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Transform _cardsContainer;
        [SerializeField] private UpgradeCardUI _cardPrefab;
        [SerializeField] private Button _skipButton;
        [SerializeField] private Button _rerollButton;
        private readonly List<Upgrades.UpgradeCard> _allUpgrades = new();
        private readonly List<UpgradeCardUI> _currentCards = new();
        private List<CampaignUpgradeCard> _offer = new();
        private UpgradeApplier _upgradeApplier;
        private Experience.Experience _playerExperience;
        private int _pendingLevelUps;
        private bool _requested;
        private bool _selecting;
        private IObjectFactory _objectFactory;
        private IPauseService _pauseService;
        private IModalCoordinator _modalCoordinator;
        private ICurrencyService _currency;
        public bool HasPendingChoices => _pendingLevelUps > 0 || _requested;
        public IReadOnlyList<CampaignUpgradeCard> CurrentOffer => _offer;

        [Inject]
        public void Construct(IObjectFactory factory, IPauseService pause, IModalCoordinator modals, ICurrencyService currency)
        {
            _objectFactory = factory;
            _pauseService = pause;
            _modalCoordinator = modals;
            _currency = currency;
        }

        private void Awake()
        {
            _panelRoot.SetActive(false);
            if (_skipButton != null) _skipButton.onClick.AddListener(HidePanel);
            if (_rerollButton != null) _rerollButton.onClick.AddListener(Reroll);
        }

        private void OnEnable() => YG2.onSwitchLang += OnLanguage;
        private void OnDisable() => YG2.onSwitchLang -= OnLanguage;
        private void OnLanguage(string _) => RefreshReroll();
        private void OnDestroy()
        {
            if (_playerExperience != null) _playerExperience.LevelUp -= OnPlayerLevelUp;
            _pauseService?.Resume(this);
        }

        public void Initialize(Experience.Experience experience, UpgradeApplier applier)
        {
            if (_playerExperience != null) _playerExperience.LevelUp -= OnPlayerLevelUp;
            _playerExperience = experience;
            _upgradeApplier = applier;
            _playerExperience.LevelUp += OnPlayerLevelUp;
        }

        private void OnPlayerLevelUp(int _) => ShowUpgradeChoice();
        public void SetAvailableUpgrades(IReadOnlyList<Upgrades.UpgradeCard> upgrades)
        {
            _allUpgrades.Clear();
            if (upgrades != null) _allUpgrades.AddRange(upgrades);
        }

        public void ShowUpgradeChoice()
        {
            if (_upgradeApplier == null) return;
            _pendingLevelUps++;
            if (_requested) return;
            _requested = true;
            _modalCoordinator.RequestShow(ModalType.Upgrade, ModalPriority.Upgrade, OpenPanelInternal);
        }

        public void ResetPendingState()
        {
            _pendingLevelUps = 0;
            _requested = false;
            _panelRoot.SetActive(false);
            _pauseService?.Resume(this);
            if (_modalCoordinator?.CurrentModal == ModalType.Upgrade) _modalCoordinator.NotifyClosed(ModalType.Upgrade);
            ClearCards();
        }

        private List<CampaignUpgradeCard> Available()
        {
            var holder = _upgradeApplier.Holder;
            var all = _allUpgrades.OfType<CampaignUpgradeCard>().Where(x => x.IsAvailable(holder)).GroupBy(x => x.Id).Select(x => x.First()).ToList();
            var normal = all.Where(x => x.Kind != CampaignUpgradeKind.Heal && x.Kind != CampaignUpgradeKind.Coins).ToList();
            if (normal.Count < 3)
            {
                var heal = all.FirstOrDefault(x => x.Kind == CampaignUpgradeKind.Heal);
                if (heal != null) normal.Add(heal);
                var coins = all.FirstOrDefault(x => x.Kind == CampaignUpgradeKind.Coins);
                if (normal.Count < 3 && coins != null) normal.Add(coins);
            }
            return normal;
        }

        private List<CampaignUpgradeCard> MakeOffer()
        {
            var pool = Available();
            var selected = new List<CampaignUpgradeCard>();
            var preferred = pool.Where(x => IsWeapon(x.Kind) && _upgradeApplier.Holder.Campaign.Rank(x.Kind) > 0).ToList();
            if (preferred.Count > 0)
            {
                var card = preferred[Random.Range(0, preferred.Count)];
                selected.Add(card);
                pool.Remove(card);
            }
            while (selected.Count < 3 && pool.Count > 0)
            {
                int index = Random.Range(0, pool.Count);
                selected.Add(pool[index]);
                pool.RemoveAt(index);
            }
            return selected;
        }

        private static bool IsWeapon(CampaignUpgradeKind kind) => kind != CampaignUpgradeKind.Health && kind != CampaignUpgradeKind.Shield && kind != CampaignUpgradeKind.Heal && kind != CampaignUpgradeKind.Coins;
        private void OpenPanelInternal()
        {
            _pauseService.Pause(this);
            _panelRoot.SetActive(true);
            _panelRoot.transform.parent?.SetAsLastSibling();
            _panelRoot.transform.SetAsLastSibling();
            _offer = MakeOffer();
            DrawOffer();
        }

        private void ClearCards()
        {
            foreach (var card in _currentCards) if (card != null) Destroy(card.gameObject);
            _currentCards.Clear();
        }

        private void DrawOffer()
        {
            ClearCards();
            _selecting = false;
            foreach (var upgrade in _offer)
            {
                var card = _objectFactory.Create(_cardPrefab, _cardsContainer);
                card.Initialize(upgrade, OnUpgradeSelected, _upgradeApplier.Holder);
                _currentCards.Add(card);
            }
            RefreshReroll();
        }

        private void RefreshReroll()
        {
            if (_rerollButton == null || _upgradeApplier == null) return;
            int count = _upgradeApplier.Holder.Campaign.Rerolls;
            _rerollButton.gameObject.SetActive(_upgradeApplier.Holder.Campaign.UnlockedStage >= 10);
            _rerollButton.interactable = count > 0 && Available().Any(x => !_offer.Contains(x));
            var label = _rerollButton.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null) label.text = (YG2.lang switch { "en" => "Reroll", "tr" => "Yenile", _ => "Переброс" }) + $" ({count})";
        }

        public void Reroll()
        {
            if (!HasPendingChoices || _upgradeApplier.Holder.Campaign.Rerolls <= 0) return;
            var alternatives = Available().Where(x => !_offer.Contains(x)).ToList();
            if (alternatives.Count == 0) return;
            var next = MakeOffer();
            if (next.Count == _offer.Count && next.All(_offer.Contains))
                next[next.Count - 1] = alternatives[Random.Range(0, alternatives.Count)];
            if (!_upgradeApplier.Holder.Campaign.SpendReroll()) return;
            _offer = next;
            DrawOffer();
        }

        private void OnUpgradeSelected(Upgrades.UpgradeCard card)
        {
            if (_selecting || !HasPendingChoices || card is not CampaignUpgradeCard campaign || !_offer.Contains(campaign)) return;
            _selecting = true;
            if (campaign.Kind == CampaignUpgradeKind.Coins) _currency.Add(25);
            else if (campaign.IsAvailable(_upgradeApplier.Holder)) _upgradeApplier.ApplyUpgrade(card);
            HidePanel();
        }

        private void HidePanel()
        {
            if (_pendingLevelUps <= 0) return;
            _pendingLevelUps--;
            if (_pendingLevelUps > 0) { OpenPanelInternal(); return; }
            _requested = false;
            _panelRoot.SetActive(false);
            _pauseService.Resume(this);
            _modalCoordinator.NotifyClosed(ModalType.Upgrade);
        }
    }
}
