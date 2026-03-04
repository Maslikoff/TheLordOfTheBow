using Game.Scripts.Characters.Bullets;
using UnityEngine;

namespace Game.Scripts.Characters.Enemy
{
    public class TrollEnemy : Enemy
    {
        protected void Awake()
        {
            _race = Race.Troll;
        }
    }
}