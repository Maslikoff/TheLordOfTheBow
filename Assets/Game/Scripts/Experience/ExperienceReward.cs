using Game.Scripts.Characters;
using UnityEngine;

namespace Game.Scripts.Experience
{
    public class ExperienceReward : MonoBehaviour
    {
        [SerializeField] private float _experienceReward = 50f;
        [SerializeField] private bool _rewardOnDeath = true;

        private Health _health;
        
        private bool _rewardGiven = false;

        private void OnEnable()
        {
            if (_health != null && _rewardOnDeath)
                _health.Death += OnEnemyDeath;
        }

        private void Awake()
        {
            _health = GetComponent<Health>();
        }

        private void OnDisable()
        {
            if (_health != null && _rewardOnDeath)
                _health.Death -= OnEnemyDeath;
        }

        public void GiveRewardManually()
        {
            OnEnemyDeath();
        }

        public void SetExperienceReward(float newReward)
        {
            _experienceReward = newReward;
        }

        private void OnEnemyDeath()
        {
            if (_rewardGiven) return;

            var playerExp = FindObjectOfType<Experience>();

            if (playerExp != null)
            {
                playerExp.AddExperience(_experienceReward);
                _rewardGiven = true;

                OnExperienceRewarded();
            }
        }

        private void OnExperienceRewarded()
        {
            // Можно добавить визуальный эффект (всплывающий текст, частицы и т.д.)
        }
    }
}