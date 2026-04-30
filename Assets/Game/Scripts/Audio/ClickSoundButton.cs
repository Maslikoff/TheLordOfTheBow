using System;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Game.Scripts.Audio
{
    [RequireComponent(typeof(Button))]
    public class ClickSoundButton : MonoBehaviour
    {
        [SerializeField] private AudioAsset _asset;

        private Button _button;
        
        private IAudioService _audioService;

        [Inject]
        public void Construct(IAudioService audioService)
        {
            _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
        }

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(OnClick);
        }
        
        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnClick);
        }

        private void OnClick()
        {
            _audioService.PlayOneShot(_asset);
        }
    }
}