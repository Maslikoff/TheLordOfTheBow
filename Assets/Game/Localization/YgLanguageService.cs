using System;

namespace Game.Localization
{
    /// <summary>
    /// Language service backed by the JTL SDK (through <see cref="Loc"/>).
    /// Keeps the <see cref="ILanguageService"/> surface so DI consumers are unchanged.
    /// </summary>
    public sealed class YgLanguageService : ILanguageService, IDisposable
    {
        private static readonly string[] SupportedLanguages = { "ru", "en", "tr" };

        public string CurrentLanguage => Loc.Lang;
        public event Action<string> LanguageChanged;

        public YgLanguageService()
        {
            Loc.Changed += OnLanguageSwitched;
        }

        public void SwitchToNextLanguage()
        {
            int index = Array.IndexOf(SupportedLanguages, Loc.Lang);
            int nextIndex = index < 0 ? 0 : (index + 1) % SupportedLanguages.Length;
            SwitchTo(SupportedLanguages[nextIndex]);
        }

        public void SwitchTo(string languageCode)
        {
            if (string.IsNullOrEmpty(languageCode))
                return;

            Loc.SetByCode(languageCode.ToLower());
        }

        private void OnLanguageSwitched(string languageCode)
        {
            LanguageChanged?.Invoke(languageCode);
        }

        public void Dispose()
        {
            Loc.Changed -= OnLanguageSwitched;
        }
    }
}
