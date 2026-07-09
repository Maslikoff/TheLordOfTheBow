using System;

namespace Game.Localization
{
    public interface ILanguageService
    {
        string CurrentLanguage { get; }
        event Action<string> LanguageChanged;
        void SwitchToNextLanguage();
        void SwitchTo(string languageCode);
    }
}