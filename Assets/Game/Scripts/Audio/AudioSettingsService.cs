using UnityEngine;
using UnityEngine.Audio;

namespace Game.Scripts.Audio
{
    public class AudioSettingsService : IAudioSettingsService
    {
        private const string MusicVolumeKey = "MusicVolume";
        private const string SFXVolumeKey = "SFXVolume";
        private const string MusicMutedKey = "MusicMuted";
        private const string SFXMutedKey = "SFXMuted";

        private readonly AudioMixer _mixer;

        public float MusicVolume { get; private set; } = 1f;
        public float SFXVolume { get; private set; } = 1f;
        public bool IsMusicMuted { get; private set; }
        public bool IsSFXMuted { get; private set; }

        public AudioSettingsService(AudioMixer mixer)
        {
            _mixer = mixer;
            LoadSettings();
        }

        public void SetMusicVolume(float volume)
        {
            MusicVolume = Mathf.Clamp01(volume);
            ApplyMusicVolume();
        }

        public void SetSFXVolume(float volume)
        {
            SFXVolume = Mathf.Clamp01(volume);
            ApplySFXVolume();
        }

        public void ToggleMusicMute()
        {
            IsMusicMuted = !IsMusicMuted;
            ApplyMusicVolume();
        }

        public void ToggleSFXMute()
        {
            IsSFXMuted = !IsSFXMuted;
            ApplySFXVolume();
        }

        public void LoadSettings()
        {
            MusicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
            SFXVolume = PlayerPrefs.GetFloat(SFXVolumeKey, 1f);
            IsMusicMuted = PlayerPrefs.GetInt(MusicMutedKey, 0) == 1;
            IsSFXMuted = PlayerPrefs.GetInt(SFXMutedKey, 0) == 1;
        
            ApplyMusicVolume();
            ApplySFXVolume();
        }

        public void SaveSettings()
        {
            PlayerPrefs.SetFloat(MusicVolumeKey, MusicVolume);
            PlayerPrefs.SetFloat(SFXVolumeKey, SFXVolume);
            PlayerPrefs.SetInt(MusicMutedKey, IsMusicMuted ? 1 : 0);
            PlayerPrefs.SetInt(SFXMutedKey, IsSFXMuted ? 1 : 0);
            PlayerPrefs.Save();
        }
        
        private void ApplyMusicVolume()
        {
            float volume = IsMusicMuted ? 0f : MusicVolume;
            _mixer.SetFloat("MusicVolume", LinearToDecibel(volume));
        }
    
        private void ApplySFXVolume()
        {
            float volume = IsSFXMuted ? 0f : SFXVolume;
            _mixer.SetFloat("SFXVolume", LinearToDecibel(volume));
        }
    
        private float LinearToDecibel(float linear)
        {
            return linear > 0f ? 20f * Mathf.Log10(linear) : -80f;
        }
    }
}