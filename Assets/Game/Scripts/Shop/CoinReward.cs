using Game.Scripts.Characters;
using Game.Scripts.Characters.Enemy;
using UnityEngine;
using VContainer;

namespace Game.Scripts.Shop
{
    public class CoinReward : MonoBehaviour
    {
        [SerializeField] private bool _rewardOnDeath = true;

        private Health _health;
        private Enemy _enemy;
        private ICurrencyService _currency;
        private bool _rewardGiven;

        [Inject]
        private void Construct(ICurrencyService currency)
        {
            _currency = currency;
        }

        private void Awake()
        {
            _health = GetComponent<Health>();
            _enemy = GetComponent<Enemy>();
        }

        private void OnEnable()
        {
            _rewardGiven = false;

            if (_health != null && _rewardOnDeath)
                _health.Death += OnEnemyDeath;
        }

        private void OnDisable()
        {
            if (_health != null && _rewardOnDeath)
                _health.Death -= OnEnemyDeath;
        }

        private void OnEnemyDeath()
        {
            if (_rewardGiven || _currency == null)
                return;

            int amount = RollCoinsForRace(_enemy != null ? _enemy.RaceEnemy : Race.Goblin);
            _currency.Add(amount);
            _rewardGiven = true;
            Debug.Log($"[Coins] +{amount} from {_enemy?.RaceEnemy} (total {_currency.Coins})");
        }

        private static int RollCoinsForRace(Race race)
        {
            return race switch
            {
                Race.Goblin => UnityEngine.Random.Range(1, 4),
                Race.Orc => UnityEngine.Random.Range(2, 5),
                Race.Troll => UnityEngine.Random.Range(3, 6),
                _ => UnityEngine.Random.Range(1, 4)
            };
        }
    }
}
