using System;
using Game.Scripts.Characters;
using Game.Scripts.Characters.Player;
using UnityEngine;

namespace Game.Scripts.Upgrades
{
    public class PlayerUpgradeHolder : MonoBehaviour
    {
        [SerializeField] private PlayerShoot _playerShoot;
        [SerializeField] private Health _playerHealth;
        [SerializeField] private PlayerBulletUpgradeCollection _collection;
        
        public PlayerShoot PlayerShoot => _playerShoot;
        public Health PlayerHealth=> _playerHealth;
        public PlayerBulletUpgradeCollection Collection => _collection;
        
    }
}