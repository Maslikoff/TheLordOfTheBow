using Game.Scripts.Characters.Enemy;
using Game.Scripts.Spawners;
using UnityEngine;

namespace Game.Scripts.ObjectPool
{
    public class EnemyPool : ObjectPool<Enemy>
    {
        [SerializeField] private Transform _playerTarget;
        [SerializeField] private BulletSpawner _bulletSpawner;

        protected override Enemy CreateNewObject()
        {
            Enemy enemy = base.CreateNewObject();
            
            if (enemy != null && _playerTarget != null)
                enemy.Initialize(_playerTarget, _bulletSpawner);
                
            return enemy;
        }

        public override Enemy GetFromPool()
        {
            Enemy enemy = base.GetFromPool();
            
            if (enemy != null && _playerTarget != null)
                enemy.Initialize(_playerTarget, _bulletSpawner);
                
            return enemy;
        }

        public void SetPlayerTarget(Transform player)
        {
            _playerTarget = player;
        }
    }
}