using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts.Wave
{
    [Serializable]
    public class WaveConfig
    {
        [SerializeField] private List<WaveDefinition> _waves = new();
        [SerializeField] private float _timeBetweemWaves = 3f;

        public IReadOnlyList<WaveDefinition> Waves => _waves;
        public float TimeBetweenWaves => _timeBetweemWaves;
    }
}
