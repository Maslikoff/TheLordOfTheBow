using System;
using UnityEngine;

namespace Game.Scripts.Characters.Enemy
{
    [Serializable]
    public struct EnemyRaceConfig
    {
        [SerializeField] private Race _race;
        [SerializeField] private Enemy _prefab;
        [SerializeField] private int _poolSize;
        [SerializeField] [Range(0, 100)] private float _spawnWeight;

        public Race Race => _race;
        public Enemy Prefab => _prefab;
        public int PoolSize => _poolSize;
        public float SpawnWeight => _spawnWeight;
    }
}