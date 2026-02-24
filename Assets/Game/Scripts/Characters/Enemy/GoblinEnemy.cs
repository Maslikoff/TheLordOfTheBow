using Game.Scripts.Characters.Bullets;
using UnityEngine;

namespace Game.Scripts.Characters.Enemy
{
    public class GoblinEnemy : Enemy
    {
        protected override void Attack()
        {
            Debug.Log($"Гоблин стреляет из лука! Урон: {_damage}");
            Bullet projectile = Instantiate(_bulletPrefab, _shootPoint.position, _shootPoint.rotation);
        }
    }
}