using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Scripts.AddressablesContent;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

namespace Game.Scripts.Audio
{
    [CreateAssetMenu(fileName = "AudioAsset", menuName = "Game/Audio/Audio Asset", order = 0)]
    public class AudioAsset : ScriptableObject
    {
        [SerializeField] private AudioCategories _category;
        [SerializeField] private AudioMixerGroup _outputMixerGroup;
        [SerializeField] private AssetReferenceT<AudioClip>[] _audioClipReferences;
        [SerializeField] [Range(0f, 1f)] private float _volume = 1f;
        [SerializeField] [Range(0.1f, 3f)] private float _pitch = 1f;

        [NonSerialized] private AudioClip[] _resolvedClips;
        [NonSerialized] private bool _isLoaded;
        [NonSerialized] private UniTaskCompletionSource _loadTcs;

        public AudioCategories Category => _category;
        public AudioMixerGroup OutputMixerGroup => _outputMixerGroup;
        public float Volume => _volume;
        public float Pitch => _pitch;

        private void OnEnable()
        {
            // ScriptableObject fields survive Play Mode when domain reload is disabled.
            ResetRuntimeState();
        }

        private void OnDisable()
        {
            ResetRuntimeState();
        }

        public async UniTask EnsureLoadedAsync(IAddressablesContentService content,
            CancellationToken cancellationToken = default)
        {
            while (true)
            {
                if (HasValidClips())
                    return;

                UniTaskCompletionSource inFlight = _loadTcs;
                if (inFlight != null)
                {
                    await inFlight.Task;
                    continue;
                }

                if (_audioClipReferences == null || _audioClipReferences.Length == 0)
                    throw new InvalidOperationException($"{name} has no audio clip references.");

                var tcs = new UniTaskCompletionSource();
                _loadTcs = tcs;

                try
                {
                    await LoadClipsAsync(content, cancellationToken);
                    tcs.TrySetResult();
                    return;
                }
                catch (Exception exception)
                {
                    _resolvedClips = null;
                    _isLoaded = false;
                    tcs.TrySetException(exception);
                    throw;
                }
                finally
                {
                    if (_loadTcs == tcs)
                        _loadTcs = null;
                }
            }
        }

        private async UniTask LoadClipsAsync(IAddressablesContentService content,
            CancellationToken cancellationToken)
        {
            var clips = new AudioClip[_audioClipReferences.Length];

            for (int i = 0; i < _audioClipReferences.Length; i++)
            {
                AssetReferenceT<AudioClip> reference = _audioClipReferences[i];
                if (reference == null || reference.RuntimeKeyIsValid() == false)
                    throw new InvalidOperationException($"{name} clip reference[{i}] is invalid.");

                AudioClip clip = await content.LoadAssetAsync<AudioClip>(reference, cancellationToken);
                if (clip == null)
                    throw new InvalidOperationException(
                        $"{name} failed to load clip[{i}] ({reference.AssetGUID}).");

                clips[i] = clip;
            }

            _resolvedClips = clips;
            _isLoaded = true;
        }

        public AudioClip GetRandomClip()
        {
            if (HasValidClips() == false)
                throw new InvalidOperationException($"{name} clips are not loaded. Call EnsureLoadedAsync first.");

            int index = Random.Range(0, _resolvedClips.Length);
            AudioClip clip = _resolvedClips[index];
            if (clip == null)
                throw new InvalidOperationException($"{name} resolved clip[{index}] is null.");

            return clip;
        }

        private bool HasValidClips()
        {
            if (_isLoaded == false || _resolvedClips == null || _resolvedClips.Length == 0)
                return false;

            for (int i = 0; i < _resolvedClips.Length; i++)
            {
                if (_resolvedClips[i] == null)
                    return false;
            }

            return true;
        }

        private void ResetRuntimeState()
        {
            _resolvedClips = null;
            _isLoaded = false;
            _loadTcs = null;
        }
    }
}
