using System;
using System.IO;
using Game.Scripts.Environment;
using UnityEngine;

namespace Game.Scripts.Characters.Bullets
{
    public class PoisonArrow : Bullet
    {
        private const float MinBounceSpeedSqr = 0.01f;
        private const float BounceCooldown = 0.05f;

        [SerializeField] private float _defaultLifeTime = 10f;

        private float _maxLifeTime;
        private float _currentLifeTime;
        private Vector3 _currentVelocity;
        private int _lastBounceColliderId;
        private float _nextBounceTime;

        private void OnEnable()
        {
            _currentLifeTime = 0f;
            _currentVelocity = Vector3.zero;
            _lastBounceColliderId = 0;
            _nextBounceTime = 0f;

            _destroyOnCollision = false;
        }

        protected override void FixedUpdate()
        {
            _currentLifeTime += Time.fixedDeltaTime;

            if (_currentLifeTime >= _maxLifeTime)
            {
                Release();
                return;
            }

            MoveBullet();
        }

        public override void Initialize(BulletData bulletData)
        {
            base.Initialize(bulletData);

            _maxLifeTime = bulletData.LifeTime > 0 ? bulletData.LifeTime : _defaultLifeTime;
        }

        protected override void MoveBullet()
        {
            if (_currentVelocity == Vector3.zero)
            {
                Vector3 startDirection = _direction != Vector3.zero ? _direction : transform.forward;
                startDirection.y = 0;

                if (startDirection.sqrMagnitude <= MinBounceSpeedSqr)
                    startDirection = Vector3.forward;

                _currentVelocity = startDirection.normalized * _speed;
                AlignToMovement();
            }

            _rigidbody.velocity = _currentVelocity;
            _rigidbody.angularVelocity = Vector3.zero;
        }

        protected override void HandleCollision(Collision other)
        {
            if (CanBounceNow(other) == false)
                return;

            // #region agent log
            try
            {
                File.AppendAllText(Path.Combine(Application.dataPath, "..", "debug-239574.log"),
                    $"{{\"sessionId\":\"239574\",\"runId\":\"post-fix-v2\",\"hypothesisId\":\"F\",\"location\":\"PoisonArrow.cs:HandleCollision\",\"message\":\"bounce applied\",\"data\":{{\"hit\":\"{other.gameObject.name}\",\"vel\":\"{_currentVelocity.x:F2},{_currentVelocity.z:F2}\"}},\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n");
            }
            catch { }
            // #endregion

            if (other.gameObject.TryGetComponent(out Enemy.Enemy enemy))
                if (enemy.TryGetComponent(out Health health))
                    health.TakeDamage(_currentDamage);

            BounceHorizontally(other);
        }

        protected override bool CanCollide(Collision other)
        {
            if (IsOwnerCollision(other))
                return false;

            if (other.gameObject.TryGetComponent(out Player.Player _))
                return false;

            if (other.gameObject.TryGetComponent(out Enemy.Enemy _))
                return true;

            if (other.gameObject.TryGetComponent(out Wall _))
                return true;

            if (other.gameObject.GetComponentInParent<Wall>() != null)
                return true;

            return other.collider != null && other.collider.isTrigger == false;
        }

        public override void Release()
        {
            _currentLifeTime = 0f;
            _currentVelocity = Vector3.zero;
            _lastBounceColliderId = 0;
            _nextBounceTime = 0f;

            base.Release();
        }

        private bool CanBounceNow(Collision other)
        {
            if (other.contactCount == 0)
                return false;

            int colliderId = other.collider.GetInstanceID();

            if (colliderId == _lastBounceColliderId && Time.time < _nextBounceTime)
                return false;

            _lastBounceColliderId = colliderId;
            _nextBounceTime = Time.time + BounceCooldown;
            return true;
        }

        private void BounceHorizontally(Collision other)
        {
            ContactPoint contact = other.contacts[0];
            Vector3 normal = contact.normal;

            Vector3 horizontalVelocity = new Vector3(_currentVelocity.x, 0, _currentVelocity.z);
            Vector3 reflectedHorizontal = ResolveReflection(horizontalVelocity, normal);

            if (reflectedHorizontal.sqrMagnitude <= MinBounceSpeedSqr)
            {
                reflectedHorizontal = horizontalVelocity.sqrMagnitude > MinBounceSpeedSqr
                    ? -horizontalVelocity
                    : Vector3.forward * _speed;
            }

            _currentVelocity = reflectedHorizontal.normalized * _speed;
            AlignToMovement();
        }

        private static Vector3 ResolveReflection(Vector3 horizontalVelocity, Vector3 normal)
        {
            Vector3 flatNormal = new Vector3(normal.x, 0f, normal.z);

            if (flatNormal.sqrMagnitude <= MinBounceSpeedSqr)
                return new Vector3(horizontalVelocity.x, 0f, -horizontalVelocity.z);

            return Vector3.Reflect(horizontalVelocity, flatNormal.normalized);
        }

        private void AlignToMovement()
        {
            Vector3 flatDirection = new Vector3(_currentVelocity.x, 0, _currentVelocity.z);

            if (flatDirection.sqrMagnitude <= MinBounceSpeedSqr)
                return;

            transform.rotation = Quaternion.LookRotation(flatDirection.normalized, Vector3.up);
        }
    }
}
