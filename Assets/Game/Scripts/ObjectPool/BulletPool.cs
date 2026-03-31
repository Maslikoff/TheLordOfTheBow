using System.Collections.Generic;
using Game.Scripts.Characters.Bullets;
using UnityEngine;

namespace Game.Scripts.ObjectPool
{
    public class BulletPool : ObjectPool<Bullet>
    {
        [SerializeField] private List<BulletTypeConfig> _bulletConfigs = new();

        private Dictionary<BulletType, Queue<Bullet>> _typePools = new();
        private Dictionary<BulletType, Transform> _typeParents = new();
        private Dictionary<BulletType, Bullet> _typePrefabs = new();

        protected override void Awake()
        {
            if (_isInitialized == false)
                InitializePools();
        }

        protected override void OnDestroy()
        {
            foreach (var pool in _typePools)
            {
                foreach (var bullet in pool.Value)
                {
                    if(bullet != null)
                        bullet.Released -= OnHandleBulletReleased;
                }
            }
        }
        
        protected override Bullet CreateNewObject()
        {
            Debug.LogError("BulletPool should use GetBullet with type parameter!");
            return null;
        }
        
        public void ReturnBullet(Bullet bullet)
        {
            if (bullet == null) return;

            foreach (var pair in _typePrefabs)
            {
                if (bullet.GetType() == pair.Value.GetType())
                {
                    ReturnBulletToTypePool(bullet, pair.Key);
                    break;
                }
            }
        }

        public Bullet GetBullet(BulletType type)
        {
            if (_isInitialized == false)
                InitializePools();

            if (_typePools.ContainsKey(type) == false)
            {
                Debug.LogError($"No pool for bullet type {type}!");
                return null;
            }

            Bullet bullet;
            
            if (_typePools[type].Count > 0)
                bullet = _typePools[type].Dequeue();
            else
                bullet = CreateBulletForType(type);

            if (bullet != null)
            {
                bullet.gameObject.SetActive(true);
                
                OnObjectGet(bullet);
            }

            return bullet;
        }
        
        private void InitializePools()
        {
            foreach (var config in _bulletConfigs)
            {
                if (config.Prefab == null)
                {
                    Debug.LogError($"Prefab for bullet type {config.Type} is null!");
                    continue;
                }

                Transform parent = new GameObject($"{config.Type}Pool").transform;
                parent.SetParent(transform);
                
                _typeParents[config.Type] = parent;
                _typePrefabs[config.Type] = config.Prefab;
                
                Queue<Bullet> pool = new Queue<Bullet>();
                _typePools[config.Type] = pool;
                
                for (int i = 0; i < config.PoolSize; i++)
                {
                    Bullet bullet = CreateBulletForType(config.Type);
                    ReturnBulletToTypePool(bullet, config.Type);
                }
            }
            
            _isInitialized = true;
        }
        
        private Bullet CreateBulletForType(BulletType type)
        {
            if (_typePrefabs.ContainsKey(type) == false)
            {
                Debug.LogError($"No prefab for bullet type {type}!");
                return null;
            }

            Bullet bullet = Instantiate(_typePrefabs[type], _typeParents[type]);
            bullet.gameObject.SetActive(false);
            
            bullet.Released += OnHandleBulletReleased;
            
            return bullet;
        }
        
        private void ReturnBulletToTypePool(Bullet bullet, BulletType type)
        {
            if (bullet == null) 
                return;

            bullet.gameObject.SetActive(false);
            bullet.transform.SetParent(_typeParents[type]);
            bullet.transform.localPosition = Vector3.zero;
            
            _typePools[type].Enqueue(bullet);
        }
        
        private void OnHandleBulletReleased(IPoolable poolable)
        {
            if (poolable is Bullet bullet)
                ReturnBullet(bullet);
        }
    }
}