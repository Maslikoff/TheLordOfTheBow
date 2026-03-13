using System;
using System.Collections.Generic;
using Game.Scripts.Environment;
using UnityEngine;

namespace Game.Scripts.ObjectPool
{
    public class GroundPool : ObjectPool<Ground>
    {
        [SerializeField] private List<GroundPrefabConfig> _groundPrefabs = new();
        [SerializeField] private float _groundLength = 20f;

        private List<Queue<Ground>> _typePools = new();
        private List<Transform> _typeParents = new();
        private List<Ground> _typePrefabs = new();
        
        private int _currentPrefabIndex = 0;
        private float _lastSpawnZ = 0f;

        public float GroundLength => _groundLength;
        
        public Action GroundReturned;
        
        protected override void Awake()
        {
            base.Awake();
            
            InitializePools();
        }
        
        protected override void OnHandleObjectReleased(IPoolable poolable)
        {
            if (poolable is Ground ground)
                ReturnGround(ground); 
        }
        
        protected override Ground CreateNewObject()
        {
            Debug.LogError("GroundPool should use GetFromPool or GetNextGround!");
            return null;
        }
        
        public void ReturnGround(Ground ground)
        {
            if (ground == null) 
                return;

            for (int i = 0; i < _typePrefabs.Count; i++)
            {
                if (ground.GetType() == _typePrefabs[i].GetType())
                {
                    ReturnGroundToPool(ground, i);
                    GroundReturned?.Invoke();
                    break;
                }
            }
        }
        
        public override Ground GetFromPool()
        {
            if (_typePools.Count == 0)
            {
                Debug.LogError("No ground prefabs configured!");
                return null;
            }

            Ground ground;
            
            if (_typePools[_currentPrefabIndex].Count > 0)
                ground = _typePools[_currentPrefabIndex].Dequeue();
            else
                ground = CreateGroundForIndex(_currentPrefabIndex);

            if (ground != null)
                OnObjectGet(ground);

            return ground;
        }
        
        public Ground GetNextGround()
        {
            Ground ground = GetFromPool();
            _currentPrefabIndex = (_currentPrefabIndex + 1) % _typePrefabs.Count;
            
            return ground;
        }

        private void InitializePools()
        {
            Transform rootParent = new GameObject("GroundPool").transform;
            rootParent.SetParent(transform);

            foreach (var config in _groundPrefabs)
            {
                if (config.GroundPrefab == null)
                {
                    Debug.LogError("Ground prefab is null!");
                    continue;
                }
                
                _typeParents.Add(rootParent);
                _typePrefabs.Add(config.GroundPrefab);
                
                Queue<Ground> pool = new Queue<Ground>();
                _typePools.Add(pool);
                
                for (int i = 0; i < config.PoolSize; i++)
                {
                    Ground ground = CreateGroundForIndex(_typePrefabs.Count - 1);
                    ReturnGroundToPool(ground, _typePrefabs.Count - 1);
                }
            }
            
            _isInitialized = true;
        }
        
        private Ground CreateGroundForIndex(int index)
        {
            if (index < 0 || index >= _typePrefabs.Count)
            {
                Debug.LogError($"Invalid ground index: {index}");
                return null;
            }

            Ground ground = Instantiate(_typePrefabs[index], _typeParents[index]);
            ground.gameObject.SetActive(false);
            
            ground.Released += OnHandleObjectReleased; 
            
            return ground;
        }

        private void ReturnGroundToPool(Ground ground, int index)
        {
            if (ground == null) 
                return;

            ground.gameObject.SetActive(false);
            ground.transform.SetParent(_typeParents[index]);
            ground.transform.localPosition = Vector3.zero;
            ground.transform.localRotation = Quaternion.identity;
            
            _typePools[index].Enqueue(ground);
        }
    }
}