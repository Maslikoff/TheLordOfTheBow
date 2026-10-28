using System;
using JTLStudio.SDK;

namespace Game.Localization
{
    /// <summary>
    /// Static language helper backed by <see cref="JTLSDK.Language"/>.
    /// Drop-in replacement for the former PluginYG2 <c>YG2.lang</c> (code string) and
    /// <c>YG2.onSwitchLang</c> (<see cref="Action{T}"/> of string) surface.
    /// </summary>
    public static class Loc
    {
        private const string Fallback = "ru";

        private static readonly LanguageCodes Codes = new LanguageCodes();
        private static bool _subscribed;

        /// <summary>Former <c>YG2.onSwitchLang</c>: fires with the new language code ("ru"/"en"/"tr"/...).</summary>
        public static event Action<string> Changed;

        /// <summary>Former <c>YG2.lang</c>: the active language code.</summary>
        public static string Lang => JTLSDK.IsCreated ? Codes.ToCode(JTLSDK.Language.Current) : Fallback;

        /// <summary>Called once by the SDK bootstrap right after <c>JTLSDK.Create()</c>.</summary>
        internal static void Init()
        {
            if (_subscribed || JTLSDK.IsCreated == false) return;
            JTLSDK.Language.Changed += OnChanged;
            _subscribed = true;
        }

        /// <summary>Former <c>YG2.SwitchLanguage(code)</c>.</summary>
        public static void SetByCode(string code)
        {
            if (JTLSDK.IsCreated == false || string.IsNullOrWhiteSpace(code)) return;
            if (Codes.TryParse(code, out Language language)) JTLSDK.Language.Set(language);
        }

        private static void OnChanged(Language language) => Changed?.Invoke(Codes.ToCode(language));
    }
}
