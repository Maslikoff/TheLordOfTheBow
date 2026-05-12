using System;
using Game.Scripts.Spawners;
using Game.Scripts.Upgrades;
using UnityEngine;
using VContainer;

namespace Game.Scripts.Characters.Player
{
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(InputHandler))]
    [RequireComponent(typeof(Health))]
    public class Player : MonoBehaviour
    {
        [SerializeField] private PlayerMovement movement;
        [SerializeField] private InputHandler _inputHandler;
        [SerializeField] private PlayerShoot _playerShoot;
        [SerializeField] private PlayerBulletUpgradeCollection _bulletUpgrades;
        [SerializeField] private Experience.Experience _experience;
        [SerializeField] private UpgradeApplier _upgradeApplier;
        [SerializeField] private Health _playerHealth;

        public PlayerShoot PlayerShoot => _playerShoot;
        public PlayerBulletUpgradeCollection BulletUpgrades => _bulletUpgrades;
        public Experience.Experience Experience => _experience;
        public UpgradeApplier UpgradeApplier => _upgradeApplier;
        public Health PlayerHealth => _playerHealth;
        public bool IsDead { get; private set; }
        
        [Inject]
        private void Construct(DynamicJoystick joystick, BulletSpawner bulletSpawner)
        {
            _inputHandler.SetJoystick(joystick);
            _playerShoot.Initialize(bulletSpawner);
        }

        private void OnValidate()
        {
            _playerHealth ??= GetComponent<Health>();
        }

        private void OnEnable()
        {
            _inputHandler.MoveInput += movement.Move;
            _playerHealth.Death += OnDeath;
        }

        private void OnDisable()
        {
            _inputHandler.MoveInput -= movement.Move;
            _playerHealth.Death -= OnDeath;
        }

        private void OnDeath()
        {
            IsDead = true;
        }
    }
}