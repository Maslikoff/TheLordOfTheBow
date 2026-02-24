using Game.Scripts.Characters.Bullets;
using UnityEngine;

namespace Game.Scripts.Characters.Enemy
{
    public class TrollEnemy : Enemy
    {
        protected override void Attack()
        {
            Debug.Log($"Тролль кидает камень! Урон: {_damage}");
            Bullet projectile = Instantiate(_bulletPrefab, _shootPoint.position, _shootPoint.rotation);
        }
    }
}