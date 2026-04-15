using System;
using UnityEngine;

namespace Game.Scripts.Environment.Effect
{
    [Serializable]
    public class EffectConfig
    {
        [field: SerializeField] public EffectType Type { get; private set; }
        [field: SerializeField] public GameEffect Prefab { get; private set; }
        [field: SerializeField] public int PoolSize { get; private set; } = 5;
    }
}