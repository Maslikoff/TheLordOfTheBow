using System.Collections.Generic;
using Assets.Game.Scripts.Characters.Player;
using Game.Scripts.Characters;
using Game.Scripts.Characters.Enemy;
using Game.Scripts.Spawners;
using UnityEngine;
using VContainer;

namespace Game.Scripts.ObjectPool
{
    public class EnemyPool : ObjectPool<Enemy>
    {
        [SerializeField] private List<EnemyRaceConfig> _enemyConfigs = new();
        [SerializeField] private BulletSpawner _bulletSpawner;

        private Dictionary<Race, Queue<Enemy>> _racePools = new();
        private Dictionary<Race, Transform> _raceParents = new();
        private Dictionary<Race, Enemy> _racePrefabs = new();
        private Dictionary<Race, float> _raceWeights = new();

        private float _totalWeight;

        private ITransformHolder _playerTarget;

        [Inject] 
        public void Construct(ITransformHolder playerTarget)
        {
            _playerTarget = playerTarget; 
        }

        protected override void Awake()
        {
            base.Awake();

            InitializePools();
        }

        protected override void OnDestroy()
        {
            foreach (var pool in _racePools)
                foreach (var enemy in pool.Value)
                    if (enemy != null)
                        enemy.Released -= OnHandleEnemyReleased;
        }

        protected override Enemy CreateNewObject() => null;

        public void ReturnEnemy(Enemy enemy)
        {
            if (enemy == null) return;

            foreach (var pair in _racePrefabs)
            {
                if (enemy.GetType() == pair.Value.GetType())
                {
                    ReturnEnemyToRacePool(enemy, pair.Key);
                    break;
                }
            }
        }

        public Enemy GetEnemy(Race race)
        {
            if (_isInitialized == false)
                InitializePools();

            if (_racePools.ContainsKey(race) == false)
                return null;

            Enemy enemy;
            
            if (_racePools[race].Count > 0)
                enemy = _racePools[race].Dequeue();
            else
                enemy = CreateEnemyForRace(race);

            if (enemy != null)
            {
                enemy.gameObject.SetActive(true);
                
                if (_playerTarget != null)
                    enemy.Initialize(_playerTarget, _bulletSpawner);
                
                OnObjectGet(enemy);
            }

            return enemy;
        }

        public Enemy GetRandomEnemyByWeight()
        {
            if (_racePrefabs.Count == 0)
                return null;

            if (_totalWeight <= 0)
            {
                foreach (var race in _racePrefabs.Keys)
                    return GetEnemy(race);
            }

            float randomValue = Random.Range(0, _totalWeight);
            float currentSum = 0;

            foreach (var pair in _raceWeights)
            {
                currentSum += pair.Value;
                
                if (randomValue <= currentSum)
                    return GetEnemy(pair.Key);
            }

            foreach (var race in _racePrefabs.Keys)
                return GetEnemy(race);

            return null;
        }

        private void InitializePools()
        {
            if (_isInitialized) return;

            Transform rootParent = new GameObject("EnemyPool").transform;
            rootParent.SetParent(transform);

            _totalWeight = 0;
            _raceWeights.Clear();
            _racePrefabs.Clear();
            _racePools.Clear();
            _raceParents.Clear();

            foreach (var config in _enemyConfigs)
            {
                if (config.Prefab == null)
                    continue;

                Transform parent = new GameObject($"{config.Race}Pool").transform;
                parent.SetParent(rootParent);

                _raceParents[config.Race] = parent;
                _racePrefabs[config.Race] = config.Prefab;
                _raceWeights[config.Race] = config.SpawnWeight;
                _totalWeight += config.SpawnWeight;

                Queue<Enemy> pool = new Queue<Enemy>();
                _racePools[config.Race] = pool;

                for (int i = 0; i < config.PoolSize; i++)
                {
                    Enemy enemy = CreateEnemyForRace(config.Race);
                    ReturnEnemyToRacePool(enemy, config.Race);
                }
            }

            _isInitialized = true;
        }

        private Enemy CreateEnemyForRace(Race race)
        {
            if (_racePrefabs.ContainsKey(race) == false)
                return null;

            Enemy enemy = Instantiate(_racePrefabs[race], _raceParents[race]);
            enemy.gameObject.SetActive(false);

            enemy.Released += OnHandleEnemyReleased;

            return enemy;
        }

        private void ReturnEnemyToRacePool(Enemy enemy, Race race)
        {
            if (enemy == null)
                return;

            var rb = enemy.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.Sleep();
            }

            var health = enemy.GetComponent<Health>();

            if (health != null)
                health.ResetHealth();

            enemy.gameObject.SetActive(false);
            enemy.transform.SetParent(_raceParents[race]);
            enemy.transform.localPosition = Vector3.zero;
            enemy.transform.localRotation = Quaternion.identity;

            _racePools[race].Enqueue(enemy);
        }

        private void OnHandleEnemyReleased(IPoolable poolable)
        {
            if (poolable is Enemy enemy)
                ReturnEnemy(enemy);
        }
    }
}