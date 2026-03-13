using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts.Wave
{
    [Serializable]
    public class WaveConfig
    {
        [SerializeField] private List<int> _wavesEnemyCount = new List<int>();
        [SerializeField] private float _timeBetweemWaves = 3f;

        public List<int> WavesEnemyCount => _wavesEnemyCount;
        public float TimeBetweenWaves => _timeBetweemWaves;
    }
}