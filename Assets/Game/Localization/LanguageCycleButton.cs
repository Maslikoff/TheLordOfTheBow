using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Game.Localization
{
    public sealed class LanguageCycleButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _label;
        
        private ILanguageService _languageService;
        
        [Inject]
        public void Construct(ILanguageService languageService)
        {
            _languageService = languageService;
        }
        
        private void OnEnable()
        {
            if (_button != null)
                _button.onClick.AddListener(OnClick);
            
            YG.YG2.onSwitchLang += RefreshLabel;
            RefreshLabel(YG.YG2.lang);
        }
        
        private void OnDisable()
        {
            if (_button != null)
                _button.onClick.RemoveListener(OnClick);
            
            YG.YG2.onSwitchLang -= RefreshLabel;
        }
        
        private void OnClick()
        {
            _languageService?.SwitchToNextLanguage();
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