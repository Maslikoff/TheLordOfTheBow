using DG.Tweening;
using Game.Scripts.StateServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;

namespace Game.Scripts.UI
{
    public class TapToStartZone : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Visual Elements")] 
        [SerializeField] private RectTransform _textContainer;
        [SerializeField] private TMPro.TextMeshProUGUI _tapText;
        [SerializeField] private GameObject _prefabAnimationTutorialImage;
        [SerializeField] private Image _tapZoneBackground;

        [Header("Animation Settings")] 
        [SerializeField] private float _idlePulseSpeed = 1f;
        [SerializeField] private float _hoverScale = 1.1f;
        [SerializeField] private float _clickScaleDuration = 0.2f;
        [SerializeField] private float _fadeDuration = 0.5f;

        [Header("References")] 
        [SerializeField] private GameObject _gameCamera;
        [SerializeField] private GameObject _menuCamera;
        [SerializeField] private MonoBehaviour[] _gameScriptsToEnable;

        private bool _isGameStarted = false;
        private Tween _textScaleTween;
        
        private IGameStateService _gameStateService;
        
        [Inject]
        private void Construct(IGameStateService gameStateService)
        {
            _gameStateService = gameStateService;
        }
        
        private void Start()
        {
            MakeFullscreenBlocker();
            PlayIdleAnimation();
        }
        
        private void MakeFullscreenBlocker()
        {
            RectTransform rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
                rectTransform = gameObject.AddComponent<RectTransform>();
        
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
        
            Image image = GetComponent<Image>();
            if (image == null)
                image = gameObject.AddComponent<Image>();
        
            image.color = new Color(0, 0, 0, 0);
            image.raycastTarget = true;
        
            Canvas canvas = GetComponent<Canvas>();
            
            if (canvas == null)
                canvas = gameObject.AddComponent<Canvas>();
        
            canvas.overrideSorting = true;
            canvas.sortingOrder = 999;
        
            GraphicRaycaster raycaster = GetComponent<GraphicRaycaster>();
            
            if (raycaster == null)
                raycaster = gameObject.AddComponent<GraphicRaycaster>();
        
            _tapZoneBackground = image;
        }

        private void PlayIdleAnimation()
        {
            if (_tapText != null)
            {
                _tapText.alpha = 1f;
                _textScaleTween = _tapText.transform.DOScale(1.1f, _idlePulseSpeed)
                    .SetEase(Ease.InOutQuad)
                    .SetLoops(-1, LoopType.Yoyo);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log("Tap detected!");
        
            if (_isGameStarted) 
                return;
        
            StartGame();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_isGameStarted) 
                return;
        
            if (_textContainer != null)
                _textContainer.DOScale(_hoverScale, 0.3f).SetEase(Ease.OutBack);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_isGameStarted) 
                return;
        
            if (_textContainer != null)
                _textContainer.DOScale(1f, 0.3f).SetEase(Ease.OutQuad);
        }

        private void StartGame()
        {
            Debug.Log("Starting game...");
            
            _isGameStarted = true;
        
            _textScaleTween?.Kill();
        
            Sequence fadeSequence = DOTween.Sequence();
        
            fadeSequence.Join(_textContainer.DOScale(0f, _fadeDuration).SetEase(Ease.InBack));
            fadeSequence.Join(_tapText.DOFade(0f, _fadeDuration));
            fadeSequence.Join(_tapZoneBackground.DOFade(0f, _fadeDuration));
            _prefabAnimationTutorialImage.SetActive(false);
        
            fadeSequence.OnComplete(() =>
            {
                gameObject.SetActive(false);
                SwitchToGameCamera();
                EnableGameScripts();
                
                if (_gameStateService != null)
                    _gameStateService.StartGame();
            });
        
            fadeSequence.Play();
        }

        private void SwitchToGameCamera()
        {
            if (_menuCamera != null)
                _menuCamera.SetActive(false);
        
            if (_gameCamera != null)
            {
                _gameCamera.SetActive(true);
            
                Camera cam = _gameCamera.GetComponent<Camera>();
                
                if (cam != null)
                    cam.DOFieldOfView(70f, 1f).From(90f).SetEase(Ease.OutQuad);
            }
        }

        private void EnableGameScripts()
        {
            foreach (var script in _gameScriptsToEnable)
            {
                if (script != null)
                    script.enabled = true;
            }
        }
    }
}