using UnityEngine;

namespace Game.Scripts.Characters.Enemy
{
    public interface IPlayerProvider
    {
        Vector3 GetPlayerPosition();
        Player.Player GetPlayerGameObject();
    }
}