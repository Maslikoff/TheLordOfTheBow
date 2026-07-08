using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Game.Scripts.UI
{
    public static class UIPanelAnimator
    {
        public static UniTask Show(
            GameObject panel,
            CanvasGroup canvasGroup,
            float fadeDuration,
            float scaleDuration,
            Ease ease)
        {
            panel.SetActive(true);
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            panel.transform.localScale = Vector3.one * 0.8f;

            return DOTween.Sequence()
                .SetUpdate(true)
                .Join(canvasGroup.DOFade(1f, fadeDuration).SetUpdate(true))
                .Join(panel.transform.DOScale(1f, scaleDuration).SetEase(ease).SetUpdate(true))
                .OnComplete(() =>
                {
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                })
                .AsyncWaitForCompletion();
        }

        public static Tween Hide(
            GameObject panel,
            CanvasGroup canvasGroup,
            float fadeDuration,
            float scaleDuration)
        {
            return DOTween.Sequence()
                .SetUpdate(true)
                .Join(canvasGroup.DOFade(0f, fadeDuration).SetUpdate(true))
                .Join(panel.transform.DOScale(0.8f, scaleDuration).SetEase(Ease.InBack).SetUpdate(true))
                .OnComplete(() => panel.SetActive(false));
        }
    }
}