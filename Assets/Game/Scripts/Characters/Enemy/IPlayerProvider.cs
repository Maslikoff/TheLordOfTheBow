using UnityEngine;

namespace Game.Scripts.Characters.Enemy
{
    public interface IPlayerProvider
    {
        Transform GetPlayerTransform();
        Vector3 GetPlayerPosition();
        Player.Player GetPlayerGameObject();
    }
}