using System;
using Game.Scripts.Environment;
using Game.Scripts.ObjectPool;
using UnityEngine;

namespace Game.Scripts.Spawners
{
    public class GroundSpawner : Spawner<Ground>
    {
        [SerializeField] private GroundPool _groundPool;
        [SerializeField] private int _initialSegmentCount = 6;
        [SerializeField] private Vector3 _startPosition = Vector3.zero;

        protected override bool CanSpawn() => true;
        
        protected override void OnEnable()
        {
        }

        protected override void OnDisable()
        {
            if (_groundPool != null)
                _groundPool.GroundReturned -= SpawnObject;
        }

        protected override void Initialize()
        {
            if (_groundPool == null) _groundPool = _objectPool as GroundPool;
            if (_groundPool == null) throw new NullReferenceException("Can't get for ground pool");

            _groundPool.GroundReturned += SpawnObject;

            for (int i = 0; i < _initialSegmentCount; i++)
                SpawnGroundAtIndexedPosition(i);
        }

        protected override void SpawnObject() => SpawnGroundAtIndexedPosition(_initialSegmentCount - 1);

        private void SpawnGroundAtIndexedPosition(int index)
        {
            if (_groundPool == null) return;

            Ground newGround = _groundPool.GetNextGround();

            if (newGround == null) return;

            Vector3 newGroundPosition = new Vector3(_startPosition.x, _startPosition.y, _groundPool.GroundLength * index);
            newGround.transform.position = newGroundPosition;
            newGround.gameObject.SetActive(true);
            
            IncreaseObjectCount();
        }
    }
}