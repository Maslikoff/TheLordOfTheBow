using Game.Scripts.Audio;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Game.Scripts.UI
{
    public class AudioSettingsUI : MonoBehaviour
    {
        [Header("Music")]
        [SerializeField] private Slider _musicSlider;
        [SerializeField] private Button _musicMuteButton;
        [SerializeField] private Image _musicIcon;
        [SerializeField] private Sprite _musicOnSprite;
        [SerializeField] private Sprite _musicOffSprite;
    
        [Header("SFX")]
        [SerializeField] private Slider _sfxSlider;
        [SerializeField] private Button _sfxMuteButton;
        [SerializeField] private Image _sfxIcon;
        [SerializeField] private Sprite _sfxOnSprite;
        [SerializeField] private Sprite _sfxOffSprite;
    
        private IAudioSettingsService _audioSettings;
    
        [Inject]
        private void Construct(IAudioSettingsService audioSettings)
        {
            _audioSettings = audioSettings;
        }
        
        private void Start()
        {
            _musicSlider.value = _audioSettings.MusicVolume;
            _sfxSlider.value = _audioSettings.SFXVolume;
            
            UpdateMusicIcon();
            UpdateSFXIcon();
        
            _musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
            _sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);
            _musicMuteButton.onClick.AddListener(OnMusicMuteClicked);
            _sfxMuteButton.onClick.AddListener(OnSFXMuteClicked);
        }
    
        private void OnMusicSliderChanged(float value)
        {
            _audioSettings.SetMusicVolume(value);
        }
    
        private void OnSFXSliderChanged(float value)
        {
            _audioSettings.SetSFXVolume(value);
        }
    
        private void OnMusicMuteClicked()
        {
            _audioSettings.ToggleMusicMute();
            UpdateMusicIcon();
        }
    
        private void OnSFXMuteClicked()
        {
            _audioSettings.ToggleSFXMute();
            UpdateSFXIcon();
        }
    
        private void UpdateMusicIcon()
        {
            _musicIcon.sprite = _audioSettings.IsMusicMuted ? _musicOffSprite : _musicOnSprite;
        }
    
        private void UpdateSFXIcon()
        {
            _sfxIcon.sprite = _audioSettings.IsSFXMuted ? _sfxOffSprite : _sfxOnSprite;
        }
    
        private void OnDestroy()
        {
            _audioSettings.SaveSettings();
        }
    }
}