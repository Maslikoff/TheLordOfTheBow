using Game.Scripts.Spawners;
using Game.Scripts.Upgrades;
using UnityEngine;
using VContainer;

namespace Game.Scripts.Characters.Player
{
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(InputHandler))]
    public class Player : MonoBehaviour
    {
        [SerializeField] private PlayerMovement movement;
        [SerializeField] private InputHandler _inputHandler;
        [SerializeField] private PlayerShoot _playerShoot;
        [SerializeField] private PlayerBulletUpgradeCollection _bulletUpgrades;
        [SerializeField] private Experience.Experience _experience;
        [SerializeField] private UpgradeApplier _upgradeApplier;

        public PlayerShoot PlayerShoot => _playerShoot;
        public PlayerBulletUpgradeCollection BulletUpgrades => _bulletUpgrades;
        public Experience.Experience Experience => _experience;
        public UpgradeApplier UpgradeApplier => _upgradeApplier;
        
        [Inject]
        private void Construct(DynamicJoystick joystick, BulletSpawner bulletSpawner)
        {
            _inputHandler.SetJoystick(joystick);
            _playerShoot.Initialize(bulletSpawner);
        }

        private void OnEnable()
        {
            _inputHandler.MoveInput += movement.Move;
        }

        private void OnDisable()
        {
            _inputHandler.MoveInput -= movement.Move;
        }
    }
}