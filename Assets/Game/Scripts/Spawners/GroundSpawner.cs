using Game.Scripts.Environment;
using Game.Scripts.ObjectPool;
using UnityEngine;

namespace Game.Scripts.Spawners
{
    public class GroundSpawner : Spawner<Ground>
    {
        [SerializeField] private GroundPool _groundPool;
        [SerializeField] private int _initialSegmentCount = 3;
        [SerializeField] private Vector3 _startPosition = Vector3.zero;
        
        private float _nextSpawnZ;

        protected override void OnEnable() { }

        protected override void OnDisable()
        {
            if (_groundPool != null)
                _groundPool.GroundReturned -= SpawnObject;
        }

        protected override void Initialize()
        {
            base.Initialize();
            
            if (_groundPool == null)
                _groundPool = _objectPool as GroundPool;
            
            _groundPool.GroundReturned += SpawnObject;
            
            for (int i = 0; i < _initialSegmentCount; i++)
            {
                SpawnGroundSegment();
            }
        }

        protected override void SpawnObject()
        {
            SpawnGroundSegment();
        }

        protected override bool CanSpawn() => true;

        private void SpawnGroundSegment()
        {
            if (_groundPool == null) return;
            
            Ground newGround = _groundPool.GetNextGround();
            
            if (newGround != null)
            {
                newGround.transform.position = new Vector3(_startPosition.x, _startPosition.y, _nextSpawnZ);
                
                newGround.gameObject.SetActive(true);
                _nextSpawnZ += _groundPool.GroundLength;
                
                IncreaseObjectCount();
            }
        }
    }
}