using System;
using UnityEngine;

namespace Game.Scripts.Characters.Health
{
    public class Health: MonoBehaviour

    {
        [SerializeField] private int _maxCount = 100;
        [SerializeField] private bool _isInvulnerable = false;

        private int _currentCount;

        public int MaxCount => _maxCount;
        public int CurrentCount => _currentCount;

        public event Action Death;
        public event Action<int> Changed;
        public event Action<int> DamageTaken;

        private void Start()
        {
            _currentCount = _maxCount;

            Changed?.Invoke(_currentCount);
        }

        public void TakeDamage(int damage)
        {
            if (_isInvulnerable || _currentCount <= 0)
                return;

            _currentCount -= damage;

            Changed?.Invoke(_currentCount);
            DamageTaken?.Invoke(damage);

            if (_currentCount <= 0)
                Die();
        }

        public void SetMaxHealth(int newMaxHealth)
        {
            _maxCount = newMaxHealth;

            if (_currentCount > _maxCount)
                _currentCount = _maxCount;

            Changed?.Invoke(_currentCount);
        }

        public void Heal(int healAmount)
        {
            _currentCount += healAmount;

            if (_currentCount > _maxCount)
                _currentCount = _maxCount;

            Changed?.Invoke(_currentCount);
        }

        private void Die()
        {
            Death?.Invoke();
        }
    }
}