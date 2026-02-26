using UnityEngine;
using Zenject;

namespace Game.Scripts.Characters.Enemy
{
    public class PlayerProvider : MonoBehaviour, IPlayerProvider
    {
        [SerializeField] private Player.Player _playerGameObject;

        private Vector3 _playerPosition;

        public Vector3 GetPlayerPosition() => _playerPosition != null ? _playerPosition : Vector3.zero;

        public Player.Player GetPlayerGameObject() => _playerGameObject;

        private void Awake()
        {
            _playerPosition = _playerGameObject.transform.position;
        }

        [Inject]
        public void Construct([Inject(Optional = true)] Player.Player playerGameObject)
        {
            if (playerGameObject != null)
            {
                _playerGameObject = playerGameObject;
                _playerPosition = playerGameObject.transform.position;
            }
        }
    }
}