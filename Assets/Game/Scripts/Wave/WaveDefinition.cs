using System;
using System.Collections.Generic;
using Game.Scripts.Characters.Enemy;
using UnityEngine;

namespace Game.Scripts.Wave
{
    [Serializable]
    public class WaveDefinition
    {
        [Min(0)] public int Goblins;
        [Min(0)] public int Orcs;
        [Min(0)] public int Trolls;
        [Min(1)] public int MaxAlive = 4;
        [Min(0.05f)] public float SpawnInterval = 1f;
        [Range(0, 1)] public float HealAfter;
        public bool TrollLast;
        public int Total => Goblins + Orcs + Trolls;
        public bool IsValid => Goblins >= 0 && Orcs >= 0 && Trolls >= 0 && Total > 0 && MaxAlive > 0 && SpawnInterval > 0;

        public List<Race> CreateSpawnOrder()
        {
            var result = new List<Race>(Total);
            Add(result, Race.Goblin, Goblins);
            Add(result, Race.Orc, Orcs);
            if (!TrollLast) Add(result, Race.Troll, Trolls);
            for (int i = result.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (result[i], result[j]) = (result[j], result[i]);
            }
            if (TrollLast) Add(result, Race.Troll, Trolls);
            return result;
        }

        private static void Add(List<Race> list, Race race, int count)
        {
            for (int i = 0; i < count; i++) list.Add(race);
        }
    }
}
