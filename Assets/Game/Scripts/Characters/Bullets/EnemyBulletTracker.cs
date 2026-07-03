using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts.Characters.Bullets
{
    public class EnemyBulletTracker
    {
        private static readonly Dictionary<int, HashSet<EnemyBullet>> ActiveByOwnerId = new();
        
        public static void Register(Transform owner, EnemyBullet bullet)
        {
            if (owner == null || bullet == null)
                return;
            
            int id = owner.GetInstanceID();
            
            if (ActiveByOwnerId.ContainsKey(id) == false)
                ActiveByOwnerId[id] = new HashSet<EnemyBullet>();
            
            ActiveByOwnerId[id].Add(bullet);
        }
        public static void Unregister(Transform owner, EnemyBullet bullet)
        {
            if (owner == null || bullet == null)
                return;
            
            int id = owner.GetInstanceID();
            
            if (ActiveByOwnerId.TryGetValue(id, out HashSet<EnemyBullet> set))
            {
                set.Remove(bullet);
                
                if (set.Count == 0)
                    ActiveByOwnerId.Remove(id);
            }
        }
        public static void ReleaseAllForOwner(Transform owner)
        {
            if (owner == null)
                return;
            
            int id = owner.GetInstanceID();
            
            if (ActiveByOwnerId.TryGetValue(id, out HashSet<EnemyBullet> set) == false)
                return;
            
            var snapshot = new List<EnemyBullet>(set);
            
            foreach (EnemyBullet bullet in snapshot)
            {
                if (bullet != null && bullet.gameObject.activeInHierarchy)
                    bullet.Release();
            }
            
            ActiveByOwnerId.Remove(id);
        }
    }
}