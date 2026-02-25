using System;
using Game.Scripts.Characters.Enemy;
using UnityEngine;

namespace Game.Scripts.Wave
{
    [Serializable]
    public class WaveConfig
    {
        [Header("Wave Settings")] 
        [SerializeField] private string _waveName = "Wave";
        [SerializeField] private int _enemyCount = 5;

        [Header("Race Probabilities (Total should be 100%)")] 
        [SerializeField] [Range(0, 100)] private int _goblinChance = 33;
        [SerializeField] [Range(0, 100)] private int _orcChance = 33;
        [SerializeField] [Range(0, 100)] private int _trollChance = 34;

        [Header("Spawn Delay")] 
        [SerializeField] private float _spawnInterval = 0.5f;
        [SerializeField] private bool _spawnAllAtOnce;

        public string WaveName => _waveName;
        public int EnemyCount => _enemyCount;
        public float SpawnInterval => _spawnInterval;
        public bool SpawnAllAtOnce => _spawnAllAtOnce;

        public Race GetRandomRace()
        {
            int random = UnityEngine.Random.Range(0, 100);

            if (random < _goblinChance)
                return Race.Goblin;

            if (random < _goblinChance + _orcChance)
                return Race.Orc;

            return Race.Troll;
        }

        public void ValidateProbabilities()
        {
            int total = _goblinChance + _orcChance + _trollChance;
            
            if (total != 100)
                Debug.LogWarning($"Wave '{_waveName}' probabilities total is {total}%. Adjusting to 100%");
        }
    }
}