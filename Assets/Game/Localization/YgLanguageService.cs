using System;
using YG;

namespace Game.Localization
{
    public sealed class YgLanguageService : ILanguageService, IDisposable
    {
        private static readonly string[] SupportedLanguages = { "ru", "en", "tr" };
        
        public string CurrentLanguage => YG2.lang;
        public event Action<string> LanguageChanged;
        
        public YgLanguageService()
        {
            YG2.onSwitchLang += OnLanguageSwitched;
        }
        
        public void SwitchToNextLanguage()
        {
            int index = Array.IndexOf(SupportedLanguages, YG2.lang);
            int nextIndex = index < 0 ? 0 : (index + 1) % SupportedLanguages.Length;
            SwitchTo(SupportedLanguages[nextIndex]);
        }
        
        public void SwitchTo(string languageCode)
        {
            if (string.IsNullOrEmpty(languageCode))
                return;
            
            YG2.SwitchLanguage(languageCode.ToLower());
        }
        
        private void OnLanguageSwitched(string languageCode)
        {
            LanguageChanged?.Invoke(languageCode);
        }
        
        public void Dispose()
        {
            YG2.onSwitchLang -= OnLanguageSwitched;
        }
    }
}