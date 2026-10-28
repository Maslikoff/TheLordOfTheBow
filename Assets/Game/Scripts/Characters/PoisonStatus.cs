using System.Collections;
using UnityEngine;

namespace Game.Scripts.Characters
{
    [RequireComponent(typeof(Health))]
    public class PoisonStatus : MonoBehaviour
    {
        private Health _health;
        private Coroutine _poisonRoutine;
        private float _damagePerSecond;
        private float _duration;

        private void Awake()
        {
            _health = GetComponent<Health>();
        }

        private void OnDisable()
        {
            Clear();
        }

        public void Apply(float damagePerSecond, float duration)
        {
            if (damagePerSecond <= 0f || duration <= 0f)
                return;

            _damagePerSecond = damagePerSecond;
            _duration = duration;

            if (_poisonRoutine != null)
                StopCoroutine(_poisonRoutine);

            _poisonRoutine = StartCoroutine(PoisonRoutine());
        }

        public void Clear()
        {
            if (_poisonRoutine != null)
            {
                StopCoroutine(_poisonRoutine);
                _poisonRoutine = null;
            }

            _damagePerSecond = 0f;
            _duration = 0f;
        }

        private IEnumerator PoisonRoutine()
        {
            float elapsed = 0f;

            while (elapsed < _duration)
            {
                yield return new WaitForSeconds(1f);
                elapsed += 1f;

                if (_health == null || _health.CurrentCount <= 0f)
                    break;

                _health.TakeDamage(_damagePerSecond);
            }

            _poisonRoutine = null;
        }
    }
}
