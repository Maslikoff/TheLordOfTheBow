using Game.Scripts.Characters.Bullets;
using UnityEngine;

namespace Game.Scripts.Characters.Enemy
{
    public class OrcEnemy: Enemy

    {
        protected override void Attack()
        {
            Debug.Log($"Орг стреляет из арболета! Урон: {_damage}");
            Bullet projectile = Instantiate(_bulletPrefab, _shootPoint.position, _shootPoint.rotation);
        }
    }
}