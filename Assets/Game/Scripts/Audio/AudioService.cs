using System;
using Cysharp.Threading.Tasks;
using Game.Scripts.AddressablesContent;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Scripts.Audio
{
    public class AudioService : IAudioService
    {
        private const float DestroyDelayPadding = 0.1f;

        private readonly GameObject _audioRoot;
        private readonly IAddressablesContentService _content;

        private AudioSource _musicSource;
        private AudioAsset _currentMusicAsset;

        public AudioService(IAddressablesContentService content)
        {
            _content = content ?? throw new ArgumentNullException(nameof(content));
            _audioRoot = new GameObject(nameof(AudioService));
            Object.DontDestroyOnLoad(_audioRoot);
        }

        public void PlayOneShot(AudioAsset asset)
        {
            PlayOneShotAsync(asset).Forget();
        }

        public void PlayMusic(AudioAsset asset)
        {
            PlayMusicAsync(asset).Forget();
        }

        private async UniTaskVoid PlayOneShotAsync(AudioAsset asset)
        {
            if (asset == null)
                return;

            if (asset.Category != AudioCategories.SFX)
                throw new InvalidOperationException();

            await asset.EnsureLoadedAsync(_content);

            AudioSource source = CreateSource(asset);
            source.Play();
            Object.Destroy(source.gameObject, GetDestroyDelay(source));
        }

        private async UniTaskVoid PlayMusicAsync(AudioAsset asset)
        {
            if (asset == null)
                return;

            if (asset.Category != AudioCategories.Music)
                throw new InvalidOperationException();

            if (asset == _currentMusicAsset && _musicSource != null && _musicSource.isPlaying)
                return;

            await asset.EnsureLoadedAsync(_content);

            StopMusic();

            _musicSource = CreateSource(asset);
            _musicSource.loop = true;
            _currentMusicAsset = asset;
            _musicSource.Play();
        }

        private void StopMusic()
        {
            if (_musicSource == null)
                return;

            _musicSource.Stop();
            Object.Destroy(_musicSource.gameObject);
            _musicSource = null;
            _currentMusicAsset = null;
        }

        private AudioSource CreateSource(AudioAsset asset)
        {
            AudioClip clip = asset.GetRandomClip();

            GameObject audioObject = new GameObject(clip.name);
            audioObject.transform.SetParent(_audioRoot.transform);

            AudioSource source = audioObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.outputAudioMixerGroup = asset.OutputMixerGroup;
            source.volume = asset.Volume;
            source.pitch = asset.Pitch;
            source.playOnAwake = false;

            return source;
        }

        private float GetDestroyDelay(AudioSource source)
        {
            return source.clip.length / source.pitch + DestroyDelayPadding;
        }
    }
}
