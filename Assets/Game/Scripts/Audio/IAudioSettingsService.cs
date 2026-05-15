namespace Game.Scripts.Audio
{
    public interface IAudioSettingsService
    {
        float MusicVolume { get; }
        float SFXVolume { get; }
        bool IsMusicMuted { get; }
        bool IsSFXMuted { get; }
        
        void SetMusicVolume(float volume);
        void SetSFXVolume(float volume);
        void ToggleMusicMute();
        void ToggleSFXMute();
        void LoadSettings();
        void SaveSettings();
    }
}