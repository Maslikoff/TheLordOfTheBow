using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Game.Localization
{
    public sealed class LanguageCycleButton : MonoBehaviour
    {
        private static readonly string[] SupportedLanguages = { "ru", "en", "tr" };

        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _label;

        private ILanguageService _languageService;

        [Inject]
        public void Construct(ILanguageService languageService)
        {
            _languageService = languageService;
        }

        private void Awake()
        {
            if (_button == null)
                _button = GetComponent<Button>();

            if (_label == null)
                _label = GetComponentInChildren<TMP_Text>(true);
        }

        private void OnEnable()
        {
            if (_button != null)
                _button.onClick.AddListener(OnClick);

            Loc.Changed += RefreshLabel;
            RefreshLabel(Loc.Lang);
        }

        private void OnDisable()
        {
            if (_button != null)
                _button.onClick.RemoveListener(OnClick);

            Loc.Changed -= RefreshLabel;
        }

        private void OnClick()
        {
            if (_languageService != null)
            {
                _languageService.SwitchToNextLanguage();
                return;
            }

            int index = System.Array.IndexOf(SupportedLanguages, Loc.Lang);
            int nextIndex = index < 0 ? 0 : (index + 1) % SupportedLanguages.Length;
            Loc.SetByCode(SupportedLanguages[nextIndex]);
        }

        private void RefreshLabel(string lang)
        {
            if (_label == null)
                return;

            _label.text = lang switch
            {
                "ru" => "RU",
                "tr" => "TR",
                _ => "EN"
            };
        }
    }
}