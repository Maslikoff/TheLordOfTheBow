using System;
using UnityEngine;
using VContainer;

namespace Game.Scripts.Audio
{
    public class MusicStarter : MonoBehaviour
    {
        [SerializeField] private AudioAsset _asset;

        private IAudioService _audioService;

        [Inject]
        public void Construct(IAudioService audioService)
        {
            _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
        }

        private void Start()
        {
            _audioService.PlayMusic(_asset);
        }
    }
}