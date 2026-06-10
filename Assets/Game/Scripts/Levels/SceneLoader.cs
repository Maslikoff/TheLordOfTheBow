using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TransitionsPlus;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Scripts.Levels
{
    public class SceneLoader : ISceneLoader
    {
        private readonly TransitionProfile _transitionProfile;

        public SceneLoader()
        {
            const string Path = "Animator/TransitionProfile";
            
            _transitionProfile = Resources.Load<TransitionProfile>(Path) ?? throw new ArgumentNullException();
        }
        
        /*public async UniTask LoadAsync(SceneNames sceneName)
        {
            await SceneManager.LoadSceneAsync(sceneName.ToString());
        }*/

        public async UniTask LoadAsync(SceneNames sceneName, SceneTransitionMode transitionMode = SceneTransitionMode.CloseAndOpen,
            Action onComplete = null)
        {
            switch (transitionMode)
            {
                case SceneTransitionMode.CloseAndOpen:
                    CreateTransition(invertTransition:false, autoDestroy:false, 
                        onComplete:() => LoadAndOpenScene(sceneName, onComplete).Forget());
                    break;
                
                case SceneTransitionMode.OpenOnly:
                    LoadAndOpenScene(sceneName, onComplete).Forget();
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(transitionMode));
            }
        }

        private async UniTask LoadAndOpenScene(SceneNames sceneName, Action onComplete = null)
        {
            await LoadSceneAsync(sceneName, onComplete);

            CreateTransition(invertTransition: true);
        }

        private async UniTask LoadSceneAsync(SceneNames sceneName, Action onComplete = null)
        {
            DOTween.KillAll();

            await SceneManager.LoadSceneAsync(sceneName.ToString(), LoadSceneMode.Single).ToUniTask();

            onComplete?.Invoke();
        }
        
        private void CreateTransition(bool invertTransition, bool autoDestroy = true, Action onComplete = null)
        {
            TransitionProfile transitionProfileInstance = UnityEngine.Object.Instantiate(_transitionProfile);
            transitionProfileInstance.invert = invertTransition;

            TransitionAnimator transitionAnimator = TransitionAnimator.Start(transitionProfileInstance, autoDestroy);

            transitionAnimator.onTransitionEnd.AddListener(() => UnityEngine.Object.Destroy(transitionProfileInstance));

            if (onComplete != null)
                transitionAnimator.onTransitionEnd.AddListener(() => onComplete());
        }
    }
}