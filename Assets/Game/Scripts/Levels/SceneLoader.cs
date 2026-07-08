using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TransitionsPlus;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Game.Scripts.Levels
{
    public class SceneLoader : ISceneLoader
    {
        private readonly TransitionProfile _transitionProfile;

        public SceneLoader()
        {
            const string Path = "Animator/TransitionProfile";
            
            _transitionProfile = Resources.Load<TransitionProfile>(Path)
                                 ?? throw new ArgumentNullException(nameof(_transitionProfile),
                                     $"TransitionProfile not found at Resources/{Path}");
        }

        public async UniTask LoadAsync(
            SceneNames sceneName,
            SceneTransitionMode transitionMode = SceneTransitionMode.CloseAndOpen,
            Action onComplete = null)
        {
            switch (transitionMode)
            {
                case SceneTransitionMode.CloseAndOpen:
                    await PlayTransitionAsync(invertTransition: false, autoDestroy: false);
                    await LoadSceneAsync(sceneName, onComplete);
                    await PlayTransitionAsync(invertTransition: true);
                    break;
                
                case SceneTransitionMode.OpenOnly:
                    await LoadSceneAsync(sceneName, onComplete);
                    await PlayTransitionAsync(invertTransition: true);
                    break;
                
                case SceneTransitionMode.None:
                    await LoadSceneAsync(sceneName, onComplete);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(transitionMode), transitionMode, null);
            }
        }
        
        private async UniTask LoadSceneAsync(SceneNames sceneName, Action onComplete = null)
        {
            DOTween.KillAll();
            
            await SceneManager.LoadSceneAsync(sceneName.ToString(), LoadSceneMode.Single).ToUniTask();
            
            onComplete?.Invoke();
        }
        
        private async UniTask PlayTransitionAsync(bool invertTransition, bool autoDestroy = true)
        {
            TransitionProfile transitionProfileInstance = UnityEngine.Object.Instantiate(_transitionProfile);
            transitionProfileInstance.invert = invertTransition;
            TransitionAnimator transitionAnimator = TransitionAnimator.Start(transitionProfileInstance, autoDestroy);
            
            await WaitTransitionEndAsync(transitionAnimator);
            
            UnityEngine.Object.Destroy(transitionProfileInstance);
        }
        
        private static UniTask WaitTransitionEndAsync(TransitionAnimator transitionAnimator)
        {
            if (transitionAnimator == null)
                return UniTask.CompletedTask;
            
            var tcs = new UniTaskCompletionSource();
            
            UnityAction onEnd = null;
            onEnd = () =>
            {
                transitionAnimator.onTransitionEnd.RemoveListener(onEnd);
                tcs.TrySetResult();
            };
            
            transitionAnimator.onTransitionEnd.AddListener(onEnd);
            
            return tcs.Task;
        }
    }
}