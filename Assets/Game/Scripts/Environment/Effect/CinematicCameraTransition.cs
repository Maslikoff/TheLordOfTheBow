using System;
using DG.Tweening;
using UnityEngine;

namespace Game.Scripts.Environment.Effect
{
    public class CinematicCameraTransition : MonoBehaviour
    {
        [Header("Crane")]
        [SerializeField] private float _duration = 1.65f;
        [SerializeField] private float _heightBoost = 5f;
        [SerializeField] private float _groundY;
        [SerializeField] private float _targetFov = 70f;
        [SerializeField] private Ease _ease = Ease.InOutCubic;

        private Tween _tween;

        public void Play(Camera fromCamera, Camera toCamera, Action onComplete)
        {
            _tween?.Kill();

            if (toCamera == null)
            {
                onComplete?.Invoke();
                return;
            }

            Transform toTransform = toCamera.transform;
            Vector3 endPosition = toTransform.position;
            Quaternion endRotation = toTransform.rotation;

            Vector3 startPosition = endPosition;
            Quaternion startRotation = endRotation;
            float startFov = toCamera.fieldOfView;

            if (fromCamera != null)
            {
                Transform fromTransform = fromCamera.transform;
                startPosition = fromTransform.position;
                startRotation = fromTransform.rotation;
                startFov = fromCamera.fieldOfView;

                toTransform.SetPositionAndRotation(startPosition, startRotation);
                toCamera.fieldOfView = startFov;
                toCamera.gameObject.SetActive(true);
                fromCamera.gameObject.SetActive(false);
            }
            else
            {
                toCamera.gameObject.SetActive(true);
            }

            Vector3 startLook = GetGroundLookPoint(startPosition, startRotation);
            Vector3 endLook = GetGroundLookPoint(endPosition, endRotation);

            Vector3 control = Vector3.Lerp(startPosition, endPosition, 0.5f);
            control.y = Mathf.Max(startPosition.y, endPosition.y) + _heightBoost;

            _tween = DOVirtual.Float(0f, 1f, _duration, t =>
            {
                Vector3 position = EvaluateQuadraticBezier(startPosition, control, endPosition, t);
                Vector3 lookPoint = Vector3.Lerp(startLook, endLook, t);
                Vector3 toLook = lookPoint - position;

                toTransform.position = position;
                toTransform.rotation = toLook.sqrMagnitude > 0.0001f
                    ? Quaternion.LookRotation(toLook, Vector3.up)
                    : endRotation;
                toCamera.fieldOfView = Mathf.Lerp(startFov, _targetFov, t);
            })
            .SetEase(_ease)
            .SetLink(gameObject)
            .OnComplete(() =>
            {
                toTransform.SetPositionAndRotation(endPosition, endRotation);
                toCamera.fieldOfView = _targetFov;
                onComplete?.Invoke();
            });
        }

        private void OnDestroy()
        {
            _tween?.Kill();
        }

        private Vector3 GetGroundLookPoint(Vector3 position, Quaternion rotation)
        {
            Vector3 forward = rotation * Vector3.forward;
            Plane ground = new Plane(Vector3.up, new Vector3(0f, _groundY, 0f));
            Ray ray = new Ray(position, forward);

            if (ground.Raycast(ray, out float enter) && enter > 0.01f)
                return ray.GetPoint(enter);

            Vector3 fallback = position + forward * 10f;
            fallback.y = _groundY;
            return fallback;
        }

        private static Vector3 EvaluateQuadraticBezier(Vector3 start, Vector3 control, Vector3 end, float t)
        {
            float inverse = 1f - t;
            return inverse * inverse * start + 2f * inverse * t * control + t * t * end;
        }
    }
}
