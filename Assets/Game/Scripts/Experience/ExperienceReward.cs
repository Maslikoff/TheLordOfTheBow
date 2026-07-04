using Game.Scripts.Characters;
using UnityEngine;

namespace Game.Scripts.Experience
{
    public class ExperienceReward : MonoBehaviour
    {
        [SerializeField] private float _experienceReward = 50f;
        [SerializeField] private bool _rewardOnDeath = true;

        private Health _health;
        
        private Experience _playerExperience;
        private bool _rewardGiven = false;

        private void OnEnable()
        {
            _rewardGiven = false;
            
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

        public void Initialize(Experience playerExperience)
        {
            _playerExperience = playerExperience;
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
            if (_rewardGiven) 
                return;

            if (_playerExperience != null)
            {
                _playerExperience.AddExperience(_experienceReward);
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