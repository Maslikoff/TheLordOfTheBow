using Game.Scripts.Characters.Bullets;
using UnityEngine;

namespace Game.Scripts.Characters.Enemy
{
    public class OrcEnemy: Enemy

    {
        protected void Awake()
        {
            _race = Race.Orc;
        }
    }
}