using Game.Scripts.Shop;
using Game.Scripts.StateServices;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Game.Scripts.UI.Shop
{
    public class PreStartShopButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private ShopPanel _shopPanel;
        [SerializeField] private GameObject _root;

        private IGameStateService _gameState;

        [Inject]
        private void Construct(IGameStateService gameState)
        {
            _gameState = gameState;
        }

        private void Awake()
        {
            if (_button == null)
                _button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            if (_button != null)
                _button.onClick.AddListener(OpenShop);

            if (_gameState != null)
                _gameState.GameStarted += OnGameStarted;

            RefreshVisibility();
        }

        private void OnDisable()
        {
            if (_button != null)
                _button.onClick.RemoveListener(OpenShop);

            if (_gameState != null)
                _gameState.GameStarted -= OnGameStarted;
        }

        private void OpenShop()
        {
            if (_gameState != null && _gameState.IsGameStarted)
                return;

            _shopPanel?.Open();
        }

        private void OnGameStarted()
        {
            _shopPanel?.Close();
            RefreshVisibility();
        }

        private void RefreshVisibility()
        {
            bool visible = _gameState == null || _gameState.IsGameStarted == false;
            if (_root != null)
                _root.SetActive(visible);
            else
                gameObject.SetActive(visible);
        }
    }
}
