using System;
using Game.Scripts.Spawners;
using UnityEngine;
using VContainer;

namespace Game.Scripts.Environment.Effect
{
    [Serializable]
    public class EffectService : IEffectService
    {
        private readonly EffectSpawner _effectSpawner;

        [Inject]
        public EffectService(EffectSpawner effectSpawner)
        {
            _effectSpawner = effectSpawner;
        }
        
        public void PlayEffect(EffectType type, Vector3 position)
        {
            _effectSpawner?.SpawnEffect(type, position);
        }
    }
}