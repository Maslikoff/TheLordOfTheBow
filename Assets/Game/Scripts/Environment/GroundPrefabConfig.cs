using System;
using UnityEngine;

namespace Game.Scripts.Environment
{
    [Serializable]
    public struct GroundPrefabConfig
    {
        [SerializeField] private Ground _groundPrefab;
        [SerializeField] private int _poolSize;
        
        public Ground GroundPrefab => _groundPrefab;
        public int  PoolSize => _poolSize;
    }
}