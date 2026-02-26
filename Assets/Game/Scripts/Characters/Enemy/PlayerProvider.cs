using UnityEngine;
using Zenject;

namespace Game.Scripts.Characters.Enemy
{
    public class PlayerProvider : MonoBehaviour, IPlayerProvider
    {
        [SerializeField] private Player.Player _playerGameObject;

        private Transform _playerTransform;

        public Transform GetPlayerTransform() => _playerTransform;

        public Vector3 GetPlayerPosition() => _playerTransform != null ? _playerTransform.position : Vector3.zero;

        public Player.Player GetPlayerGameObject() => _playerGameObject;

        private void Awake()
        {
            _playerTransform = _playerGameObject.transform;
        }

        [Inject]
        public void Construct([Inject(Optional = true)] Player.Player playerGameObject)
        {
            if (playerGameObject != null)
            {
                _playerGameObject = playerGameObject;
                _playerTransform = playerGameObject.transform;
            }
        }
    }
}