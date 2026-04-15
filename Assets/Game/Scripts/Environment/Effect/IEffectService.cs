using UnityEngine;

namespace Game.Scripts.Environment.Effect
{
    public interface IEffectService
    {
        void PlayEffect(EffectType type, Vector3 position);
    }
}