using System.Collections;
using UnityEngine;
using Game.Scripts.ObjectPool;

namespace Game.Scripts.Spawners
{
    public abstract class Spawner<T> : MonoBehaviour where T : Component, IPoolable
    {
        [SerializeField] protected float _spawnInterval;
        [SerializeField] protected int _maxObjects;
        [SerializeField] protected Vector3 _spawnArea;

        protected ObjectPool<T> _objectPool;
        protected int _currentObjectsCount;
        
        private Coroutine _spawnCoroutine;

        protected virtual void OnEnable()
        {
            StartSpawning();
        }

        protected virtual void Start()
        {
            Initialize();
        }

        protected virtual void OnDisable()
        {
            StopSpawning();
        }

        protected virtual void Initialize()
        {
            if (_objectPool == null)
                _objectPool = GetComponent<ObjectPool<T>>();
        }

        private IEnumerator SpawnRoutine()
        {
            WaitForSeconds waitForSeconds = new WaitForSeconds(_spawnInterval);

            while (enabled)
            {
                yield return waitForSeconds;

                if (CanSpawn())
                    SpawnObject();
            }
        }

        protected virtual bool CanSpawn() => _currentObjectsCount < _maxObjects;
        
        protected virtual void StartSpawning()
        {
            if (_spawnCoroutine != null)
                StopCoroutine(_spawnCoroutine);

            _spawnCoroutine = StartCoroutine(SpawnRoutine());
        }

        protected virtual void StopSpawning()
        {
            if (_spawnCoroutine != null)
            {
                StopCoroutine(_spawnCoroutine);
                _spawnCoroutine = null;
            }
        }

        protected abstract void SpawnObject();

        protected Vector3 GetRandomSpawnPosition()
        {
            Vector3 randomPoint = new Vector3(Random.Range(-_spawnArea.x, _spawnArea.x), 0f,
                Random.Range(-_spawnArea.z, _spawnArea.z));

            return transform.position + randomPoint;
        }

        private IEnumerator DelayedSpawnCheck()
        {
            yield return new WaitForSeconds(_spawnInterval);

            if (CanSpawn())
                StartSpawning();
        }

        public void OnObjectTaken()
        {
            _currentObjectsCount--;

            if (CanSpawn())
                StartCoroutine(DelayedSpawnCheck());
        }

        protected void IncreaseObjectCount()
        {
            _currentObjectsCount++;
        }

        protected void DecreaseObjectCount()
        {
            _currentObjectsCount = Mathf.Max(0, _currentObjectsCount - 1);
        }

        public void SetMaxObjects(int maxObjects)
        {
            _maxObjects = maxObjects;
        }

        public void SetSpawnInterval(float interval)
        {
            _spawnInterval = interval;
        }
    }
}